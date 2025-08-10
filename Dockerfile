FROM mcr.microsoft.comdotnetsdk8.0 AS build
WORKDIR src

# Copia solo los archivos necesarios para restaurar dependencias
COPY [MailMinimalApiMailMinimalApi.csproj, MailMinimalApi]
COPY [MailMinimalApiappsettings.json, MailMinimalApi]
RUN dotnet restore MailMinimalApiMailMinimalApi.csproj

# Copia todo el código fuente
COPY . .

# Publica la aplicación
RUN dotnet publish MailMinimalApiMailMinimalApi.csproj -c Release -o apppublish

FROM mcr.microsoft.comdotnetaspnet8.0 AS final
WORKDIR app
COPY --from=build apppublish .
COPY --from=build srcMailMinimalApiappsettings.json .
ENTRYPOINT [dotnet, MailMinimalApi.dll]