The one with the tag config storage research and point schema validation
===

## Persistent storage facilities

ASP.NET Core discourages having a state in a web app as it contradicts REST guidelines. The provided storage facilities are quite limited, here's the full list:
* Per-request storage in HttpContext.Items
* Per-session storage on client side: cookies, query string
* Per-session storage on server side: distributed session cache in which the framework matches client's cookie with the cache instance and makes sure that the distributed part is handled properly, but developer must provide an external storage for backing the cache
* Global read-only app config
* Global in memory cache backed by an external storage
* Raw global external storage

For storing the tags configuration, only the three last options fit. Although, in the future we want to provide the api for the client to modify that config, so it's unreasonable to choose the read-only option now. We'll go for an external (file) storage with in memory caching. We'll stick with the same option for the points storage.

## Validating mixed json models

The main goal of our little project is to provide humans with an easy way to track arbitrary measurement points throughout the day. Point is transmitted as a json with two known fields (tag id and date) and a set of fields whose names and values are only known at runtime.

There are several ways this kind of data structure can be represented in C#:
* JObjects, with all validation coded by hand. This solution adds a lot of noise to the code and makes the type system highly dependent on a particular data type implementation from a third party library.
* Dynamic objects, with the known fields validated in the code and unknown fields and types being checked via reflection. The only downside is the manual validation for the fields known at compile time.
* DynamicObject descendants with known and unknown fields, where the unknown fields validation is handled through reflection. This approach has troubles with deserialization, as JSON.NET also uses reflection to set the values and ignores the helper methods for setting the unknown fields.
* Ordinary class with two fields and a dictionary with a `JsonExtensionData` attribute. Validation for the known fields is handled by JSON deserializer, unknown fields are checked using reflection. This option seems to be the best candidate so far.

ASP.NET Core encourages the developer to leave the model validation to the framework and only work with deserialized models in controller actions. To act on model validation issues, one either has to check the `ModelState` in the controller actions by hand or to define an action filter which can be reused across actions. ASP.NET Core provides a way to return all model validation error messages at once, but it doesn't work well with the kind of errors JSON.NET throws on missing fields, so for now our action filter will return an empty `BadRequestResult` if any issues are encountered. This can be improved by overriding the serializer of JSON.NET exceptions, which on the other hand increases code complexity and may cause sensitive data leakage, so such decision is to be taken carefully.

## Type checking

For our purposes, only a handful of types is needed for the point fields: string, int, double, date, time and custom enums. Sounds simple, but there's a catch: there's no standartized date format in JSON, so by convention dates are usually passed as strings in ISO 8601 format. Moreover, the Point objects are required to contain a calendar date, and there is no standardized way to encode a date without time or time of a day, and that forces us to come up with our own conventions. C# type system doesn't have the date separation as well, so even with the aid of reflection a custom type description and validation system is needed. As a good starting point, we'll stick to the C# type checks and introduce the custom types later on.
