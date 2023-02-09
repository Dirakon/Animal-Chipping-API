using AutoMapper;
using ItPlanetAPI.Models;

namespace ItPlanetAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account,AccountDto>();
        CreateMap<AccountRequest,Account>();
        CreateMap<AnimalTypeRequest,AnimalType>();
        CreateMap<AnimalRequest,Animal>();
        CreateMap<AnimalLocationRequest,AnimalLocation>();
    }
}