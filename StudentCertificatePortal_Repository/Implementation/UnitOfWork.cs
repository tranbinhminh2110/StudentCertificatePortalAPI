using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CipdbContext _context;

        private IBaseRepository<User>? _userRepository;
        private IBaseRepository<Organize>? _organizeRepository;
        private IBaseRepository<Major>? _majorRepository;
        private IBaseRepository<Course>? _courseRepository;
        private IBaseRepository<Certification>? _certificationRepository;
        private IBaseRepository<JobPosition>? _jobPositionRepository;
        private IBaseRepository<ExamSession>? _examSessionRepository;
        private IBaseRepository<Feedback>? _feedbackRepository;
        private IBaseRepository<SimulationExam>? _simulationExamRepository;
        private IBaseRepository<Question>? _questionRepository;
        private IBaseRepository<Answer>? _answerRepository;
        private IBaseRepository<CoursesEnrollment>? _courseEnrollmentRepository;
        private IBaseRepository<ExamsEnrollment>? _examEnrollmentRepository;
        private IBaseRepository<Wallet>? _walletRepository;
        private IBaseRepository<CertType>? _certTypeRepository;
        private IBaseRepository<Transaction>? _transactionRepository;
        private IBaseRepository<Voucher>? _voucherRepository;
        private IBaseRepository<Payment>? _paymentRepository;
        private IBaseRepository<StudentOfExam>? _studentOfExamRepository;
        private IBaseRepository<StudentOfCourse>? _studentOfCourseRepository;
        private IBaseRepository<Cart>? _cartRepository;
        private IBaseRepository<Score>? _scoreRepository;
        private IBaseRepository<Notification>? _notificationRepository;
        private IBaseRepository<UserAnswer>? _userAnswerRepository;

        public UnitOfWork(CipdbContext context)
        {
            _context = context;
        }
        internal CipdbContext Context => _context;

        public IBaseRepository<User> UserRepository => _userRepository ??= new UserRepository(_context);
        public IBaseRepository<Organize> OrganizeRepository => _organizeRepository ??= new OrganizeRepository(_context);
        public IBaseRepository<Major> MajorRepository => _majorRepository ??= new MajorRepository(_context);
        public IBaseRepository<Course> CourseRepository => _courseRepository ??= new CourseRepository(_context);
        public IBaseRepository<Certification> CertificationRepository => _certificationRepository ??= new CertificationRepository(_context);
        public IBaseRepository<JobPosition> JobPositionRepository => _jobPositionRepository ??= new JobPositionRepository(_context);
        public IBaseRepository<ExamSession> ExamSessionRepository => _examSessionRepository ??= new ExamSessionRepository(_context);
        public IBaseRepository<Feedback> FeedbackRepository => _feedbackRepository ??= new FeedbackRepository(_context);

        public IBaseRepository<SimulationExam> SimulationExamRepository => _simulationExamRepository ??= new SimulationExamRepository(_context);
        public IBaseRepository<CoursesEnrollment> CourseEnrollmentRepository => _courseEnrollmentRepository ??= new CourseEnrollmentRepository(_context);

        public IBaseRepository<Question> QuestionRepository => _questionRepository ??= new QuestionRepository(_context);

        public IBaseRepository<Answer> AnswerRepository => _answerRepository ??= new AnswerRepository(_context);
        public IBaseRepository<ExamsEnrollment> ExamEnrollmentRepository => _examEnrollmentRepository ??= new ExamEnrollmentRepository(_context);

        public IBaseRepository<Wallet> WalletRepository => _walletRepository ??= new WalletRepository(_context);

        public IBaseRepository<CertType> CertTypeRepository => _certTypeRepository ??= new CertTypeRepository(_context);

        public IBaseRepository<Transaction> TransactionRepository => _transactionRepository ??= new TransactionRepository(_context);
        public IBaseRepository<Voucher> VoucherRepository => _voucherRepository ??= new VoucherRepository(_context);

        public IBaseRepository<Payment> PaymentRepository => _paymentRepository ??= new PaymentRepository(_context);
        public IBaseRepository<StudentOfExam> StudentOfExamRepository => _studentOfExamRepository ??= new StudentOfExamRepository(_context);
        public IBaseRepository<StudentOfCourse> StudentOfCourseRepository => _studentOfCourseRepository ??= new StudentOfCourseRepository(_context);
        public IBaseRepository<Cart> CartRepository => _cartRepository ??= new CartRepository(_context);

        public IBaseRepository<Score> ScoreRepository => _scoreRepository ??= new ScoreRepository(_context);
        public IBaseRepository<Notification> NotificationRepository => _notificationRepository ??= new NotificationRepository(_context);
        public IBaseRepository<UserAnswer> UserAnswerRepository => _userAnswerRepository ??= new UserAnswerRepository(_context);

        public async Task Commit(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}
