namespace Pollux.Application.Mappers
{
    using AutoMapper;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Application.Models.Response;
    using Domain.Entities;

    public class UserMappers : Profile
    {
        public UserMappers()
        {
            this.CreateMap<LogInModel, User>();
            this.CreateMap<SignUpModel, User>();
            this.CreateMap<UserPreferences, UserPreferenceModel>().ReverseMap();
        }
    }
}
