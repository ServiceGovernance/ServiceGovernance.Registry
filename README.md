# ServiceGovernance.Registry

[![Build status](https://ci.appveyor.com/api/projects/status/iti2hroxya5fkbrw?svg=true)](https://ci.appveyor.com/project/twenzel/servicegovernance-registry)
[![NuGet Version](http://img.shields.io/nuget/v/ServiceGovernance.Registry.svg?style=flat)](https://www.nuget.org/packages/ServiceGovernance.Registry/)
[![License](https://img.shields.io/badge/license-Apache-blue.svg)](LICENSE)

ServiceRegistry is a combination of middleware and services.
All configuration is done in your startup class.

## Usage

You add the ServiceRegistry services to the DI system by calling:

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddServiceRegistry();
}

public void Configure(IApplicationBuilder app)
{
    ...
    app.UseServiceRegistry();
    ...
    app.UseMvc();
}
```

Optionally you can pass in options into this call.

This will return you a builder object that in turn has a number of convenience methods to wire up additional services.

## In-Memory stores

The "in-memory" configuration APIs allow for configuring ServiceRegistry from an in-memory list of configuration objects.
These "in-memory" collections can be hard-coded in the hosting application, or could be loaded dynamically from a configuration file or a database.
By design, though, these collections are only created when the hosting application is starting up.

Use of these configuration APIs are designed for use when prototyping, developing, and/or testing where it is not necessary to dynamically consult database at runtime for the configuration data.

* `AddInMemoryStore`
    Registers `IServiceStore` implementation storing services as in-memory list. Optional arguments are services which will be added to the store.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddServiceRegistry()
        .AddInMemoryStore();
}
```

## Other stores

Available persistence libraries are:

* [Entity Framework](https://github.com/ServiceGovernance/ServiceGovernance.Registry.EntityFramework)
* [Redis](https://github.com/ServiceGovernance/ServiceGovernance.Registry.Redis)

## Additional services

* `AddServiceStore`
    Adds `IServiceStore` implementation for reading and storing services (You can also use the already implemented versions for EntityFramework or Redis).

## Caching

Service configuration data is used frequently by ServiceRegistry.
If this data is being loaded from a database or other external store, then it might be expensive to frequently re-load the same data.

* `AddInMemoryCaching`
    To use any of the caches described below, an implementation of `ICache<T>` must be registered in DI.
    This API registers a default in-memory implementation of `ICache<T>` that's based on ASP.NET Core's `MemoryCache`.

* `AddServiceStoreCache`
    Registers a `IServiceStore` decorator implementation which will maintain an in-memory cache of `Service` configuration objects.
    The cache duration is configurable on the `Caching` configuration options on the `ServiceRegistryOptions`.

Further customization of the cache is possible:

The default caching relies upon the `ICache<T>` implementation.
If you wish to customize the caching behavior for the specific configuration objects, you can replace this implementation in the dependency injection system.

The default implementation of the `ICache<T>` itself relies upon the `IMemoryCache` interface (and `MemoryCache` implementation) provided by .NET.
If you wish to customize the in-memory caching behavior, you can replace the `IMemoryCache` implementation in the dependency injection system.

## UI

There's no built-in UI to show the registered services. It's fairly easy to build one by yourself using the `IServiceStore`. Please have a look at the [sample](https://github.com/ServiceGovernance/ServiceGovernance.Registry/blob/master/samples/Registry/Controllers/HomeController.cs).

## APIs

Following APIs are provided by this library:

### Register service

This endpoint registers a service in the registry.

|Url|Method|Type
|-|-|-|
|/v1/register|POST|application/json

### Parameter

```json
{
    "serviceId": "UniqueServiceId", //required
    "displayName": "A human friendly display name",
    "endpoints": ["https://myservice01-qa.com"], //required
    "ipAddress": "10.10.0.1",
    "publicUrls": ["https://myserviceurl-qa.com"]
}
```

### Response

`text/plain` HTTP 200

The endpoint returns a token which should be used to unregister the service.

### Unregister service

This endpoint removes a service registration.

|Url|Method|Type
|-|-|-|
|/v1/register|DELETE|text/plain

### Parameter

The token returned from the service registration call.

```plain
dfg54dfg54df3g21df53g4df3g54
```

### Response

`text/plain` HTTP 200

### Get service

This endpoint returns the registered service.

|Url|Method|Type
|-|-|-|
|/v1/service/{serviceId}|Get|

### Parameter
`serviceid` The service you want to retrieve

### Response

`application/json` HTTP 200

The endpoint returns the registered service.
> If no public url was registered, the endpoints will be published as public urls.

```json
{
    "serviceId": "UniqueServiceId",
    "displayName": "A human friendly display name",
    "endpoints": ["http://myserviceurl01.com", "http://myserviceurl02.com"],
    "ipAddresses": ["10.10.0.1", "10.10.0.2"],
    "publicUrls": ["https://myserviceurl.com"]
}
```

### Get all services

This endpoint returns all registered services.

|Url|Method|Type
|-|-|-|
|/v1/service/Get|

### Parameter

### Response

`application/json` HTTP 200

The endpoint returns all registered services.

```json
[
    {
        "serviceId": "UniqueServiceId",
        "displayName": "A human friendly display name",
        "serviceEndpoints": ["http://myserviceurl01.com", "http://myserviceurl02.com"],
        "ipAddresses": ["10.10.0.1", "10.10.0.2"],
        "publicUrls": ["https://myserviceurl.com"]
    },
    {
        ...
    }
]
```