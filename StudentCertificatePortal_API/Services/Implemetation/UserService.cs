﻿using AutoMapper;
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
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateUserRequest> _addUserValidator;
        private readonly IValidator<UpdateUserRequest> _updateUserValidator;
        private readonly IValidator<CreateRegisterUserRequest> _addregisterUserValidator;


        public UserService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateUserRequest> addUserValidator,
            IValidator<UpdateUserRequest> updateUserValidator,
            IValidator<CreateRegisterUserRequest> addregisterUserValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addUserValidator = addUserValidator;
            _updateUserValidator = updateUserValidator;
            _addregisterUserValidator = addregisterUserValidator;
        }

        public async Task<UserDto> ChangeStatusAccountAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if(user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.Status = !user.Status;

            _uow.UserRepository.Update(user);

            await _uow.Commit(cancellationToken);
            return _mapper.Map<UserDto>(user);

        }

        public async Task<UserDto> CreateRegisterUserAsync(CreateRegisterUserRequest request, CancellationToken cancellationToken)
        {
            var existingUser = await _uow.UserRepository.FirstOrDefaultAsync(x =>
               x.Username.Trim().ToLower() == request.Username.Trim().ToLower()
               || x.Email.Trim() == request.Email.Trim(),
               cancellationToken);

            if (existingUser != null)
            {
                throw new Exception("This username/email has already been taken.");
            }

            var userEntity = new User()
            {
                Username = request.Username?.Trim(),
                Password = request.Password,
                Email = request.Email?.Trim(),
                Fullname = request.Fullname?.Trim(),
                Dob = request.Dob,
                Address = request.Address?.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                Role = "Student",
                Status = true,
                UserCreatedAt = DateTime.UtcNow,
                UserImage = request.UserImage
            };

            var addedUser = await _uow.UserRepository.AddAsync(userEntity);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<UserDto>(addedUser);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var existingUser = await _uow.UserRepository.FirstOrDefaultAsync(x =>
                x.Username.Trim().ToLower() == request.Username.Trim().ToLower()
                || x.Email.Trim() == request.Email.Trim(),
                cancellationToken);

            if (existingUser != null)
            {
                throw new Exception("This username/email has already been taken.");
            }

            var userEntity = new User()
            {
                Username = request.Username?.Trim(),
                Password = request.Password,
                Email = request.Email?.Trim(),
                Fullname = request.Fullname?.Trim(),
                Dob = request.Dob,
                Address = request.Address?.Trim(), 
                PhoneNumber = request.PhoneNumber?.Trim(),
                Role = request.Role,
                Status = request.Status,
                UserCreatedAt = DateTime.UtcNow,
                UserImage = request.UserImage
            };

            var addedUser = await _uow.UserRepository.AddAsync(userEntity);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<UserDto>(addedUser);
        }

        public async Task<UserDto> DeleteUserByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId,
                cancellationToken,
                include: q => q.Include(c => c.Feedbacks)
                        .Include(c => c.Wallet)
                        .Include(c => c.CoursesEnrollments)
                        .Include(c => c.ExamsEnrollments)
                        .Include(c => c.Cart));

            

            if(user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.Feedbacks?.Clear();
            user.CoursesEnrollments?.Clear();
            user.ExamsEnrollments?.Clear();

            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == user.UserId);
            if(wallet != null)
            {
                _uow.WalletRepository.Delete(wallet);
                await _uow.Commit(cancellationToken);
            }
             

            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            _uow.UserRepository.Delete(user);

            await _uow.Commit(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetAll()
        {
            var result = await _uow.UserRepository.GetAll();
            return _mapper.Map<List<UserDto>>(result);
        }

        public async Task<UserDto> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var result = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (result is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return _mapper.Map<UserDto>(result);
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken)
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
            user.Password = request.Password;
            user.Email = request.Email?.Trim();
            user.Address = request.Address?.Trim();
            user.PhoneNumber = request.PhoneNumber?.Trim();
            user.Fullname = request.Fullname?.Trim();
            user.Dob = request.Dob;
            user.Status = request.Status;
            user.UserImage = request.UserImage?.Trim();
            user.Role = request.Role?.Trim();

            _uow.UserRepository.Update(user);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }
    }
}
