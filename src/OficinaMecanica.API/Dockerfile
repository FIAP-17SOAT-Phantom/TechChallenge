FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OficinaMecanica.API/OficinaMecanica.API.csproj", "OficinaMecanica.API/"]
COPY ["OficinaMecanica.Application/OficinaMecanica.Application.csproj", "OficinaMecanica.Application/"]
COPY ["OficinaMecanica.Domain/OficinaMecanica.Domain.csproj", "OficinaMecanica.Domain/"]
COPY ["OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj", "OficinaMecanica.Infrastructure/"]
RUN dotnet restore "OficinaMecanica.API/OficinaMecanica.API.csproj"
COPY . .
WORKDIR "/src/OficinaMecanica.API"
RUN dotnet build "OficinaMecanica.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OficinaMecanica.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "OficinaMecanica.API.dll"]
