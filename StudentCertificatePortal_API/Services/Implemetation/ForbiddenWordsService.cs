namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ForbiddenWordsService
    {
        private readonly List<string> _forbiddenWords;

        public ForbiddenWordsService()
        {
            _forbiddenWords = new List<string>
        {
        "điên","cặn bã","mất dạy","súc vật","khốn nạn","hèn","đểu","đê tiện","bẩn thỉu","sỉ nhục","kém cỏi","hạ đẳng","chó","mất nết","rác rưởi","phế phẩm","bỉ ổi","điếm","hư hỏng","khốn","ngu",
        "cẩu","địt","djt","dit","fuck","dick","dmm","cặc","lồn","biến","cút","bitch","óc","cc","FUCK","DMM","Fuck","Chó","Ngu","NGU"
        };
        }
        public bool ContainsForbiddenWords(string feedbackDescription)
        {
            if (string.IsNullOrEmpty(feedbackDescription)) return false;

            return _forbiddenWords.Any(word => feedbackDescription.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}
