namespace StudentCertificatePortal_API.DTOs
{
    public class CourseDetailsDto
    {
        public int CourseId { get; set; }

        public string? CourseName { get; set; }

        public string? CourseCode { get; set; }
        public string? CourseTime { get; set; }

        public int? CourseFee { get; set; }
        public int? CourseDiscountFee { get; set; }
        public string? CourseImage { get; set; }


    }
}
