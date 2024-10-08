﻿using AutoMapper;
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
