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
            CreateMap<UpdateRequest, Account>()
                .ForAllMembers(x => x.Condition(
                    (src, dest, prop) =>
                    {
                        if (prop == null)
                        {
                            return false;
                        }
                        if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop))
                        {
                            return false;
                        }

                        if (x.DestinationMember.Name == "Role" && src.Role == null)
                        {
                            return false;
                        }

                        return true;
                    }
                ));
		}
    }
}
