
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CarRental.csproj ./
RUN dotnet restore ./CarRental.csproj

COPY . .
RUN dotnet publish ./CarRental.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/logs \
    /app/wwwroot/uploads \
    /app/wwwroot/files \
    /app/wwwroot/images/cars

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CarRental.dll"]