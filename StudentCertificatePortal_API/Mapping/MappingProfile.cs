using AutoMapper;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_Data.Models;
using System.Reflection;

namespace StudentCertificatePortal_API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
{
    
    ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    CreateMap<UserAnswer, UserAnswerForEssayDto>();
    CreateMap<PeerReview, PeerReviewDto>()
    .ForMember(dest => dest.ReviewedUserId, opt => opt.MapFrom(src => src.ReviewedUserId ?? 0)) // Default to 0 if null
    .ForMember(dest => dest.ScorePeerReviewer, opt => opt.MapFrom(src => src.ScorePeerReviewer ?? 0)) // Default to 0 if null
    .ForMember(dest => dest.FeedbackPeerReviewer, opt => opt.MapFrom(src => src.FeedbackPeerReviewer ?? string.Empty)); // Default to empty string if null
    CreateMap<Certification, CertificationDto>()
    .ForMember(dest => dest.CertPrerequisite, opt => opt.Ignore())
    .ForMember(dest => dest.CertCodePrerequisite, opt => opt.Ignore())
    .ForMember(dest => dest.CertDescriptionPrerequisite, opt => opt.Ignore());
}

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var mapFromType = typeof(IMapFrom<>);

            var mappingMethodName = nameof(IMapFrom<object>.Mapping);

            bool HasInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;

            var types = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(HasInterface)).ToList();

            var argumentTypes = new Type[] { typeof(Profile) };

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod(mappingMethodName);

                if (methodInfo != null)
                {
                    methodInfo.Invoke(instance, new object[] { this });
                }
                else
                {
                    var interfaces = type.GetInterfaces().Where(HasInterface).ToList();

                    if (interfaces.Count > 0)
                    {
                        foreach (var @interface in interfaces)
                        {
                            var interfaceMethodInfo = @interface.GetMethod(mappingMethodName, argumentTypes);

                            interfaceMethodInfo?.Invoke(instance, new object[] { this });
                        }
                    }
                }
            }
        }
    }
}
