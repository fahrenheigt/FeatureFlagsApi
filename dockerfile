FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FeatureFlagsApi/FeatureFlagsApi.csproj", "FeatureFlagsApi/"]
RUN dotnet restore "FeatureFlagsApi/FeatureFlagsApi.csproj"

COPY . .
RUN dotnet publish "FeatureFlagsApi/FeatureFlagsApi.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FeatureFlagsApi.dll"]