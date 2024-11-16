using Microsoft.EntityFrameworkCore;
using Quartz;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Jobs
{
    public class RemoveExpiredExamsJob : IJob
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<RemoveExpiredExamsJob> _logger;

        public RemoveExpiredExamsJob(IUnitOfWork uow, ILogger<RemoveExpiredExamsJob> logger)
        {
            _uow = uow;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Starting the RemoveExpiredExamsJob job at {Time}", DateTime.Now);
                await RemoveExpiredExamsAsync(new CancellationToken());
                _logger.LogInformation("Completed the RemoveExpiredExamsJob job at {Time}", DateTime.Now);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing RemoveExpiredExamsJob");
                throw;
            }
        }
        public async Task RemoveExpiredExamsAsync(CancellationToken cancellationToken)
        {
            var currentDate = DateTime.Now;
            var expiredExams = await _uow.StudentOfExamRepository.WhereAsync(se => se.ExpiryDate < currentDate,cancellationToken, include: q => q.Include(c => c.Exam)
            .Include(c => c.Enrollment));


            if (expiredExams.Any())
            {
                _logger.LogInformation("{Count} expired exams found. Deleting...", expiredExams.Count());

                foreach (var expiredExam in expiredExams)
                {

                    
                    var eEnrollment = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(x => x.ExamEnrollmentId == expiredExam.EnrollmentId);
                    eEnrollment.ExamEnrollmentStatus = Enums.EnumExamEnrollment.Expired.ToString();
                    _uow.ExamEnrollmentRepository.Update(eEnrollment);
                    await _uow.Commit(cancellationToken);
                    _uow.StudentOfExamRepository.Delete(expiredExam);
                    await _uow.Commit(cancellationToken);
                    
                }
                await _uow.Commit(cancellationToken);
                _logger.LogInformation("Expired exams have been deleted.");
            }
            else
            {
                _logger.LogInformation("No expired exams found to delete.");
            }
        }
    }
}
