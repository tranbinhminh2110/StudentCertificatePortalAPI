using AutoMapper;
using FluentValidation;
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

            var voucher = await _voucherService.GetVoucherByIdAsync(request.VoucherId , cancellationToken);
            if(voucher is null || voucher.VoucherStatus == false)
            {
                throw new KeyNotFoundException("Voucher not found or is expired.");
            }
            float? percentage = voucher.Percentage ?? 0;
            var exam = new SimulationExam()
            {
                ExamName = request.ExamName,
                ExamCode = request.ExamCode,
                ExamDescription = request.ExamDescription,
                ExamFee = request.ExamFee,
                ExamDiscountFee = (int?)((1 - (float)(voucher.Percentage.Value) / 100f) * request.ExamFee.Value),
                ExamImage = request.ExamImage,
                CertId = request.CertId,
            };
            var result = await _uow.SimulationExamRepository.AddAsync(exam);
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
            var result = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
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
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId, cancellationToken);
            if (exam is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }

            var voucher = await _uow.VoucherRepository.FirstOrDefaultAsync(x => x.VoucherId == request.VourcherId);
            if(voucher is null)
            {
                throw new KeyNotFoundException("Voucher not found.");
            }
            exam.ExamName = request.ExamName;
            exam.ExamDescription = request.ExamDescription;
            exam.ExamCode = request.ExamCode;
            exam.ExamFee = request.ExamFee;
            exam.ExamDiscountFee = request.ExamFee*(1-voucher.Percentage);
            exam.ExamImage = request.ExamImage;
            exam.CertId = request.CertId;
            _uow.SimulationExamRepository.Update(exam);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<SimulationExamDto>(exam);
        }

        public async Task<SimulationExamDto> DeleteSimulationExamAsync(int examId, CancellationToken cancellationToken)
        {
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId, cancellationToken);
            if (exam is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }
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
    }
}
