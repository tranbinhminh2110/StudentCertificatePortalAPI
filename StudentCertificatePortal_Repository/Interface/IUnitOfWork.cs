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

        Task Commit(CancellationToken cancellationToken);
    }
}
