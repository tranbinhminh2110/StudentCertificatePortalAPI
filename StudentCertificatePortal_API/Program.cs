using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Implementation;
using StudentCertificatePortal_Repository.Interface;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<CipdbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("CIPDB")));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Primary services
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());


// Add a implementation "Repositories"
builder.Services.AddTransient<IBaseRepository<User>, UserRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();


// Add a implement "Service"
builder.Services.AddScoped<IUserService, UserService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
