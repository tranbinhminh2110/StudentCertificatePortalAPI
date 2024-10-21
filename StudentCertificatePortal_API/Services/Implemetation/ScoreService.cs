using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ScoreService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public Task<ScoreDto> Scoring(UserAnswerRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        /*public async Task<ScoreDto> Scoring(UserAnswerRequest request, CancellationToken cancellationToken)
{
   int countQuestionCorrect = 0;
   var numberQuestion = request.QuestionRequests.Count;
   foreach(var model in request.QuestionRequests)
   {
       bool checkQuestion =await CheckAnswerCorrect(model.QuestionId, model.UserAnswerId, cancellationToken);
       if (checkQuestion)
       {
           countQuestionCorrect++;
       }
   }
   float finalScore = countQuestionCorrect * (10f/numberQuestion);

   var scoreEntity = new Score()
   {
       UserId = request.UserId,
       ExamId = request.ExamId,
       ScoreValue = (decimal)finalScore,
   };

   var result = await _uow.ScoreRepository.AddAsync(scoreEntity);
   await _uow.Commit(cancellationToken);

   return _mapper.Map<ScoreDto>(result);
}

public async Task<bool> CheckAnswerCorrect (int questionId,  List<int> answerId, CancellationToken cancellationToken) {
   var question = await _uow.QuestionRepository.FirstOrDefaultAsync(x => x.QuestionId == questionId,
       cancellationToken,
       include: i => i.Include(a => a.Answers));

   if (question != null)
   {
       List<int> answerCorrectIds = new List<int>();
       foreach(var answer in question.Answers) {
           if(answer.IsCorrect)
           {
               answerCorrectIds.Add(answer.AnswerId);
           }
       }
       if(answerCorrectIds.Count == answerId.Count)
       {
           if (answerId.Count == 1)
           {
               if (answerId[0] == answerCorrectIds[0])
               {
                   return true;
               }
               return false;
           }else if(answerId.Count > 1)
           {
               int count = 0;
               for(int i = 0; i < answerId.Count; i++)
               {
                   foreach(var ansCorrect in answerCorrectIds)
                   {
                       if (answerId[0] == ansCorrect)
                       {
                           count++;
                       }
                   }
               }
               if(count == answerCorrectIds.Count)
               {
                   return true;
               }else
               {
                   return false;
               }
           }
       } 
       return false;
   }
   return false ;
}*/
    }
}
