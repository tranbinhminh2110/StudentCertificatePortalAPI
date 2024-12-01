using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.ComponentModel.Design;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class SelectedCertService : ISelectedCertService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SelectedCertService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<bool> DeleteCertForUser(int userId, int certId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.UserRepository.FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken,
                    include: u => u.Include(c => c.Certs)
                );

                if (user == null || user.Status == false)
                {
                    throw new KeyNotFoundException("User not found or deactivated.");
                }

                var certToDelete = user.Certs.FirstOrDefault(c => c.CertId == certId);

                if (certToDelete == null)
                {
                    throw new KeyNotFoundException($"Certification with ID {certId} not found.");
                }

                user.Certs.Remove(certToDelete);

                await _uow.Commit(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CertificationDto>> GetCertsByUserId(int userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.UserRepository.FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken,
                    include: u => u.Include(c => c.Certs)
                );

                if (user == null || user.Status == false)
                {
                    throw new KeyNotFoundException("User not found or deactivated.");
                }

                return _mapper.Map<List<CertificationDto>>(user.Certs.ToList());
            }
            catch (Exception ex)
            {
                 throw new Exception("An error occurred while fetching certifications.", ex);
            }
        }

        public async Task<bool> SelectedCertForUser(CreateCertForUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken, include: u => u.Include(c => c.Certs));

                if (user == null || user.Status == false)
                {
                    throw new KeyNotFoundException("User not found or deactivated.");
                }

                foreach (var certId in request.CertificationId)
                {
                    var certExisting = await _uow.CertificationRepository.FirstOrDefaultAsync(
                        x => x.CertId == certId,
                        cancellationToken
                    );

                    if (certExisting == null)
                    {
                        throw new KeyNotFoundException($"Certification with ID {certId} not found.");
                    }

                    if (user.Certs.Any(c => c.CertId == certId))
                    {
                        continue;
                    }

                    user.Certs.Add(certExisting);
                    await _uow.Commit(cancellationToken);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCertForUser(UpdateCertForUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.UserRepository.FirstOrDefaultAsync(
                    x => x.UserId == request.UserId,
                    cancellationToken,
                    include: u => u.Include(c => c.Certs)
                );

                if (user == null || user.Status == false)
                {
                    throw new KeyNotFoundException("User not found or deactivated.");
                }

                user.Certs.Clear();

                foreach (var certId in request.CertificationId)
                {
                    var certExisting = await _uow.CertificationRepository.FirstOrDefaultAsync(
                        x => x.CertId == certId,
                        cancellationToken
                    );

                    if (certExisting == null)
                    {
                        throw new KeyNotFoundException($"Certification with ID {certId} not found.");
                    }

                    user.Certs.Add(certExisting);
                }

                await _uow.Commit(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
