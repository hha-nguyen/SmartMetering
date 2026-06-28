# ---------- Stage 1: BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Metering/SmartMetering.Metering.Domain/*.csproj         src/Metering/SmartMetering.Metering.Domain/
COPY src/Metering/SmartMetering.Metering.Application/*.csproj     src/Metering/SmartMetering.Metering.Application/
COPY src/Metering/SmartMetering.Metering.Infrastructure/*.csproj  src/Metering/SmartMetering.Metering.Infrastructure/
COPY src/Metering/SmartMetering.Metering.Api/*.csproj             src/Metering/SmartMetering.Metering.Api/
RUN dotnet restore src/Metering/SmartMetering.Metering.Api/SmartMetering.Metering.Api.csproj

COPY . .
RUN dotnet publish src/Metering/SmartMetering.Metering.Api/SmartMetering.Metering.Api.csproj \
    -c Release -o /app/publish

# ---------- Stage 2: RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartMetering.Metering.Api.dll"]
