using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
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

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;



        public SimulationExamService(IUnitOfWork uow, IMapper mapper, 
            IValidator<CreateSimulationExamRequest> addSimulationExamValidator,
            IValidator<UpdateSimulationExamRequest> updateSimulationExamValidator,
            IVoucherService voucherService,
            IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addSimulationExamValidator = addSimulationExamValidator;
            _updateSimulationExamValidator = updateSimulationExamValidator;
            _voucherService = voucherService;
            _hubContext = hubContext;
            _notificationService = notificationService;
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
                Duration = request.Duration,
                QuestionCount = request.QuestionCount,
                ExamPermission = Enums.EnumPermission.Pending.ToString()
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
            var notification = new Notification()
            {
                NotificationName = "New Simulation Exam Created",
                NotificationDescription = $"A new simulation exam '{request.ExamName}' has been created and is pending approval.",
                NotificationImage = request.ExamImage,
                CreationDate = DateTime.UtcNow,
                Role = "Manager",
                IsRead = false,

            };
            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            return _mapper.Map<SimulationExamDto>(result);
        }


        public async Task<List<SimulationExamDto>> GetAll()
        {
            var result = await _uow.SimulationExamRepository.GetAllAsync(query =>
                query.Include(c => c.Vouchers)
                     .Include(c => c.Feedbacks)
                     .Include(c => c.Cert)
                     .ThenInclude(cert => cert.Type));

            var sExamDto = result.Select(x =>
            {
                var examDto = _mapper.Map<SimulationExamDto>(x);
                examDto.FeedbackCount = x.Feedbacks.GroupBy(feedback => feedback.ExamId)
                                           .Select(group => group.Count())
                                           .Sum();
                examDto.CertificationDetails = x.Cert != null ? new List<CertificationDetailsDto>
        {
            new CertificationDetailsDto
            {
                CertId = x.Cert.CertId,
                CertName = x.Cert.CertName,
                CertCode = x.Cert.CertCode,
                CertDescription = x.Cert.CertDescription,
                CertImage = x.Cert.CertImage,
                TypeName = x.Cert.Type?.TypeName,
                CertValidity = x.Cert.CertValidity,
                Permission = x.Cert.Permission,
            }
        } : new List<CertificationDetailsDto>();

                examDto.VoucherDetails = x.Vouchers != null ? x.Vouchers.Select(voucher => new VoucherDetailsDto
                {
                    VoucherId = voucher.VoucherId,
                    VoucherName = voucher.VoucherName,
                    VoucherDescription = voucher.VoucherDescription,
                    Percentage = voucher.Percentage,
                    CreationDate = voucher.CreationDate,
                    ExpiryDate = voucher.ExpiryDate,
                    VoucherStatus = voucher.VoucherStatus
                }).ToList() : new List<VoucherDetailsDto>();

                return examDto;
            }).ToList();

            
            return sExamDto;
        }


        public async Task<SimulationExamDto> GetSimulationExamByIdAsync(int examId, CancellationToken cancellationToken)
        {
            var simulation = await _uow.SimulationExamRepository.FirstOrDefaultAsync(
                x => x.ExamId == examId,
                cancellationToken,
                query => query.Include(a => a.Vouchers)
                               .Include(a => a.Feedbacks)
                              .Include(a => a.Cert)
                              .ThenInclude(cert => cert.Type)
            );

            if (simulation is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }

            var questions = await _uow.QuestionRepository.WhereAsync(
                x => x.ExamId == simulation.ExamId,
                cancellationToken,
                include: i => i.Include(ans => ans.Answers)
            );

            var result = _mapper.Map<SimulationExamDto>(simulation);

            result.CertificationDetails = simulation.Cert != null ? new List<CertificationDetailsDto>
    {
        new CertificationDetailsDto
        {
            CertId = simulation.Cert.CertId,
            CertName = simulation.Cert.CertName,
            CertCode = simulation.Cert.CertCode,
            CertDescription = simulation.Cert.CertDescription,
            CertImage = simulation.Cert.CertImage,
            TypeName = simulation.Cert.Type?.TypeName,
            CertValidity = simulation.Cert.CertValidity,
                Permission = simulation.Cert.Permission,
        }
    } : new List<CertificationDetailsDto>();

            result.VoucherDetails = simulation.Vouchers != null ? simulation.Vouchers.Select(voucher => new VoucherDetailsDto
            {
                VoucherId = voucher.VoucherId,
                VoucherName = voucher.VoucherName,
                VoucherDescription = voucher.VoucherDescription,
                Percentage = voucher.Percentage,
                CreationDate = voucher.CreationDate,
                ExpiryDate = voucher.ExpiryDate,
                VoucherStatus = voucher.VoucherStatus
            }).ToList() : new List<VoucherDetailsDto>();


            if (questions != null)
            {
                result.ListQuestions = questions.Select(question => new ExamList
                {
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionText,
                    Answers = question.Answers.Select(answer => new AnswerList
                    {
                        AnswerId = answer.AnswerId,
                        AnswerText = answer.Text,
                        IsCorrect = answer.IsCorrect,
                    }).ToList()
                }).ToList();
            }
            result.FeedbackCount = simulation.Feedbacks?.Count() ?? 0;
            return result;
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
            exam.Duration = request.Duration;
            exam.QuestionCount = request.QuestionCount;
            exam.ExamPermission = Enums.EnumPermission.Pending.ToString();

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
            var notification = new Notification
            {
                NotificationName = "Simulation Exam Updated",
                NotificationDescription = $"The simulation exam '{exam.ExamName}' has been updated and is pending approval.",
                NotificationImage = request.ExamImage,  
                CreationDate = DateTime.UtcNow,
                Role = "Manager",
                IsRead = false,

            };

            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

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

            if (string.IsNullOrEmpty(examName))
            {
                result = await _uow.SimulationExamRepository.GetAllAsync(query =>
                    query.Include(a => a.Vouchers)
                        .Include(a => a.Feedbacks)
                         .Include(a => a.Cert));
            }
            else
            {
                result = await _uow.SimulationExamRepository.WhereAsync(
                    x => x.ExamName.Contains(examName), cancellationToken,
                    query => query.Include(a => a.Vouchers).Include(a => a.Feedbacks)
                                  .Include(a => a.Cert));

                if (!result.Any())
                {
                    throw new KeyNotFoundException("Simulation Exam not found.");
                }
            }

            var sExamDto = result.Select(x =>
            {
                var examDto = _mapper.Map<SimulationExamDto>(x);
                examDto.FeedbackCount = x.Feedbacks.GroupBy(feedback => feedback.ExamId)
                                           .Select(group => group.Count())
                                           .Sum();

                examDto.CertificationDetails = x.Cert != null ? new List<CertificationDetailsDto>
        {
            new CertificationDetailsDto
            {
                CertId = x.Cert.CertId,
                CertName = x.Cert.CertName,
                CertCode = x.Cert.CertCode,
                CertDescription = x.Cert.CertDescription,
                CertImage = x.Cert.CertImage,
                TypeName = x.Cert.Type?.TypeName,
                CertValidity = x.Cert.CertValidity,
                Permission = x.Cert.Permission,
            }
        } : new List<CertificationDetailsDto>();

                examDto.VoucherDetails = x.Vouchers != null ? x.Vouchers.Select(voucher => new VoucherDetailsDto
                {
                    VoucherId = voucher.VoucherId,
                    VoucherName = voucher.VoucherName,
                    VoucherDescription = voucher.VoucherDescription,
                    Percentage = voucher.Percentage,
                    CreationDate = voucher.CreationDate,
                    ExpiryDate = voucher.ExpiryDate,
                    VoucherStatus = voucher.VoucherStatus
                }).ToList() : new List<VoucherDetailsDto>();

                return examDto;
            }).ToList();

            return sExamDto;
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
        public async Task<SimulationExamDto> UpdateExamVouchersAsync(int examId, List<int> voucherIds, CancellationToken cancellationToken)
        {
            // Lấy exam từ cơ sở dữ liệu
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(
                x => x.ExamId == examId,
                cancellationToken,
                include: x => x.Include(e => e.Vouchers));

            if (exam is null)
            {
                throw new KeyNotFoundException("Simulation Exam not found.");
            }

            // Xóa tất cả voucher hiện tại liên kết với exam
            exam.Vouchers.Clear();

            // Biến để tính tổng giảm giá
            float? totalDiscount = 1;

            // Thêm các voucher mới
            foreach (var voucherId in voucherIds)
            {
                if (voucherId > 0)
                {
                    // Lấy thông tin voucher từ service
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher == null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException($"Voucher with ID {voucherId} not found or is expired.");
                    }

                    // Kiểm tra xem voucher có tồn tại trong database hay không
                    var existingVoucher = await _uow.VoucherRepository.FirstOrDefaultAsync(v => v.VoucherId == voucherId, cancellationToken);
                    if (existingVoucher != null)
                    {
                        exam.Vouchers.Add(existingVoucher);
                    }
                    else
                    {
                        exam.Vouchers.Add(_mapper.Map<Voucher>(voucher));
                    }

                    // Tính tổng giảm giá
                    totalDiscount *= (1 - voucher.Percentage / 100f);
                }
            }

            // Cập nhật giá sau giảm
            if (exam.ExamFee.HasValue)
            {
                exam.ExamDiscountFee = (int?)(exam.ExamFee.Value * totalDiscount);
            }

            // Cập nhật dữ liệu
            _uow.SimulationExamRepository.Update(exam);
            await _uow.Commit(cancellationToken);

            // Trả về kết quả
            var examDto = _mapper.Map<SimulationExamDto>(exam);
            examDto.VoucherDetails = exam.Vouchers.Select(v => new VoucherDetailsDto
            {
                VoucherId = v.VoucherId,
                VoucherName = v.VoucherName,
                VoucherDescription = v.VoucherDescription,
                Percentage = v.Percentage,
                CreationDate = v.CreationDate,
                ExpiryDate = v.ExpiryDate,
                VoucherStatus = v.VoucherStatus
            }).ToList();

            return examDto;
        }
        
    }
}
