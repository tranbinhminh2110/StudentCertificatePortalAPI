using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ProfileService: IProfileService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateProfileRequest> _updateUserValidator;


        public ProfileService(IUnitOfWork uow, IMapper mapper, IValidator<UpdateProfileRequest> updateUserValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _updateUserValidator = updateUserValidator;
        }

        public async Task<UserDto> FindByEmailAsync(string email)
        {
            if (email == null) throw new ArgumentNullException("email");
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.Email == email);
            return _mapper.Map<UserDto>(user);
        }
        public async Task<UserDto> GetProfileByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var result = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (result is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return _mapper.Map<UserDto>(result);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.Email == email);
            if (user is null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Người dùng không tồn tại." });
            }

            user.Password = newPassword;

            _uow.UserRepository.Update(user);
            await _uow.Commit(new CancellationToken());

            return IdentityResult.Success;
        }
        public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateUserValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User is not found.");
            }
            user.Username = request.Username?.Trim();
            user.Email = request.Email?.Trim();
            user.Address = request.Address?.Trim();
            user.PhoneNumber = request.PhoneNumber?.Trim();
            user.Fullname = request.Fullname?.Trim();
            user.Dob = request.Dob;
            user.UserImage = request.UserImage?.Trim();

            _uow.UserRepository.Update(user);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }
    }
}
