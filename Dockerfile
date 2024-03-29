﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ItPlanetAPI/ItPlanetAPI.csproj", "ItPlanetAPI/"]
RUN dotnet restore "ItPlanetAPI/ItPlanetAPI.csproj"
COPY . .
WORKDIR "/src/ItPlanetAPI"
RUN dotnet build "ItPlanetAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ItPlanetAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ItPlanetAPI.dll"]
