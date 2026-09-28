FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/ConferenceHallBooking.Domain/ConferenceHallBooking.Domain.csproj ./src/ConferenceHallBooking.Domain/
COPY src/ConferenceHallBooking.Application/ConferenceHallBooking.Application.csproj ./src/ConferenceHallBooking.Application/
COPY src/ConferenceHallBooking.Infrastructure/ConferenceHallBooking.Infrastructure.csproj ./src/ConferenceHallBooking.Infrastructure/
COPY src/ConferenceHallBooking.Api/ConferenceHallBooking.Api.csproj ./src/ConferenceHallBooking.Api/
RUN dotnet restore src/ConferenceHallBooking.Api/ConferenceHallBooking.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/ConferenceHallBooking.Api/ConferenceHallBooking.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

LABEL org.opencontainers.image.title="ConferenceHallBooking" \
      org.opencontainers.image.description="Conference hall booking REST API (.NET 8, Clean Architecture, PostgreSQL)"

ENV DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ConferenceHallBooking.Api.dll"]
