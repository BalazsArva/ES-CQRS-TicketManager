# May have to replace "-1709" depending on host OS version
FROM microsoft/dotnet:2.2-aspnetcore-runtime
COPY dist /app
WORKDIR /app
EXPOSE 80/tcp
ENTRYPOINT ["dotnet", "TicketManager.WebAPI.dll"]