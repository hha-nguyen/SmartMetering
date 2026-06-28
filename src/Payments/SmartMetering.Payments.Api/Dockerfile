# ---------- Stage 1: BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Payments/SmartMetering.Payments.Domain/*.csproj         src/Payments/SmartMetering.Payments.Domain/
COPY src/Payments/SmartMetering.Payments.Application/*.csproj     src/Payments/SmartMetering.Payments.Application/
COPY src/Payments/SmartMetering.Payments.Infrastructure/*.csproj  src/Payments/SmartMetering.Payments.Infrastructure/
COPY src/Payments/SmartMetering.Payments.Api/*.csproj             src/Payments/SmartMetering.Payments.Api/
RUN dotnet restore src/Payments/SmartMetering.Payments.Api/SmartMetering.Payments.Api.csproj

COPY . .
RUN dotnet publish src/Payments/SmartMetering.Payments.Api/SmartMetering.Payments.Api.csproj \
    -c Release -o /app/publish

# ---------- Stage 2: RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartMetering.Payments.Api.dll"]
