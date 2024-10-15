using AutoMapper;
using Core.Entities.Models;
using Core.Responses;

namespace API.Helpers
{
	public class MappingProfiles : Profile
	{
        public MappingProfiles()
        {
            CreateMap<Account, AccountResponse>();
            CreateMap<Account, AuthResponse>();
        }
    }
}
