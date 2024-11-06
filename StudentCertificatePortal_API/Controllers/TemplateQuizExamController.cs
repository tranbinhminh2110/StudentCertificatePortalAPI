using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class TemplateQuizExamController : ApiControllerBase
    {
        private readonly ITemplateService _service;

        public TemplateQuizExamController(ITemplateService service) { _service = service; }
        [HttpGet("download-exam-template")]
        public IActionResult DownloadExamTemplate()
        {
            
            var fileContents = _service.GenerateExamTemplate();

            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExamTemplate.xlsx");
        }
        [HttpPost("upload-exam-template")]
        public async Task<IActionResult> UploadExamTemplate(IFormFile file)
        {
            if (file.Length <= 0)
                return BadRequest("File không hợp lệ.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                await _service.AddQuestionsFromExcelAsync(stream, new CancellationToken());
            }

            return Ok("Đề thi đã được thêm thành công.");
        }
    }
}
