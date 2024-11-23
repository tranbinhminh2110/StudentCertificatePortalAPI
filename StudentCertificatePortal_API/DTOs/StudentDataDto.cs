namespace StudentCertificatePortal_API.DTOs
{
    public class StudentDataDto
    {
        public int TotalStudents { get; set; }
        public double PercentageTotalStudents { get; set; }
        public int OnlyEnrolledInCourse { get; set; }
        public double PercentageOnlyEnrolledInCourse { get; set; }
        public int OnlyPurchaseSimulationExams { get; set; }
        public double PercentageOnlyPurchaseSimulationExams { get; set; }
        public int PurchaseBoth {  get; set; }
        public double PercentagePurchaseBoth { get; set; }
        public int PurchaseNothing { get; set; }
        public double PercentagePurchaseNothing { get; set; }
    }
}
