# Etapa base: runtime de ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Etapa de build: SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el .csproj con su ruta correcta y restaurar dependencias
COPY MailMinimalApi/MailMinimalApi.csproj MailMinimalApi/
RUN dotnet restore MailMinimalApi/MailMinimalApi.csproj

# Copiar el resto de los archivos
COPY . .

# Publicar la app
RUN dotnet publish MailMinimalApi/MailMinimalApi.csproj -c Release -o /app/publish

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MailMinimalApi.dll"]