namespace Pollux.Application.Mappers
{
    using AutoMapper;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Domain.Entities;

    public class UserMappers : Profile
    {
        public UserMappers()
        {
            this.CreateMap<LogInModel, User>();
            this.CreateMap<SignUpModel, User>();
        }
    }
}
