﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
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
            var certifications = await _uow.CertificationRepository.GetAllAsync(query => query.Where(cert => cert.Permission == Enums.EnumPermission.Approve.ToString())
            .Include(a => a.Organize));

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

        public async Task<List<SimulationExamDto>> GetSimulationExamByTopSearchAsync(int topN, EnumPermission permission)
        {
            var exams = await _uow.SimulationExamRepository.GetAllAsync(query => query
                .Where(exam => exam.ExamPermission == permission.ToString())
                .Include(exam => exam.StudentOfExams));

            var topExams = exams
                .Select(exam => new
                {
                    Exam = exam,
                    TotalStudents = exam.StudentOfExams.Count
                })
                .OrderByDescending(exam => exam.TotalStudents)
                .Take(topN)
                .Select(exam => exam.Exam)
                .ToList();

            var examDtos = _mapper.Map<List<SimulationExamDto>>(topExams);
            return examDtos;
        }

    }
}
