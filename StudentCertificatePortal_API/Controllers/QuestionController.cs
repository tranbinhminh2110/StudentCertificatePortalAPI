using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class QuestionController: ApiControllerBase
    {
        private readonly IQandAService _service;
        public QuestionController(IQandAService service)
        {
            _service = service;
        }


        [HttpGet("{questionId:int}")]
        public async Task<ActionResult<Result<QandADto>>> GetQuestionById([FromRoute] int questionId)
        {
            var result = await _service.GetQandAByIdAsync(questionId, new CancellationToken());
            return Ok(Result<QandADto>.Succeed(result));
        }

        [HttpGet("get-by-exam/{examId:int}")]
        public async Task<ActionResult<Result<List<QandADto>>>> GetQuestionByExamId([FromRoute] int examId)
        {
            var result = await _service.GetQandABySExamIdAsync(examId, new CancellationToken());
            return Ok(Result<List<QandADto>>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<QandADto>>> CreateQandA([FromBody] CreateQuestionRequest request)
        {
            var result = await _service.CreateQandAAsync(request, new CancellationToken());
            if (result == null)
            {
                throw new Exception("Failed to add the question. Please check if the question already exists or if there's an issue with the data provided.");
            }
            return Ok(Result<QandADto>.Succeed(result));
        }

        [HttpPut("{questionId:int}")]
        public async Task<ActionResult<Result<QandADto>>> UpdateQuestion(int questionId, UpdateQuestionRequest request)
        {
            var result = await _service.UpdateQandAAsync(questionId, request, new CancellationToken());
            return Ok(Result<QandADto>.Succeed(result));
        }
        [HttpDelete("{questionId:int}")]
        public async Task<ActionResult<Result<QandADto>>> DeleteQuestionById([FromRoute] int questionId)
        {
            var result = await _service.DeleteQandAAsync(questionId, new CancellationToken());
            return Ok(Result<QandADto>.Succeed(result));
        }

        [HttpGet("~/api/v1/[controller]/search")]
        public async Task<ActionResult<Result<List<QandADto>>>> SearchQuestionByName([FromQuery] string? questionName = null)
        {
            IEnumerable<QandADto> result;
            if (questionName == null)
            {
                result = await _service.GetAll();
            }
            else
            {
                result = await _service.GetQandAByNameAsync(questionName, new CancellationToken());
            }

            return Ok(Result<List<QandADto>>.Succeed(result.ToList()));
        }
    }
}
