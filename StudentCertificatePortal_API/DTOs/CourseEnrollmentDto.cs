using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class CourseEnrollmentDto : IMapFrom<CoursesEnrollment>
    {
        public int CourseEnrollmentId { get; set; }

        public DateTime? CourseEnrollmentDate { get; set; }

        public string? CourseEnrollmentStatus { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }
        public string? EnrollCode { get; set; }

        public List<CourseDetailsDto> CourseDetails { get; set; } = new List<CourseDetailsDto>();

    }
}
