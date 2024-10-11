using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StudentCertificatePortal_API.Filters.ActionFilters;
using StudentCertificatePortal_API.Middlewares;
using StudentCertificatePortal_API.Policies;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Implementation;
using StudentCertificatePortal_Repository.Interface;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<CipdbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("CIPDB")));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie() 
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/sign-in-google"; 
});


// Thêm chính sách CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
           builder => builder.WithOrigins("http://localhost:3000")
                             .AllowCredentials()
                             .AllowAnyHeader()
                             .AllowAnyMethod());
});

//Add Validation && not use "ModelStateInvalidFilter"

builder.Services.Configure<ApiBehaviorOptions>(opts =>
{
    opts.SuppressModelStateInvalidFilter = true;
});
// Add filter "ValidateRequestFilter"

builder.Services.AddControllersWithViews(opts =>
{
    opts.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

    // Set order very high to allow other filters to include their own validation results.
    opts.Filters.Add<ValidateRequestFilter>(int.MaxValue - 100);
});

// Primary services
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddTransient<GenerateJSONWebTokenHelper>();
builder.Services.AddTransient<GenerateOTP>();

// Add a implementation "Repositories"
builder.Services.AddTransient<IBaseRepository<User>, UserRepository>();
builder.Services.AddTransient<IBaseRepository<Organize>, OrganizeRepository>();
builder.Services.AddTransient<IBaseRepository<Major>, MajorRepository>();
builder.Services.AddTransient<IBaseRepository<Course>, CourseRepository>();
builder.Services.AddTransient<IBaseRepository<Certification>, CertificationRepository>();
builder.Services.AddTransient<IBaseRepository<JobPosition>, JobPositionRepository>();
builder.Services.AddTransient<IBaseRepository<ExamSession>, ExamSessionRepository>();
builder.Services.AddTransient<IBaseRepository<Feedback>, FeedbackRepository>();
builder.Services.AddTransient<IBaseRepository<SimulationExam>, SimulationExamRepository>();
builder.Services.AddTransient<IBaseRepository<JobCert>, JobCertRepository>();
builder.Services.AddTransient<IBaseRepository<Question>, QuestionRepository>();
builder.Services.AddTransient<IBaseRepository<Answer>, AnswerRepository>();
builder.Services.AddTransient<IBaseRepository<CoursesEnrollment>, CourseEnrollmentRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();



// Add a implement "Service"
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IOrganizeService, OrganizeService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IMajorService, MajorService>();
builder.Services.AddScoped<ICertificationService, CertificationService>();
builder.Services.AddScoped<IJobPositionService, JobPositionService>();
builder.Services.AddScoped<IExamSessionService, ExamSessionService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ISimulationExamService, SimulationExamService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IRedisService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var redisConnectionString = configuration["ConnectionStrings:Redis"];
    return new RedisService(redisConnectionString);
});


// Middlewares & Filters
builder.Services.AddScoped<ExceptionMiddleware>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Add JwtTokenBearer Support to Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentCertificatePortal API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    };

    c.AddSecurityRequirement(securityRequirement);

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudentCertidicatePortal v1");

        c.RoutePrefix = string.Empty;
        c.EnableTryItOutByDefault();
    });
    app.UseDeveloperExceptionPage(); // Only for development
    
}
else
{
    app.UseExceptionHandler("/error"); // Use custom error page in production
}


app.UseRouting();
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
