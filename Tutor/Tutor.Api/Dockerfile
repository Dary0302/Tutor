FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR ..
COPY ["Tutor.Api/Tutor.Api.csproj", "Tutor.Api/"]
COPY ["Tutor.Application/Tutor.Application.csproj", "Tutor.Application/"]
COPY ["Tutor.Domain/Tutor.Domain.csproj", "Tutor.Domain/"]
COPY ["Tutor.Infrastructure/Tutor.Infrastructure.csproj", "Tutor.Infrastructure/"]

COPY ["Core/Core.Common/Core.Common.csproj", "Core/Core.Common/"]

RUN dotnet restore "Tutor.Api/Tutor.Api.csproj"

COPY . .
COPY ../Core ../Core

WORKDIR "/Tutor.Api"
RUN dotnet build "./Tutor.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tutor.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tutor.Api.dll"]
