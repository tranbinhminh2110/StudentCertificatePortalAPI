using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StudentCertificatePortal_Data.Models;

public partial class CipdbContext : DbContext
{
    public CipdbContext()
    {
    }

    public CipdbContext(DbContextOptions<CipdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CertType> CertTypes { get; set; }

    public virtual DbSet<Certification> Certifications { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CoursesEnrollment> CoursesEnrollments { get; set; }

    public virtual DbSet<ExamSession> ExamSessions { get; set; }

    public virtual DbSet<ExamsEnrollment> ExamsEnrollments { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<JobCert> JobCerts { get; set; }

    public virtual DbSet<JobPosition> JobPositions { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<Organize> Organizes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<SimulationExam> SimulationExams { get; set; }

    public virtual DbSet<StudentOfCourse> StudentOfCourses { get; set; }

    public virtual DbSet<StudentOfExam> StudentOfExams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-HHD16EI;Initial Catalog=CIPDB;User ID=sa;Password=1234567890;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__2EF52A27C8EC2F4E");

            entity.ToTable("Cart");

            entity.HasIndex(e => e.UserId, "UQ__Cart__B9BE370EAA6DB139").IsUnique();

            entity.Property(e => e.CartId)
                .ValueGeneratedNever()
                .HasColumnName("cart_id");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .HasConstraintName("FK__Cart__user_id__52593CB8");

            entity.HasMany(d => d.Exams).WithMany(p => p.Carts)
                .UsingEntity<Dictionary<string, object>>(
                    "CartDetail",
                    r => r.HasOne<SimulationExam>().WithMany()
                        .HasForeignKey("ExamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Cart_Deta__exam___5535A963"),
                    l => l.HasOne<Cart>().WithMany()
                        .HasForeignKey("CartId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Cart_Deta__cart___5441852A"),
                    j =>
                    {
                        j.HasKey("CartId", "ExamId").HasName("PK__Cart_Det__A73DED99AF524B10");
                        j.ToTable("Cart_Detail");
                        j.IndexerProperty<int>("CartId").HasColumnName("cart_id");
                        j.IndexerProperty<int>("ExamId").HasColumnName("exam_id");
                    });
        });

        modelBuilder.Entity<CertType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Cert_Typ__2C000598DD06AD09");

            entity.ToTable("Cert_Types");

            entity.Property(e => e.TypeId)
                .ValueGeneratedNever()
                .HasColumnName("type_id");
            entity.Property(e => e.TypeCode)
                .HasMaxLength(1)
                .HasColumnName("type_code");
            entity.Property(e => e.TypeName)
                .HasMaxLength(1)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasKey(e => e.CertId).HasName("PK__Certific__024B46EC0E7C712F");

            entity.Property(e => e.CertId)
                .ValueGeneratedNever()
                .HasColumnName("cert_id");
            entity.Property(e => e.CertCode)
                .HasMaxLength(1)
                .HasColumnName("cert_code");
            entity.Property(e => e.CertCost).HasColumnName("cert_cost");
            entity.Property(e => e.CertDescription)
                .HasMaxLength(1)
                .HasColumnName("cert_description");
            entity.Property(e => e.CertImage)
                .HasColumnType("text")
                .HasColumnName("cert_image");
            entity.Property(e => e.CertName)
                .HasMaxLength(1)
                .HasColumnName("cert_name");
            entity.Property(e => e.CertPointSystem)
                .HasMaxLength(1)
                .HasColumnName("cert_point_system");
            entity.Property(e => e.CertPrerequisite)
                .HasMaxLength(1)
                .HasColumnName("cert_prerequisite");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.OrganizeId).HasColumnName("organize_id");
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.HasOne(d => d.Organize).WithMany(p => p.Certifications)
                .HasForeignKey(d => d.OrganizeId)
                .HasConstraintName("FK__Certifica__organ__619B8048");

            entity.HasOne(d => d.Type).WithMany(p => p.Certifications)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK__Certifica__type___5FB337D6");

            entity.HasMany(d => d.CertIdTwos).WithMany(p => p.Certs)
                .UsingEntity<Dictionary<string, object>>(
                    "CertCert",
                    r => r.HasOne<Certification>().WithMany()
                        .HasForeignKey("CertIdTwo")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Cert_Cert__cert___6754599E"),
                    l => l.HasOne<Certification>().WithMany()
                        .HasForeignKey("CertId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Cert_Cert__cert___66603565"),
                    j =>
                    {
                        j.HasKey("CertId", "CertIdTwo").HasName("PK__Cert_Cer__9FBF828F9AE5A511");
                        j.ToTable("Cert_Cert");
                        j.IndexerProperty<int>("CertId").HasColumnName("cert_id");
                        j.IndexerProperty<int>("CertIdTwo").HasColumnName("cert_id_two");
                    });

            entity.HasMany(d => d.Certs).WithMany(p => p.CertIdTwos)
                .UsingEntity<Dictionary<string, object>>(
                    "CertCert",
                    r => r.HasOne<Certification>().WithMany()
                        .HasForeignKey("CertId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Cert_Cert__cert___66603565"),
                    l => l.HasOne<Certification>().WithMany()
                        .HasForeignKey("CertIdTwo")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Cert_Cert__cert___6754599E"),
                    j =>
                    {
                        j.HasKey("CertId", "CertIdTwo").HasName("PK__Cert_Cer__9FBF828F9AE5A511");
                        j.ToTable("Cert_Cert");
                        j.IndexerProperty<int>("CertId").HasColumnName("cert_id");
                        j.IndexerProperty<int>("CertIdTwo").HasColumnName("cert_id_two");
                    });
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__8F1EF7AEF670FDBF");

            entity.Property(e => e.CourseId)
                .ValueGeneratedNever()
                .HasColumnName("course_id");
            entity.Property(e => e.CertId).HasColumnName("cert_id");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(1)
                .HasColumnName("course_code");
            entity.Property(e => e.CourseDescription)
                .HasMaxLength(1)
                .HasColumnName("course_description");
            entity.Property(e => e.CourseName)
                .HasMaxLength(1)
                .HasColumnName("course_name");
            entity.Property(e => e.CourseTime)
                .HasMaxLength(255)
                .HasColumnName("course_time");

            entity.HasOne(d => d.Cert).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CertId)
                .HasConstraintName("FK__Courses__cert_id__68487DD7");
        });

        modelBuilder.Entity<CoursesEnrollment>(entity =>
        {
            entity.HasKey(e => e.CourseEnrollmentId).HasName("PK__Courses___88BB3C79962BBD33");

            entity.ToTable("Courses_Enrollment");

            entity.Property(e => e.CourseEnrollmentId)
                .ValueGeneratedNever()
                .HasColumnName("course_enrollment_id");
            entity.Property(e => e.CourseEnrollmentDate)
                .HasColumnType("datetime")
                .HasColumnName("course_enrollment_date");
            entity.Property(e => e.CourseEnrollmentStatus)
                .HasMaxLength(255)
                .HasColumnName("course_enrollment_status");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.CoursesEnrollments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Courses_E__user___6B24EA82");
        });

        modelBuilder.Entity<ExamSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__Exam_Ses__69B13FDCCEA655DC");

            entity.ToTable("Exam_Sessions");

            entity.Property(e => e.SessionId)
                .ValueGeneratedNever()
                .HasColumnName("session_id");
            entity.Property(e => e.CertId).HasColumnName("cert_id");
            entity.Property(e => e.SessionAddress)
                .HasMaxLength(1)
                .HasColumnName("session_address");
            entity.Property(e => e.SessionCode)
                .HasMaxLength(1)
                .HasColumnName("session_code");
            entity.Property(e => e.SessionCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("session_createdAt");
            entity.Property(e => e.SessionDate)
                .HasColumnType("datetime")
                .HasColumnName("session_date");
            entity.Property(e => e.SessionName)
                .HasMaxLength(1)
                .HasColumnName("session_name");

            entity.HasOne(d => d.Cert).WithMany(p => p.ExamSessions)
                .HasForeignKey(d => d.CertId)
                .HasConstraintName("FK__Exam_Sess__cert___60A75C0F");
        });

        modelBuilder.Entity<ExamsEnrollment>(entity =>
        {
            entity.HasKey(e => e.ExamEnrollmentId).HasName("PK__Exams_En__225496D52ACDA18E");

            entity.ToTable("Exams_Enrollment");

            entity.Property(e => e.ExamEnrollmentId)
                .ValueGeneratedNever()
                .HasColumnName("exam_enrollment_id");
            entity.Property(e => e.ExamEnrollmentDate)
                .HasColumnType("datetime")
                .HasColumnName("exam_enrollment_date");
            entity.Property(e => e.ExamEnrollmentStatus)
                .HasMaxLength(255)
                .HasColumnName("exam_enrollment_status");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ExamsEnrollments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Exams_Enr__user___5812160E");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__7A6B2B8C424CDE45");

            entity.Property(e => e.FeedbackId)
                .ValueGeneratedNever()
                .HasColumnName("feedback_id");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.FeedbackCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("feedback_createdAt");
            entity.Property(e => e.FeedbackDescription)
                .HasMaxLength(1)
                .HasColumnName("feedback_description");
            entity.Property(e => e.FeedbackImage)
                .HasColumnType("text")
                .HasColumnName("feedback_image");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Exam).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__Feedbacks__exam___571DF1D5");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Feedbacks__user___5629CD9C");
        });

        modelBuilder.Entity<JobCert>(entity =>
        {
            entity.HasKey(e => new { e.CertId, e.JobPositionId }).HasName("PK__Job_Cert__6266A6D88AD79F92");

            entity.ToTable("Job_Cert");

            entity.Property(e => e.CertId).HasColumnName("cert_id");
            entity.Property(e => e.JobPositionId).HasColumnName("job_position_id");

            entity.HasOne(d => d.Cert).WithMany(p => p.JobCerts)
                .HasForeignKey(d => d.CertId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Job_Cert__cert_i__628FA481");

            entity.HasOne(d => d.CertNavigation).WithMany(p => p.JobCerts)
                .HasForeignKey(d => d.CertId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Job_Cert__cert_i__6383C8BA");
        });

        modelBuilder.Entity<JobPosition>(entity =>
        {
            entity.HasKey(e => e.JobPositionId).HasName("PK__Job_Posi__02DE03475E081E95");

            entity.ToTable("Job_Positions");

            entity.Property(e => e.JobPositionId)
                .ValueGeneratedNever()
                .HasColumnName("job_position_id");
            entity.Property(e => e.JobPositionCode)
                .HasMaxLength(1)
                .HasColumnName("job_position_code");
            entity.Property(e => e.JobPositionDescription)
                .HasMaxLength(1)
                .HasColumnName("job_position_description");
            entity.Property(e => e.JobPositionName)
                .HasMaxLength(1)
                .HasColumnName("job_position_name");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasKey(e => e.MajorId).HasName("PK__Majors__DC7AC3C42401D665");

            entity.Property(e => e.MajorId)
                .ValueGeneratedNever()
                .HasColumnName("major_id");
            entity.Property(e => e.MajorCode)
                .HasMaxLength(255)
                .HasColumnName("major_code");
            entity.Property(e => e.MajorDescription)
                .HasMaxLength(1)
                .HasColumnName("major_description");
            entity.Property(e => e.MajorName)
                .HasMaxLength(1)
                .HasColumnName("major_name");

            entity.HasMany(d => d.JobPositions).WithMany(p => p.Majors)
                .UsingEntity<Dictionary<string, object>>(
                    "MajorPosition",
                    r => r.HasOne<JobPosition>().WithMany()
                        .HasForeignKey("JobPositionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Major_Pos__job_p__6477ECF3"),
                    l => l.HasOne<Major>().WithMany()
                        .HasForeignKey("MajorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Major_Pos__major__656C112C"),
                    j =>
                    {
                        j.HasKey("MajorId", "JobPositionId").HasName("PK__Major_Po__BC5723F0D8B0DE25");
                        j.ToTable("Major_Position");
                        j.IndexerProperty<int>("MajorId").HasColumnName("major_id");
                        j.IndexerProperty<int>("JobPositionId").HasColumnName("job_position_id");
                    });
        });

        modelBuilder.Entity<Organize>(entity =>
        {
            entity.HasKey(e => e.OrganizeId).HasName("PK__Organize__C5D74862BBA60ED3");

            entity.ToTable("Organize");

            entity.Property(e => e.OrganizeId)
                .ValueGeneratedNever()
                .HasColumnName("organize_id");
            entity.Property(e => e.OrganizeAddress)
                .HasMaxLength(1)
                .HasColumnName("organize_address");
            entity.Property(e => e.OrganizeContact)
                .HasMaxLength(1)
                .HasColumnName("organize_contact");
            entity.Property(e => e.OrganizeName)
                .HasMaxLength(1)
                .HasColumnName("organize_name");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__ED1FC9EAFB8FE9AD");

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("payment_id");
            entity.Property(e => e.CourseEnrollmentId).HasColumnName("course_enrollment_id");
            entity.Property(e => e.ExamEnrollmentId).HasColumnName("exam_enrollment_id");
            entity.Property(e => e.PaymentAmount).HasColumnName("payment_amount");
            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(255)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(255)
                .HasColumnName("payment_status");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.CourseEnrollment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.CourseEnrollmentId)
                .HasConstraintName("FK__Payments__course__6C190EBB");

            entity.HasOne(d => d.ExamEnrollment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ExamEnrollmentId)
                .HasConstraintName("FK__Payments__exam_e__5BE2A6F2");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("FK__Payments__wallet__5AEE82B9");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC2154972AFD824");

            entity.Property(e => e.QuestionId)
                .ValueGeneratedNever()
                .HasColumnName("question_id");
            entity.Property(e => e.CorrectAnswer).HasColumnName("correct_answer");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.QuestionAnswer)
                .HasMaxLength(1)
                .HasColumnName("question_answer");
            entity.Property(e => e.QuestionName)
                .HasMaxLength(1)
                .HasColumnName("question_name");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__Questions__exam___6D0D32F4");
        });

        modelBuilder.Entity<SimulationExam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Simulati__9C8C7BE9A7DB6CDA");

            entity.ToTable("Simulation_Exams");

            entity.Property(e => e.ExamId)
                .ValueGeneratedNever()
                .HasColumnName("exam_id");
            entity.Property(e => e.CertId).HasColumnName("cert_id");
            entity.Property(e => e.ExamCode)
                .HasMaxLength(1)
                .HasColumnName("exam_code");
            entity.Property(e => e.ExamDescription)
                .HasMaxLength(1)
                .HasColumnName("exam_description");
            entity.Property(e => e.ExamDiscountFee).HasColumnName("exam_discount_fee");
            entity.Property(e => e.ExamFee).HasColumnName("exam_fee");
            entity.Property(e => e.ExamImage)
                .HasColumnType("text")
                .HasColumnName("exam_image");
            entity.Property(e => e.ExamName)
                .HasMaxLength(1)
                .HasColumnName("exam_name");

            entity.HasOne(d => d.Cert).WithMany(p => p.SimulationExams)
                .HasForeignKey(d => d.CertId)
                .HasConstraintName("FK__Simulatio__cert___5EBF139D");

            entity.HasMany(d => d.Vouchers).WithMany(p => p.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "VoucherOfExam",
                    r => r.HasOne<Voucher>().WithMany()
                        .HasForeignKey("VoucherId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Voucher_O__vouch__5DCAEF64"),
                    l => l.HasOne<SimulationExam>().WithMany()
                        .HasForeignKey("ExamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Voucher_O__exam___5CD6CB2B"),
                    j =>
                    {
                        j.HasKey("ExamId", "VoucherId").HasName("PK__Voucher___04871413E1F77568");
                        j.ToTable("Voucher_Of_Exam");
                        j.IndexerProperty<int>("ExamId").HasColumnName("exam_id");
                        j.IndexerProperty<int>("VoucherId").HasColumnName("voucher_id");
                    });
        });

        modelBuilder.Entity<StudentOfCourse>(entity =>
        {
            entity.HasKey(e => new { e.CouseEnrollmentId, e.CourseId }).HasName("PK__Student___793235F2D5670AB1");

            entity.ToTable("Student_Of_Course");

            entity.Property(e => e.CouseEnrollmentId).HasColumnName("couse_enrollment_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("creation_date");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Course).WithMany(p => p.StudentOfCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_O__cours__693CA210");

            entity.HasOne(d => d.CouseEnrollment).WithMany(p => p.StudentOfCourses)
                .HasForeignKey(d => d.CouseEnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_O__couse__6A30C649");
        });

        modelBuilder.Entity<StudentOfExam>(entity =>
        {
            entity.HasKey(e => new { e.EnrollmentId, e.ExamId }).HasName("PK__Student___E4EC6DC40C75B221");

            entity.ToTable("Student_Of_Exam");

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("creation_date");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.StudentOfExams)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_O__enrol__59063A47");

            entity.HasOne(d => d.Exam).WithMany(p => p.StudentOfExams)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student_O__exam___59FA5E80");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FEC5D4FD4");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(1)
                .HasColumnName("address");
            entity.Property(e => e.Dob)
                .HasColumnType("datetime")
                .HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(1)
                .HasColumnName("fullname");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.Role)
                .HasMaxLength(255)
                .HasColumnName("role");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("user_createdAt");
            entity.Property(e => e.UserImage)
                .HasColumnType("text")
                .HasColumnName("user_image");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__80B6FFA8B58A01DF");

            entity.Property(e => e.VoucherId)
                .ValueGeneratedNever()
                .HasColumnName("voucher_id");
            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("creation_date");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.Percentage).HasColumnName("percentage");
            entity.Property(e => e.VoucherDescription)
                .HasMaxLength(1)
                .HasColumnName("voucher_description");
            entity.Property(e => e.VoucherName)
                .HasMaxLength(1)
                .HasColumnName("voucher_name");
            entity.Property(e => e.VoucherStatus).HasColumnName("voucher_status");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__Wallet__0EE6F04157411278");

            entity.ToTable("Wallet");

            entity.HasIndex(e => e.UserId, "UQ__Wallet__B9BE370E62A41689").IsUnique();

            entity.Property(e => e.WalletId)
                .ValueGeneratedNever()
                .HasColumnName("wallet_id");
            entity.Property(e => e.DepositDate)
                .HasColumnType("datetime")
                .HasColumnName("deposit_date");
            entity.Property(e => e.History)
                .HasMaxLength(255)
                .HasColumnName("history");
            entity.Property(e => e.Point).HasColumnName("point");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WalletStatus)
                .HasMaxLength(255)
                .HasColumnName("wallet_status");

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .HasConstraintName("FK__Wallet__user_id__534D60F1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
