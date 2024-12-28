using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICartService _cartservice;

        private readonly IValidator<CreateUserRequest> _addUserValidator;
        private readonly IValidator<UpdateUserRequest> _updateUserValidator;
        private readonly IValidator<CreateRegisterUserRequest> _addregisterUserValidator;

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;


        public UserService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateUserRequest> addUserValidator,
            IValidator<UpdateUserRequest> updateUserValidator,
            IValidator<CreateRegisterUserRequest> addregisterUserValidator, ICartService cartservice,
            IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addUserValidator = addUserValidator;
            _updateUserValidator = updateUserValidator;
            _addregisterUserValidator = addregisterUserValidator;
            _cartservice = cartservice;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task<UserDto> ChangeStatusAccountAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.Status = !user.Status;
            if (user.Status == true)
            {
                user.UserOffenseCount = 0;
                var notification = new Notification()
                {
                    NotificationName = "Account Status Activated",
                    NotificationDescription = $"The account of user {user.Username} has been activated and reset to 0 violations.",
                    NotificationImage = user.UserImage,
                    CreationDate = DateTime.UtcNow,
                    Role = "Admin",
                    IsRead = false,
                };
                await _uow.NotificationRepository.AddAsync(notification);
            }

            _uow.UserRepository.Update(user);

            await _uow.Commit(cancellationToken);
            var userDto = _mapper.Map<UserDto>(user);
            var notifications = await _notificationService.GetNotificationByRoleAsync("Admin", cancellationToken);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            return userDto;

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
                UserImage = request.UserImage,
                UserLevel = EnumLevel.Bronze.ToString(),
            };

            var addedUser = await _uow.UserRepository.AddAsync(userEntity);
            await _uow.Commit(cancellationToken);
            var cartRequest = new CreateCartRequest { UserId = addedUser.UserId };
            var cartDto = await _cartservice.CreateCartAsync(cartRequest, cancellationToken);
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
                UserImage = request.UserImage,
                UserLevel = EnumLevel.Bronze.ToString(),
            };

            var addedUser = await _uow.UserRepository.AddAsync(userEntity);
            await _uow.Commit(cancellationToken);
            var cartRequest = new CreateCartRequest { UserId = addedUser.UserId };
            var cartDto = await _cartservice.CreateCartAsync(cartRequest, cancellationToken);
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
                        .Include(c => c.Cart).Include(c => c.Notifications).Include(c => c.Scores));



            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.Feedbacks?.Clear();
            user.CoursesEnrollments?.Clear();
            user.ExamsEnrollments?.Clear();

            user.Notifications?.Clear();
            user.Scores?.Clear();
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == user.UserId);
            if (wallet != null)
            {
                _uow.WalletRepository.Delete(wallet);
                await _uow.Commit(cancellationToken);
            }

            var cart = await _uow.CartRepository.FirstOrDefaultAsync(c => c.UserId == user.UserId);
            if (cart != null)
            {
                _uow.CartRepository.Delete(cart);
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

        public async Task<List<UserDto>> GetAll(CancellationToken cancellationToken)
        {
            var users = await _uow.UserRepository.GetAll();

            foreach (var user in users)
            {
                if (user.UserOffenseCount >= 3 && user.Status == true)
                {
                    user.Status = false;
                    _uow.UserRepository.Update(user);
                    var notification = new Notification()
                    {
                        NotificationName = "Account Status Deactivated",
                        NotificationDescription = $"The account of user {user.Username} has been recorded by the system for 3 or more violations and has been deactivated.",
                        NotificationImage = user.UserImage,
                        CreationDate = DateTime.UtcNow,
                        Role = "Admin",
                        IsRead = false,
                    };
                    await _uow.NotificationRepository.AddAsync(notification);
                }
            }

            await _uow.Commit(cancellationToken);

            var usersDto = _mapper.Map<List<UserDto>>(users);

            var notifications = await _notificationService.GetNotificationByRoleAsync("Admin", cancellationToken);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            return usersDto;
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
