using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentCertificatePortal_API.Mapping
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType()).ReverseMap();
    }
}
