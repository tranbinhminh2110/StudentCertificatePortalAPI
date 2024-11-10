using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class LoginService: ILoginService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public LoginService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<UserDto> Authenticate(LoginUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.Email == request.Email
            && x.Password == request.Password, cancellationToken);

            if (user == null)
            {
                throw new ConflictException("Login Failed!");
            }else if(user.Status == false)
            {
                throw new UserAuthenticationException("Account is disabled. Please contact support.");
            }

            return _mapper.Map<UserDto>(user);
        }
    }
}
