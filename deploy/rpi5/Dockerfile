# Image der GwiOS WebUI für den rpi5 (linux/arm64).
#
# Deploy-WebUI.ps1 veröffentlicht die WebUI vorher mit "dotnet publish" und
# übergibt den Ausgabeordner als Build-Kontext; hier wird nichts kompiliert.
# Gegenüber dem reinen Microsoft-Image kommt nur pg_dump dazu, das die
# Datensicherung im Admin-Bereich braucht.

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Hauptversion des PostgreSQL-Clients. pg_dump muss mindestens die Hauptversion
# des Servers haben, sonst verweigert es die Sicherung - Deploy-WebUI.ps1 setzt
# sie passend zum Postgres auf dem rpi5.
ARG POSTGRES_MAJOR_VERSION

# Die Pakete des Basis-Images enthalten nur den Client ihrer Distribution in
# einer festen Version. Das offizielle PostgreSQL-Archiv bietet jede
# Hauptversion an, so passt der Client zum Server.
USER root
RUN set -eux; \
    test -n "${POSTGRES_MAJOR_VERSION}"; \
    apt-get update; \
    apt-get install -y --no-install-recommends ca-certificates curl; \
    install -d /usr/share/postgresql-common/pgdg; \
    curl -fsSL -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc \
        https://www.postgresql.org/media/keys/ACCC4CF8.asc; \
    . /etc/os-release; \
    echo "deb [signed-by=/usr/share/postgresql-common/pgdg/apt.postgresql.org.asc] https://apt.postgresql.org/pub/repos/apt ${VERSION_CODENAME}-pgdg main" \
        > /etc/apt/sources.list.d/pgdg.list; \
    apt-get update; \
    apt-get install -y --no-install-recommends "postgresql-client-${POSTGRES_MAJOR_VERSION}"; \
    apt-get purge -y --auto-remove curl; \
    rm -rf /var/lib/apt/lists/*

# pg_dump direkt aus dem Ordner der Version, nicht über den Perl-Wrapper in
# /usr/bin, der nach installierten Clustern sucht.
ENV PATH="/usr/lib/postgresql/${POSTGRES_MAJOR_VERSION}/bin:${PATH}"

WORKDIR /app
COPY . .

# Derselbe Benutzer wie im bisherigen SDK-Image (UID 1654, Home /home/app):
# apply.sh legt das Schlüsselverzeichnis für genau diese UID an.
USER $APP_UID
ENTRYPOINT ["dotnet", "GwiOS.WebUI.dll"]
