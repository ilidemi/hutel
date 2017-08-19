FROM microsoft/dotnet:2.0-runtime
COPY . /app
WORKDIR /app
EXPOSE 5000/tcp
ENTRYPOINT dotnet server.dll --server.urls "http://*:5000"