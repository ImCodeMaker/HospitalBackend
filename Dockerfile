FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY HospitalAppServer.sln ./
COPY HospitalApp.Core.Domain/HospitalApp.Core.Domain.csproj HospitalApp.Core.Domain/
COPY HospitalApp.Core.Application/HospitalApp.Core.Application.csproj HospitalApp.Core.Application/
COPY HospitalApp.Infrastructure.Identity/HospitalApp.Infrastructure.Identity.csproj HospitalApp.Infrastructure.Identity/
COPY HospitalApp.Infrastructure.Persistence/HospitalApp.Infrastructure.Persistence.csproj HospitalApp.Infrastructure.Persistence/
COPY HospitalApp.Infrastructure.Shared/HospitalApp.Infrastructure.Shared.csproj HospitalApp.Infrastructure.Shared/
COPY HospitalApp.WebAPI/HospitalApp.WebAPI.csproj HospitalApp.WebAPI/

RUN dotnet restore HospitalApp.WebAPI/HospitalApp.WebAPI.csproj

COPY . .
RUN dotnet publish HospitalApp.WebAPI/HospitalApp.WebAPI.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HospitalApp.WebAPI.dll"]
