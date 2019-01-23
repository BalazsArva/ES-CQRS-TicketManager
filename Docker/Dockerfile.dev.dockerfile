FROM microsoft/dotnet:2.2-sdk

COPY Utilities/node_modules/wait-for-it.sh/bin/wait-for-it /tools/wait-for-it.sh

RUN chmod +x /tools/wait-for-it.sh

VOLUME /app

WORKDIR /app/TicketManager.WebAPI

EXPOSE 80/tcp

ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# TODO: Consider providing what to wait for elsewhere
ENV WAITHOST1=ravendb1 WAITPORT1=8080
ENV WAITHOST2=mssql WAITPORT2=1433

ENTRYPOINT dotnet restore \
    && /tools/wait-for-it.sh $WAITHOST1:$WAITPORT1 --timeout=0 \
    && /tools/wait-for-it.sh $WAITHOST2:$WAITPORT2 --timeout=0 \
    && dotnet watch run --environment=Development