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
        CreateMap<AnimalCreationRequest, Animal>()
            .ForSourceMember(request => request.AnimalTypes, opt => opt.DoNotValidate())
            .ForMember(animal=>animal.AnimalTypes,opt=>opt.Ignore());
        CreateMap<AnimalUpdateRequest, Animal>();
        CreateMap<Animal, AnimalDto>()
            .ForMember(animalDto=>animalDto.AnimalTypes,opt=>opt.MapFrom(animal=>animal.AnimalTypes.Select(type=>type.TypeId)));
    }
}