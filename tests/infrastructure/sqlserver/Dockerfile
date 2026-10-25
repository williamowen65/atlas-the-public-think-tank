# The stock SQL Server image does not guarantee the optional full-text package.
FROM mcr.microsoft.com/mssql/server:2022-latest
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl gnupg

RUN . /etc/os-release \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/atlas-microsoft.gpg \
    && if ! grep -Rqs '/mssql-server-2022' /etc/apt/sources.list.d/; then \
         echo "deb [arch=amd64 signed-by=/usr/share/keyrings/atlas-microsoft.gpg] https://packages.microsoft.com/ubuntu/${VERSION_ID}/mssql-server-2022 ${VERSION_CODENAME} main" > /etc/apt/sources.list.d/atlas-mssql-fts.list; \
       fi

# Some Microsoft base images contain the engine without a dpkg registration.
# Select both packages from the same repository version instead of querying dpkg.
RUN apt-get update \
    && sql_server_version="$(apt-cache policy mssql-server-fts | sed -n 's/^  Candidate: //p')" \
    && test -n "$sql_server_version" && test "$sql_server_version" != '(none)' \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends "mssql-server=${sql_server_version}" "mssql-server-fts=${sql_server_version}" \
    && rm -rf /var/lib/apt/lists/*
USER mssql
