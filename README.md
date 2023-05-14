# Animal-Chipping-API

A simple backend for animal chipping service.

## Description

The server implements authorization/registration (basic) with roles, and different user operations related to animals, animal types, areas, locations.

## Getting Started

### Prerequisites

* [dotnet6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

### Dependencies

* [ASP.net](https://ASP.net)
* [NUnit](https://nunit.org/)
* [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)
* [EntityFrameworkCore](https://learn.microsoft.com/en-us/ef/core/)
* [Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/efcore.pg)
* [AutoMapper](https://automapper.org/)
* [Macross.Json.Extensions](https://www.nuget.org/packages/Macross.Json.Extensions/)
* [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)


### Installing

* Run `dotnet restore` to install the dependencies
* Run `dotnet build --configuration Release --no-restore` to build

### Executing program

* Run `dotnet run --project ItPlanetAPI` to execute. Specify the port in `./ItPlanetAPI/Properties/launchSettings.json`.
* Alternatively, build a Docker container using `docker build -t <some-name> .`, run with `docker run <some-name>`.
* The server relies on a Postgres instance running, the connection string can be set up either in the environment variables or in `./ItPlanetAPI/Properties/launchSettings.json`.


## License

This project is licensed under the MIT License - see the `LICENSE.md` file for details.

