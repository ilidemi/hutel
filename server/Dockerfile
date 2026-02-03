FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
COPY . /app
WORKDIR /app
EXPOSE 80/tcp
EXPOSE 443/tcp
ENTRYPOINT ["dotnet", "server.dll"]