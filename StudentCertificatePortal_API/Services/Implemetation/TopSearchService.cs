using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class TopSearchService : ITopSearchService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TopSearchService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<CertificationDto>> GetCertificationByTopSearchAsync(int topN)
        {
            var certifications = await _uow.CertificationRepository.GetAllAsync(query => query.Include(a => a.Organize));

            var topCertifications = certifications
        .Select(certification => new
        {
            Certification = certification,
            TopExamStudentCount = certification.SimulationExams
                .Select(exam => exam.StudentOfExams
                    .GroupBy(se => se.ExamId) 
                    .Max(group => group.Count())) 
                .DefaultIfEmpty(0)
                .Max()
        })
        .OrderByDescending(cert => cert.TopExamStudentCount)
        .Take(topN)
        .Select(cert => cert.Certification)
        .ToList();
            var certificationDtos = _mapper.Map<List<CertificationDto>>(topCertifications);
            foreach (var dto in certificationDtos)
            {
                var certification = topCertifications.FirstOrDefault(c => c.CertId == dto.CertId);
                if (certification != null)
                {
                    dto.OrganizeName = certification.Organize?.OrganizeName;
                }
            }
            return certificationDtos;
        }
        
    }
}
