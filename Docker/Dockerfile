FROM microsoft/dotnet:2.2-aspnetcore-runtime

COPY dist /app
COPY Utilities/node_modules/wait-for-it.sh/bin/wait-for-it /app/wait-for-it.sh

RUN chmod +x /app/wait-for-it.sh

WORKDIR /app

EXPOSE 80/tcp

# TODO: Consider providing what to wait for elsewhere
ENV WAITHOST1=ravendb1 WAITPORT1=8080
ENV WAITHOST2=ravendb2 WAITPORT2=8080
ENV WAITHOST3=ravendb3 WAITPORT3=8080
ENV WAITHOST4=mssql WAITPORT4=1433

ENTRYPOINT ./wait-for-it.sh $WAITHOST1:$WAITPORT1 --timeout=0 \
    && ./wait-for-it.sh $WAITHOST2:$WAITPORT2 --timeout=0 \
    && ./wait-for-it.sh $WAITHOST3:$WAITPORT3 --timeout=0 \
    && ./wait-for-it.sh $WAITHOST4:$WAITPORT4 --timeout=0 \
    && exec dotnet TicketManager.WebAPI.dll