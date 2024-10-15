using AutoMapper;
using Core.Entities.Models;
using Core.Requests;
using Core.Responses;

namespace API.Helpers
{
	public class MappingProfiles : Profile
	{
        public MappingProfiles()
        {
            CreateMap<Account, AccountResponse>();
            CreateMap<Account, AuthResponse>();
            CreateMap<RegisterRequest, Account>();
            CreateMap<CreateRequest, Account>();
        }
    }
}
