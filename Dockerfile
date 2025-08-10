# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia archivos esenciales primero
COPY ["MailMinimalApi/MailMinimalApi.csproj", "MailMinimalApi/"]
COPY ["MailMinimalApi/appsettings.json", "MailMinimalApi/"]
RUN dotnet restore "MailMinimalApi/MailMinimalApi.csproj"

# Copia el resto del código
COPY . .

# Publica la app
RUN dotnet publish "MailMinimalApi/MailMinimalApi.csproj" -c Release -o /app/publish

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/MailMinimalApi/appsettings.json .
ENTRYPOINT ["dotnet", "MailMinimalApi.dll"]