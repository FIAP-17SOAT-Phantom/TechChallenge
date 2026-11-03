FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src
COPY ["OficinaMecanica.Worker/OficinaMecanica.Worker.csproj", "OficinaMecanica.Worker/"]
COPY ["OficinaMecanica.Application/OficinaMecanica.Application.csproj", "OficinaMecanica.Application/"]
COPY ["OficinaMecanica.Domain/OficinaMecanica.Domain.csproj", "OficinaMecanica.Domain/"]
COPY ["OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj", "OficinaMecanica.Infrastructure/"]
RUN dotnet restore "OficinaMecanica.Worker/OficinaMecanica.Worker.csproj"
COPY . .
RUN dotnet publish "OficinaMecanica.Worker/OficinaMecanica.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "OficinaMecanica.Worker.dll"]
