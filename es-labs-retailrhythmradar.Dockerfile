#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 8081
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ES.Labs.RetailRhythmRadar/ES.Labs.RetailRhythmRadar.csproj", "ES.Labs.RetailRhythmRadar/"]
COPY ["ES.Labs.RetailRhythmRadar/git_ver_info.bat", "ES.Labs.RetailRhythmRadar/"]

RUN dotnet restore "./ES.Labs.RetailRhythmRadar/./ES.Labs.RetailRhythmRadar.csproj"
COPY . .
WORKDIR "/src/ES.Labs.RetailRhythmRadar"

# RUN dotnet build "./ES.Labs.RetailRhythmRadar.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ES.Labs.RetailRhythmRadar.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ES.Labs.RetailRhythmRadar.dll"]