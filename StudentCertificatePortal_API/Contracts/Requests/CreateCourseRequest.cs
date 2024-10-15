namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateCourseRequest
    {
        public string? CourseName { get; set; }

        public string? CourseCode { get; set; }

        public string? CourseTime { get; set; }

        public string? CourseDescription { get; set; }
        public int? CourseFee { get; set; }
        public int? CourseDiscountFee { get; set; }

        public string? CourseImage { get; set; }

        public int? CertId { get; set; }
    }
}
