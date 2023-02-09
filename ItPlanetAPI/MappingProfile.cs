using AutoMapper;
using ItPlanetAPI.Models;

namespace ItPlanetAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Account request -> account -> account DTO
        CreateMap<AccountRequest, Account>();
        CreateMap<Account, AccountDto>();

        // Animal type request -> animal type -> animal type DTO
        CreateMap<AnimalTypeRequest, AnimalType>();
        CreateMap<AnimalType, AnimalTypeDto>();

        // Animal location request -> animal location -> animal location DTO
        CreateMap<AnimalLocationRequest, AnimalLocation>();
        CreateMap<AnimalLocation, AnimalLocationDto>();

        // Animal request -> animal -> animal DTO
        CreateMap<AnimalRequest, Animal>()
            .ForSourceMember(request => request.AnimalTypes, opt => opt.DoNotValidate());
        CreateMap<Animal, AnimalDto>();
    }
}