# ---------- Stage 1: BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy RIÊNG các .csproj trước + restore → tận dụng cache layer của Docker.
# (Code đổi mà dependency không đổi → Docker tái dùng layer restore, build nhanh hơn nhiều)
COPY src/Ingestion/SmartMetering.Ingestion.Domain/*.csproj         src/Ingestion/SmartMetering.Ingestion.Domain/
COPY src/Ingestion/SmartMetering.Ingestion.Application/*.csproj    src/Ingestion/SmartMetering.Ingestion.Application/
COPY src/Ingestion/SmartMetering.Ingestion.Infrastructure/*.csproj src/Ingestion/SmartMetering.Ingestion.Infrastructure/
COPY src/Ingestion/SmartMetering.Ingestion.Api/*.csproj            src/Ingestion/SmartMetering.Ingestion.Api/
RUN dotnet restore src/Ingestion/SmartMetering.Ingestion.Api/SmartMetering.Ingestion.Api.csproj

# Giờ mới copy toàn bộ source rồi publish (biên dịch ra binary tối ưu)
COPY . .
RUN dotnet publish src/Ingestion/SmartMetering.Ingestion.Api/SmartMetering.Ingestion.Api.csproj \
    -c Release -o /app/publish

# ---------- Stage 2: RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
# Chiseled image: chạy non-root sẵn (an toàn cho K8s), cổng mặc định 8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartMetering.Ingestion.Api.dll"]