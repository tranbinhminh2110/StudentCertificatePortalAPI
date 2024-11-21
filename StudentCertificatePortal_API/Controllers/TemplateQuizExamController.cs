using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Responses;
using StudentCertificatePortal_API.DTOs;
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
            var result = new List<DuplicateQuestionInfoResponse>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                result = await _service.AddQuestionsFromExcelAsync(examId, stream, new CancellationToken());
            }

            string message = $"The exam has been successfully added. {result.Count} duplicate questions were skipped.";

            return Ok(Result<List<DuplicateQuestionInfoResponse>>.Succeed(result, message));
        }
    }
}
