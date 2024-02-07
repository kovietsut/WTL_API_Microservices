#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/Manga/Manga.API/Manga.API.csproj", "Services/Manga/Manga.API/"]
COPY ["BuildingBlocks/Common.Logging/Common.Logging.csproj", "BuildingBlocks/Common.Logging/"]
COPY ["BuildingBlocks/EventBus/EventBus.Messages/EventBus.Messages.csproj", "BuildingBlocks/EventBus/EventBus.Messages/"]
COPY ["Services/Manga/Manga.Application/Manga.Application.csproj", "Services/Manga/Manga.Application/"]
COPY ["BuildingBlocks/Infrastructure/Infrastructure.csproj", "BuildingBlocks/Infrastructure/"]
COPY ["BuildingBlocks/Contracts/Contracts.csproj", "BuildingBlocks/Contracts/"]
COPY ["BuildingBlocks/Shared/Shared.csproj", "BuildingBlocks/Shared/"]
COPY ["Services/Manga/Manga.Infrastructure/Manga.Infrastructure.csproj", "Services/Manga/Manga.Infrastructure/"]
RUN dotnet restore "Services/Manga/Manga.API/./Manga.API.csproj"
COPY . .
WORKDIR "/src/Services/Manga/Manga.API"
RUN dotnet build "Manga.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Manga.API.csproj" -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Manga.API.dll"]