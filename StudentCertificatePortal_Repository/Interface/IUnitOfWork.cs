using StudentCertificatePortal_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Interface
{
    public interface IUnitOfWork
    {
        IBaseRepository<User> UserRepository { get; }
        IBaseRepository<Organize> OrganizeRepository { get; }
        IBaseRepository<Major> MajorRepository { get; }
        IBaseRepository<Course> CourseRepository { get; }
        IBaseRepository<Certification> CertificationRepository { get; }
        IBaseRepository<JobPosition> JobPositionRepository { get; }
        IBaseRepository<ExamSession> ExamSessionRepository { get; }
        IBaseRepository<Feedback> FeedbackRepository { get; }
        IBaseRepository<SimulationExam> SimulationExamRepository { get; }
        IBaseRepository<Question> QuestionRepository { get; }
        IBaseRepository<Answer> AnswerRepository{ get; }
        IBaseRepository<CoursesEnrollment> CourseEnrollmentRepository { get; }
        IBaseRepository<ExamsEnrollment> ExamEnrollmentRepository { get; }
        IBaseRepository<Wallet> WalletRepository { get; }
        IBaseRepository<CertType> CertTypeRepository { get; }

        IBaseRepository<Transaction> TransactionRepository { get; }
        IBaseRepository<Voucher> VoucherRepository { get; }
        IBaseRepository<Payment> PaymentRepository { get; }

        IBaseRepository<StudentOfExam> StudentOfExamRepository { get; }

        IBaseRepository<StudentOfCourse> StudentOfCourseRepository { get; }
        IBaseRepository<Cart> CartRepository { get; }

        Task Commit(CancellationToken cancellationToken);
    }
}
