using AutoMapper;
using LandonApi.Models;

namespace LandonApi.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RoomEntity, Room>()
                .ForMember(
                    dest => dest.Rate,
                    option => option.MapFrom(src => src.Rate / 100.0m));
        }
    }
}
