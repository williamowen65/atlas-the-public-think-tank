# The stock SQL Server image does not guarantee the optional full-text package.
FROM mcr.microsoft.com/mssql/server:2022-latest
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl gnupg \
    && . /etc/os-release \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/atlas-microsoft.gpg \
    && if ! grep -Rqs '/mssql-server-2022' /etc/apt/sources.list.d/; then \
         echo "deb [arch=amd64 signed-by=/usr/share/keyrings/atlas-microsoft.gpg] https://packages.microsoft.com/ubuntu/${VERSION_ID}/mssql-server-2022 ${VERSION_CODENAME} main" > /etc/apt/sources.list.d/atlas-mssql-fts.list; \
       fi \
    && apt-get update \
    && sql_server_version="$(dpkg-query -W -f='${Version}' mssql-server)" \
    && apt-get install -y --no-install-recommends "mssql-server-fts=${sql_server_version}" \
    && rm -rf /var/lib/apt/lists/*
USER mssql
