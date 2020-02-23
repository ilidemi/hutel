FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim
COPY . /app
WORKDIR /app
EXPOSE 5000/tcp
ENTRYPOINT dotnet server.dll --server.urls "http://*:5000"