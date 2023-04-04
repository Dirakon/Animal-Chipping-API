using AutoMapper;
using ItPlanetAPI.Controllers;
using ItPlanetAPI.Dtos;
using ItPlanetAPI.Models;
using ItPlanetAPI.Relationships;
using ItPlanetAPI.Requests;

namespace ItPlanetAPI.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Account registration -> account creation -> account -> account DTO
        CreateMap<AccountRegistrationRequest, AccountCreationRequest>()
            .ForMember(
                request => request.Role,
                // After registration, default role is USER
                options => options.MapFrom(_ => AccountRole.User)
            );
        CreateMap<AccountCreationRequest, Account>();
        CreateMap<Account, AccountDto>();

        // Animal type request -> animal type -> animal type DTO
        CreateMap<AnimalTypeRequest, AnimalType>();
        CreateMap<AnimalType, AnimalTypeDto>();

        // Location request -> location -> location DTO
        CreateMap<LocationRequest, Location>();
        CreateMap<Location, LocationDto>();

        // Animal-location relationship -> animal-location relationship DTO
        CreateMap<Location, LocationDto>();

        // Animal request (update/creation) -> animal -> animal DTO
        CreateMap<AnimalCreationRequest, Animal>()
            .ForSourceMember(request => request.AnimalTypes, options => options.DoNotValidate())
            .ForMember(animal => animal.AnimalTypes, options => options.Ignore());
        CreateMap<AnimalUpdateRequest, Animal>();
        CreateMap<Animal, AnimalDto>()
            .ForMember(animalDto => animalDto.AnimalTypes,
                options => options.MapFrom(animal => animal.AnimalTypes.Select(type => type.TypeId).ToList()))
            .ForMember(animalDto => animalDto.VisitedLocations,
                options => options.MapFrom(animal => animal.VisitedLocations.Select(location => location.Id).ToList()));

        // Animal-location relationship -> animal-location relationship DTO
        CreateMap<AnimalAndLocationRelationship, AnimalLocationDto>();

        // Area request -> area -> area Dto
        CreateMap<AreaRequest, Area>()
            .ForSourceMember(request => request.AreaPoints, opt => opt.DoNotValidate())
            .ForMember(animal => animal.AreaPoints, opt => opt.Ignore());
        CreateMap<Area, AreaDto>();

        // Area point creation request -> area point -> area point DTO
        CreateMap<AreaPointCreationRequest, AreaPoint>();
        CreateMap<AreaPoint, AreaPointDto>();

        // Area analytics -> area analytics DTO
        CreateMap<AreaAnalytics, AreaAnalyticsDto>()
            .ForSourceMember(analytics => analytics.TypeIdToTypeAnalytics, options => options.DoNotValidate())
            .ForMember(analyticsDto => analyticsDto.AnimalsAnalytics, options =>
                options.MapFrom(analytics =>
                    analytics.TypeIdToTypeAnalytics
                        .Values
                        .Where(typeAnalytics =>
                            typeAnalytics.AnimalsArrived + typeAnalytics.AnimalsGone + typeAnalytics.QuantityAnimals >
                            0)
                        .ToList()
                )
            );
    }
}