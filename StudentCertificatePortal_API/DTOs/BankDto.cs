namespace StudentCertificatePortal_API.DTOs
{
    public class BankDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Bin { get; set; }
        public string ShortName { get; set; }
        public string LogoUrl { get; set; }
        public string IconUrl { get; set; }
        public string SwiftCode { get; set; }
        public int LookupSupported { get; set; }
    }
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public bool Success { get; set; }
        public List<T> Data { get; set; }
    }
}
