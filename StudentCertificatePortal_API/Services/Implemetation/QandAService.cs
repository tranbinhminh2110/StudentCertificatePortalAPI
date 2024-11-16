using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class QandAService : IQandAService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateQuestionRequest> _addQuestionValidator;
        private readonly IValidator<UpdateQuestionRequest> _updateQuestionValidator;
        public QandAService(IUnitOfWork uow, IMapper mapper, IValidator<CreateQuestionRequest> addQuestionValidator,
            IValidator<UpdateQuestionRequest> updateQuestionValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addQuestionValidator = addQuestionValidator;
            _updateQuestionValidator = updateQuestionValidator;
        }
        public async Task<QandADto> CreateQandAAsync(CreateQuestionRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addQuestionValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            bool checkIsDuplicate = await IsDuplicateQuestionAsync(request, cancellationToken);

            if (!checkIsDuplicate)
            {
                var questionEntity = new Question()
                {
                    QuestionText = request.QuestionName,
                    ExamId = request.ExamId,
                };
                var questionResult = await _uow.QuestionRepository.AddAsync(questionEntity);
                await _uow.Commit(cancellationToken);
                var result = new QandADto()
                {
                    QuestionId = questionResult.QuestionId,
                    QuestionName = questionResult.QuestionText,
                    ExamId = questionResult.ExamId,
                    Answers = new List<AnswerDto>()
                };

                foreach (var answer in request.Answers)
                {
                    var answerEntity = new Answer()
                    {
                        Text = answer.Text,
                        IsCorrect = answer.IsCorrect,
                        QuestionId = questionResult.QuestionId,
                    };
                    var answerResult = await _uow.AnswerRepository.AddAsync(answerEntity);
                    await _uow.Commit(cancellationToken);
                    result.Answers.Add(new AnswerDto()
                    {
                        AnswerId = answerResult.AnswerId,
                        Text = answerResult.Text,
                        IsCorrect = answerResult.IsCorrect
                    });

                }
                return _mapper.Map<QandADto>(result);
            }
            return null;

        }

        public async Task<QandADto> DeleteQandAAsync(int questionId, CancellationToken cancellationToken)
        {
            var question = await _uow.QuestionRepository.FirstOrDefaultAsync(x => x.QuestionId == questionId);
            if (question == null)
            {
                throw new KeyNotFoundException("Question not found!");
            }

            var answers = await _uow.AnswerRepository.WhereAsync(x => x.QuestionId == questionId);




            foreach (var answer in answers)
            {
                _uow.AnswerRepository.Delete(answer);
            }

            await _uow.Commit(cancellationToken);

            _uow.QuestionRepository.Delete(question);

            await _uow.Commit(cancellationToken);
            return _mapper.Map<QandADto>(question);
        }

        public async Task<List<QandADto>> GetAll()
        {
            var result = new List<QandADto>();
            var questions = await _uow.QuestionRepository.GetAll();
            foreach (var question in questions)
            {
                var answers = await _uow.AnswerRepository.WhereAsync(x => x.QuestionId == question.QuestionId);
                result.Add(new QandADto()
                {
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionText,
                    ExamId = question.ExamId,
                    Answers = _mapper.Map<List<AnswerDto>>(answers)
                });
            }
            return _mapper.Map<List<QandADto>>(result);
        }

        public async Task<QandADto> GetQandAByIdAsync(int questionId, CancellationToken cancellationToken)
        {
            var result = new QandADto();
            var question = await _uow.QuestionRepository.FirstOrDefaultAsync(x => x.QuestionId == questionId);
            if (question == null)
            {
                throw new KeyNotFoundException("Question not found!");
            }

            result.QuestionId = question.QuestionId;
            result.QuestionName = question.QuestionText;
            result.ExamId = question.ExamId;
            var answers = await _uow.AnswerRepository.WhereAsync(x => x.QuestionId == question.QuestionId);
            result.Answers = _mapper.Map<List<AnswerDto>>(answers);
            return _mapper.Map<QandADto>(result);
        }

        public async Task<List<QandADto>> GetQandAByNameAsync(string questionName, CancellationToken cancellationToken)
        {
            var result = new List<QandADto>();
            var questions = await _uow.QuestionRepository.WhereAsync(x => x.QuestionText.Contains(questionName));
            if (questions == null)
            {
                throw new KeyNotFoundException("Question not found!");
            }
            foreach (var question in questions)
            {
                var answers = await _uow.AnswerRepository.WhereAsync(x => x.QuestionId == question.QuestionId);
                result.Add(new QandADto()
                {
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionText,
                    ExamId = question.ExamId,
                    Answers = _mapper.Map<List<AnswerDto>>(answers)
                });
            }
            return _mapper.Map<List<QandADto>>(result);
        }

        public async Task<List<QandADto>> GetQandABySExamIdAsync(int examId, CancellationToken cancellationToken)
        {
            var result = new List<QandADto>();
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId);
            if (exam is null) throw new KeyNotFoundException("Simulation not found!");
            var questions = await _uow.QuestionRepository.WhereAsync(x => x.ExamId == examId);
            if (questions == null)
            {
                throw new KeyNotFoundException("Question not found!");
            }
            int questionCount = exam.QuestionCount ?? throw new Exception("Not enough questions available.");
            if (questions.Count() < questionCount)
            {
                throw new Exception("Not enough questions available.");
            }
            var random = new Random();
            var randomQuestions = questions
                .OrderBy(x => random.Next())
                .Take(questionCount)
                .ToList();

            foreach (var question in randomQuestions)
            {
                var answers = await _uow.AnswerRepository.WhereAsync(x => x.QuestionId == question.QuestionId);
                result.Add(new QandADto
                {
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionText,
                    ExamId = question.ExamId,
                    Answers = _mapper.Map<List<AnswerDto>>(answers)
                });
            }

            return _mapper.Map<List<QandADto>>(result);
        }

        public async Task<QandADto> UpdateQandAAsync(int questionId, UpdateQuestionRequest request, CancellationToken cancellationToken)
        {
            var result = new QandADto();
            var question = await _uow.QuestionRepository.FirstOrDefaultAsync(x => x.QuestionId == questionId);

            if (question == null)
            {
                throw new KeyNotFoundException("Question not found!");
            }

            question.QuestionText = request.QuestionName;
            question.ExamId = request.ExamId;


            _uow.QuestionRepository.Update(question);
            await _uow.Commit(cancellationToken);


            result.QuestionId = question.QuestionId;
            result.QuestionName = question.QuestionText;
            result.ExamId = question.ExamId;
            result.Answers = new List<AnswerDto>();

            var answers = await _uow.AnswerRepository.WhereAsync(x => x.QuestionId == questionId);

            foreach (var answer in answers)
            {
                _uow.AnswerRepository.Delete(answer);
            }

            await _uow.Commit(cancellationToken);

            foreach (var answerRequest in request.Answers)
            {
                var newAnswer = new Answer
                {
                    Text = answerRequest.Text,
                    IsCorrect = answerRequest.IsCorrect,
                    QuestionId = questionId
                };

                var resultAnswer = await _uow.AnswerRepository.AddAsync(newAnswer);
                await _uow.Commit(cancellationToken);
                result.Answers.Add(new AnswerDto()
                {
                    AnswerId = resultAnswer.AnswerId,
                    Text = resultAnswer.Text,
                    IsCorrect = resultAnswer.IsCorrect
                });
            }

            await _uow.Commit(cancellationToken);
            return result;
        }


        public async Task<bool> IsDuplicateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken)
        {
            var simulation = await _uow.SimulationExamRepository
                .FirstOrDefaultAsync(x => x.ExamId == request.ExamId, cancellationToken);

            if (simulation == null)
            {
                throw new Exception("Simulation not found. Question creation requires a valid CertId.");
            }

            var existingQuestion = await _uow.QuestionRepository
                .WhereAsync(q => q.ExamId == request.ExamId && q.QuestionText != null, cancellationToken, include: q => q.Include( a => a.Answers));
            var matchingQuestion = existingQuestion
                .Where(q => q.QuestionText.Trim().ToLower() == request.QuestionName.Trim().ToLower())
                .ToList();

            if (matchingQuestion.Any())
            {
                var existingAnswers = matchingQuestion.SelectMany(q => q.Answers).ToList();

                var isDuplicateAnswers = request.Answers.All(r =>
                    existingAnswers.Any(a =>
                        a.Text != null &&
                        a.Text.Trim().ToLower() == r.Text.Trim().ToLower() &&
                        a.IsCorrect == r.IsCorrect));

                if (isDuplicateAnswers)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
