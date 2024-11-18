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
        [HttpPost("upload-exam-template/{examId:int}")]
        public async Task<IActionResult> UploadExamTemplate(IFormFile file, [FromRoute] int examId)
        {
            if (file.Length <= 0)
                return BadRequest("Invalid file.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                await _service.AddQuestionsFromExcelAsync(examId, stream, new CancellationToken());
            }

            return Ok("The exam has been successfully added.");
        }
    }
}
