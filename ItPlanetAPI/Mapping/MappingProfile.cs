using AutoMapper;
using ItPlanetAPI.Models;

namespace ItPlanetAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Account request -> account -> account DTO
        CreateMap<AccountCreationRequest, Account>();
        CreateMap<Account, AccountDto>();

        // Animal type request -> animal type -> animal type DTO
        CreateMap<AnimalTypeRequest, AnimalType>();
        CreateMap<AnimalType, AnimalTypeDto>();

        // Animal location request -> animal location -> animal location DTO
        CreateMap<LocationRequest, Location>();
        CreateMap<Location, LocationDto>();

        // Animal-location relationship -> animal-location relationship DTO
        CreateMap<Location, LocationDto>();

        // Animal request (update/creation) -> animal -> animal DTO
        CreateMap<AnimalCreationRequest, Animal>()
            .ForSourceMember(request => request.AnimalTypes, opt => opt.DoNotValidate())
            .ForMember(animal => animal.AnimalTypes, opt => opt.Ignore());
        CreateMap<AnimalUpdateRequest, Animal>();
        CreateMap<Animal, AnimalDto>()
            .ForMember(animalDto => animalDto.AnimalTypes,
                opt => opt.MapFrom(animal => animal.AnimalTypes.Select(type => type.TypeId)));
    }
}