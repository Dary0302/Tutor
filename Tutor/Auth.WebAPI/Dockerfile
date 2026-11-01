# 1. Базовый слой для запуска (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 2. Слой для сборки (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы проектов (обратите внимание: названия папок и файлов заменены на Auth.*)
COPY ["Auth.WebApi/Auth.WebApi.csproj", "Auth.WebApi/"]
COPY ["Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["Auth.Application/Auth.Application.csproj", "Auth.Application/"]
COPY ["Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]

# Восстанавливаем зависимости для главного проекта
RUN dotnet restore "./Auth.WebApi/Auth.WebApi.csproj"

# Копируем всё остальное и собираем
COPY . .
WORKDIR "/src/Auth.WebApi"
RUN dotnet build "./Auth.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. Слой для миграций базы данных
# ИСПРАВЛЕНО: В оригинале было "FROM buildbase", что вызвало бы ошибку. Нужно "FROM build"
FROM build AS migrations
RUN dotnet tool install --version 8.0.2 --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
# ИСПРАВЛЕНО: Убрал лишний "src/" в путях, так как WORKDIR уже равен /src
ENTRYPOINT dotnet-ef database update --project Auth.Infrastructure/ --startup-project Auth.WebApi

# 4. Публикация (Release сборка)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 5. Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# ИСПРАВЛЕНО: Изменено имя итоговой dll-ки
ENTRYPOINT ["dotnet", "Auth.WebApi.dll"]