# --- ETAPA 1: CONSTRUCCIÓN ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos primero el archivo de proyecto para optimizar caché
COPY ["ServiceTowerWeb.csproj", "./"]
RUN dotnet restore

# Copiamos todo lo demás y publicamos
COPY . .
RUN dotnet publish -c Release -o /app/out

# --- ETAPA 2: EJECUCIÓN ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Instalamos dependencias necesarias para PDF en Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libx11-6 \
    libxrender1 \
    libxext6 \
    libfontconfig1 \
    libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

# Copiamos la app publicada
COPY --from=build /app/out .

# --- SOLUCIÓN AL ERROR DE PERMISOS ---
USER root

# 1. Creamos la carpeta por si no existe y movemos el binario si es necesario
# 2. Damos permisos totales de ejecución a la carpeta de Rotativa
RUN chmod -R 755 /app/wwwroot/Rotativa && \
    chmod +x /app/wwwroot/Rotativa/Linux/wkhtmltopdf

# Volvemos al usuario de la app por seguridad
USER $APP_UID

ENTRYPOINT ["dotnet", "ServiceTowerWeb.dll"]