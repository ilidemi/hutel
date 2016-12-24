Human Telemetry
===

To run the project, grab the latest [.NET Core](https://www.microsoft.com/net/core) and run
```
$ dotnet restore
$ dotnet run
```
in the /server folder.

To add a new point to in memory storage, issue a HTTP request:
```
POST http://localhost:5000/api/points/7fdbd84a-2405-4c6c-8b12-e72c582e4b37
content-type:application/json

{
    'id': '7fdbd84a-2405-4c6c-8b12-e72c582e4b37',
    'tagId': 'testtag',
    'date': '2016-12-19T21:07:31Z',
    'int': 1337,
    'dateTime': '2016-12-20T21:07:31Z',
    'string': 'abacaba',
    'float': 1337.0
}
```