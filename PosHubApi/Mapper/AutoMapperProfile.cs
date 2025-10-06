using AutoMapper;
using PosHubApi.Dtos;

namespace PosHubApi.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<OrderEventDto, UpdateOrderEventDto>();
        }
    }
}