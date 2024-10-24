using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Threading;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class SimulationExamService: ISimulationExamService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IVoucherService _voucherService;

        private readonly IValidator<CreateSimulationExamRequest> _addSimulationExamValidator;
        private readonly IValidator<UpdateSimulationExamRequest> _updateSimulationExamValidator;

        public SimulationExamService(IUnitOfWork uow, IMapper mapper, 
            IValidator<CreateSimulationExamRequest> addSimulationExamValidator,
            IValidator<UpdateSimulationExamRequest> updateSimulationExamValidator,
            IVoucherService voucherService)
        {
            _uow = uow;
            _mapper = mapper;
            _addSimulationExamValidator = addSimulationExamValidator;
            _updateSimulationExamValidator = updateSimulationExamValidator;
            _voucherService = voucherService;
        }

        public async Task<SimulationExamDto> CreateSimulationExamAsync(CreateSimulationExamRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addSimulationExamValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == request.CertId, cancellationToken);

            if (certification == null)
            {
                throw new Exception("Certification not found. Simulate creation requires a valid CertId.");
            }

            var exam = new SimulationExam()
            {
                ExamName = request.ExamName,
                ExamCode = request.ExamCode,
                ExamDescription = request.ExamDescription,
                ExamFee = request.ExamFee,
                ExamImage = request.ExamImage,
                CertId = request.CertId,
            };

            var result = await _uow.SimulationExamRepository.AddAsync(exam);
            await _uow.Commit(cancellationToken);

            float? totalDiscount = 1;

            foreach (var voucherId in request.VoucherIds)
            {
                if(voucherId > 0)
                {
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher is null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException("Voucher not found or is expired.");
                    }

                    var existingVoucher = await _uow.VoucherRepository.FirstOrDefaultAsync(v => v.VoucherId == voucherId, cancellationToken);

                    if (existingVoucher != null)
                    {
                        result.Vouchers.Add(existingVoucher);
                    }
                    else
                    {
                        result.Vouchers.Add(_mapper.Map<Voucher>(voucher));
                    }

                    totalDiscount = totalDiscount * (1 - voucher.Percentage / 100f);
                }
            }

            result.ExamDiscountFee = (int?)(result.ExamFee.Value * totalDiscount);

            _uow.SimulationExamRepository.Update(result);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<SimulationExamDto>(result);
        }


        public async Task<List<SimulationExamDto>> GetAll()
        {
            var result = await _uow.SimulationExamRepository.GetAll();
            return _mapper.Map<List<SimulationExamDto>>(result);
        }

        public async Task<SimulationExamDto> GetSimulationExamByIdAsync(int examId, CancellationToken cancellationToken)
        {
            var simulation = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId, cancellationToken);
            if (simulation is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }

            var questions = await _uow.QuestionRepository.WhereAsync(x => x.ExamId == simulation.ExamId
            , cancellationToken
            , include: i => i.Include(ans => ans.Answers));
            var result = new SimulationExamDto();
            result.ExamId = simulation.ExamId;
            result.ExamName = simulation.ExamName;
            result.ExamCode = simulation.ExamCode;
            result.CertId = simulation.CertId;
            result.ExamDescription = simulation.ExamDescription;
            result.ExamFee = simulation.ExamFee;
            result.ExamDiscountFee = simulation.ExamDiscountFee;
            result.ExamImage = simulation.ExamImage;
            if (questions != null)
            {
                foreach(var question in questions)
                {
                    var exam = new ExamList()
                    {
                        QuestionId = question.QuestionId,
                        QuestionName = question.QuestionName,
                    };
                    

                    foreach(var answer in question.Answers)
                    {
                        var answerExam = new AnswerList()
                        {
                            AnswerId = answer.AnswerId,
                            AnswerText = answer.Text,
                        };
                        exam.Answers.Add(answerExam);

                    }
                    result.ListQuestions.Add(exam);
                }
            }
            return _mapper.Map<SimulationExamDto>(result);
        }

        public async Task<SimulationExamDto> UpdateSimulationExamAsync(int examId, UpdateSimulationExamRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateSimulationExamValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId
            , cancellationToken
            , include: x => x.Include(p => p.Vouchers));
            if (exam is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }

            exam.ExamName = request.ExamName;
            exam.ExamDescription = request.ExamDescription;
            exam.ExamCode = request.ExamCode;
            exam.ExamFee = request.ExamFee;
            exam.ExamImage = request.ExamImage;
            exam.CertId = request.CertId;

            float? totalDiscount = 1;

            exam.Vouchers.Clear();

            foreach (var voucherId in request.VoucherIds)
            {
                if (voucherId > 0)
                {
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher == null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException("Voucher not found or is expired.");
                    }

                    var existingVoucher = await _uow.VoucherRepository.FirstOrDefaultAsync(v => v.VoucherId == voucherId, cancellationToken);
                    if (existingVoucher != null)
                    {
                        exam.Vouchers.Add(existingVoucher);
                    }
                    else
                    {
                        exam.Vouchers.Add(_mapper.Map<Voucher>(voucher));
                    }

                    totalDiscount *= (1 - voucher.Percentage / 100f);
                }
            }

            if (exam.ExamFee.HasValue)
            {
                exam.ExamDiscountFee = (int?)(exam.ExamFee.Value * totalDiscount);
            }

            _uow.SimulationExamRepository.Update(exam);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<SimulationExamDto>(exam);
        }


        public async Task<SimulationExamDto> DeleteSimulationExamAsync(int examId, CancellationToken cancellationToken)
        {
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId,
                cancellationToken, include: q => q.Include(c => c.Vouchers)
                .Include(c => c.Carts)
                .Include(c => c.StudentOfExams)
                .Include(c => c.Questions)
                .Include(c => c.Feedbacks)
                .Include(c => c.Vouchers)
                .Include(c => c.Scores)
                );
            if (exam is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }
            exam.Carts?.Clear();
            exam.Vouchers?.Clear();
            exam.StudentOfExams?.Clear();
            exam.Questions?.Clear();
            exam.Feedbacks?.Clear();
            exam.Scores?.Clear();

            _uow.SimulationExamRepository.Delete(exam);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<SimulationExamDto>(exam);
        }

        public async Task<List<SimulationExamDto>> GetSimulationExamByNameAsync(string? examName, CancellationToken cancellationToken)
        {
            IEnumerable<SimulationExam> result;
            if(string.IsNullOrEmpty(examName))
            {
                result = await _uow.SimulationExamRepository.GetAll();
            }
            else
            {
                result = await _uow.SimulationExamRepository.WhereAsync(x => x.ExamName.Contains(examName), cancellationToken);
                if (!result.Any())
                {
                    throw new KeyNotFoundException("Simulation Exam not found.");
                }
            }
            return _mapper.Map<List<SimulationExamDto>>(result);
        }

        public async Task<List<SimulationExamDto>> GetSimulationExamByCertIdAsync(int certId, CancellationToken cancellationToken)
        {
            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == certId, cancellationToken);

            if (certification == null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            var simulationExam = await _uow.SimulationExamRepository.WhereAsync(x => x.CertId == certId);

            if (simulationExam == null)
            {
                throw new KeyNotFoundException("Simulation not found.");
            }

            return _mapper.Map<List<SimulationExamDto>>(simulationExam);
        }
    }
}
