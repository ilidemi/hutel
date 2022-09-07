FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
COPY . /app
WORKDIR /app
EXPOSE 443/tcp
ENTRYPOINT dotnet server.dll --urls "http://*:80;http://*:3000;https://*:443"