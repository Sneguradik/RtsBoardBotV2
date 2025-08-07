FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

USER root

# Устанавливаем FreeTDS и ODBC в рантайм-образ
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        unixodbc \
        unixodbc-dev \
        freetds-bin \
        freetds-dev \
        tdsodbc \
    && rm -rf /var/lib/apt/lists/*

# Копируем конфиги FreeTDS и ODBC
# Положи их рядом с Dockerfile: freetds.conf, odbc.ini, odbcinst.ini
COPY RobotWorker/freetds.conf /etc/freetds/freetds.conf
COPY RobotWorker/odbcinst.ini /etc/odbcinst.ini
COPY RobotWorker/odbc.ini /etc/odbc.ini


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RobotWorker/RobotWorker.csproj", "RobotWorker/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Tinkoff.InvestApi/Tinkoff.InvestApi.csproj", "Tinkoff.InvestApi/"]
RUN dotnet restore "RobotWorker/RobotWorker.csproj"
COPY . .
WORKDIR "/src/RobotWorker"
RUN dotnet build "./RobotWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RobotWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RobotWorker.dll"]
