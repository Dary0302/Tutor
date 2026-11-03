FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tutor/Tutor.Api/Tutor.Api.csproj", "Tutor/Tutor.Api/"]
COPY ["Tutor/Tutor.Application/Tutor.Application.csproj", "Tutor/Tutor.Application/"]
COPY ["Tutor/Tutor.Domain/Tutor.Domain.csproj", "Tutor/Tutor.Domain/"]
COPY ["Tutor/Tutor.Infrastructure/Tutor.Infrastructure.csproj", "Tutor/Tutor.Infrastructure/"]
COPY ["Core/Core.Common/Core.Common.csproj", "Core/Core.Common/"]

RUN dotnet restore "Tutor/Tutor.Api/Tutor.Api.csproj"

COPY . .
WORKDIR /src/Tutor/Tutor.Api
RUN dotnet publish "Tutor.Api.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tutor.Api.dll"]
