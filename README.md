Startup
=======

ServiceRegistry is a combination of middleware and services.
All configuration is done in your startup class.

Configuring services
^^^^^^^^^^^^^^^^^^^^
You add the ServiceRegistry services to the DI system by calling::

    public void ConfigureServices(IServiceCollection services)
    {
        var builder = services.AddServiceRegistry();
    }

Optionally you can pass in options into this call. See :ref:`here <refOptions>` for details on options.

This will return you a builder object that in turn has a number of convenience methods to wire up additional services.

In-Memory configuration stores
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The "in-memory" configuration APIs allow for configuring ServiceRegistry from an in-memory list of configuration objects.
These "in-memory" collections can be hard-coded in the hosting application, or could be loaded dynamically from a configuration file or a database.
By design, though, these collections are only created when the hosting application is starting up.

Use of these configuration APIs are designed for use when prototyping, developing, and/or testing where it is not necessary to dynamically consult database at runtime for the configuration data.

* ``AddInMemoryServices``
    Registers ``IServiceStore`` implementation based on the in-memory collection of ``Service`` configuration objects.
	
Additional services
^^^^^^^^^^^^^^^^^^^

* ``AddServiceStore``
    Adds ``IServiceStore`` implementation for reading and storing services (You can also use the already implemented versions for EntityFramework or Redis).

	
Caching
^^^^^^^

Service configuration data is used frequently by ServiceRegistry.
If this data is being loaded from a database or other external store, then it might be expensive to frequently re-load the same data.

* ``AddInMemoryCaching``
    To use any of the caches described below, an implementation of ``ICache<T>`` must be registered in DI.
    This API registers a default in-memory implementation of ``ICache<T>`` that's based on ASP.NET Core's ``MemoryCache``.

* ``AddServiceStoreCache``
    Registers a ``IServiceStore`` decorator implementation which will maintain an in-memory cache of ``Service`` configuration objects.
    The cache duration is configurable on the ``Caching`` configuration options on the ``ServiceRegistryOptions``.

Further customization of the cache is possible:

The default caching relies upon the ``ICache<T>`` implementation.
If you wish to customize the caching behavior for the specific configuration objects, you can replace this implementation in the dependency injection system.

The default implementation of the ``ICache<T>`` itself relies upon the ``IMemoryCache`` interface (and ``MemoryCache`` implementation) provided by .NET.
If you wish to customize the in-memory caching behavior, you can replace the ``IMemoryCache`` implementation in the dependency injection system.

Configuring the pipeline
^^^^^^^^^^^^^^^^^^^^^^^^
You need to add ServiceRegistry to the pipeline by calling::

    public void Configure(IApplicationBuilder app)
    {
        app.UseServiceRegistry();
    }

There is no additional configuration for the middleware.
