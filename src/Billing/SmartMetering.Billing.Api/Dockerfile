# ---------- Stage 1: BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Billing/SmartMetering.Billing.Domain/*.csproj         src/Billing/SmartMetering.Billing.Domain/
COPY src/Billing/SmartMetering.Billing.Application/*.csproj     src/Billing/SmartMetering.Billing.Application/
COPY src/Billing/SmartMetering.Billing.Infrastructure/*.csproj  src/Billing/SmartMetering.Billing.Infrastructure/
COPY src/Billing/SmartMetering.Billing.Api/*.csproj             src/Billing/SmartMetering.Billing.Api/
RUN dotnet restore src/Billing/SmartMetering.Billing.Api/SmartMetering.Billing.Api.csproj

COPY . .
RUN dotnet publish src/Billing/SmartMetering.Billing.Api/SmartMetering.Billing.Api.csproj \
    -c Release -o /app/publish

# ---------- Stage 2: RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartMetering.Billing.Api.dll"]
