[![NuGet Version](https://img.shields.io/nuget/v/qckdev.QuerySpecs.svg)](https://www.nuget.org/packages/qckdev.QuerySpecs)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=qckdev.QuerySpecs&metric=alert_status)](https://sonarcloud.io/dashboard?id=qckdev.QuerySpecs)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=qckdev.QuerySpecs&metric=coverage)](https://sonarcloud.io/dashboard?id=qckdev.QuerySpecs)
![Azure Pipelines Status](https://hfrances.visualstudio.com/Main/_apis/build/status/qckdev.QuerySpecs?branchName=master)

# qckdev.QuerySpecs

`qckdev.QuerySpecs` standardizes query-spec workflows around two operations:

1. `Parse`: map an incoming request (`TSource`) to a specs contract (`TSpecs`).
2. `Apply`: execute specs rules against a target (`TTarget`) through `IQuerySpecsProcessor<TSpecs, TTarget>`.

## Package layout

This repository ships two NuGet packages:

1. `qckdev.QuerySpecs.Abstractions`
2. `qckdev.QuerySpecs`

`qckdev.QuerySpecs` is the package most consumers should install. It includes:

1. Runtime engine (`IQuerySpecsEngine` implementation)
2. Dependency injection registration (`AddQuerySpecs`)
3. A dependency on `qckdev.QuerySpecs.Abstractions`

`qckdev.QuerySpecs.Abstractions` includes:

1. Core contracts (`IQuerySpecsEngine`, `IQuerySpecsProcessor<,>`)
2. Configuration contracts (`QuerySpecsConfiguration`, `QuerySpecsEngineOptions`)
3. Optional collection contracts (`CollectionQuery`, `CollectionQuerySpecs`)

## Installation

```bash
dotnet add package qckdev.QuerySpecs
```

If you only need contracts in a shared domain/application boundary:

```bash
dotnet add package qckdev.QuerySpecs.Abstractions
```

## Quick start

### 1) Register QuerySpecs in DI

```csharp
using Microsoft.Extensions.DependencyInjection;
using qckdev.QuerySpecs;

services.AddQuerySpecs(cfg =>
{
    cfg.Map = (sp, source, destinationType) =>
    {
        var mapper = sp.GetRequiredService<AutoMapper.IMapper>();
        return mapper.Map(source, source.GetType(), destinationType);
    };

    cfg.RegisterServicesFromAssemblyContaining<GetAllQuerySpecsProcessor>();
});
```

### 2) Parse in application handlers

```csharp
var specs = querySpecsEngine.Parse<GetAllQuery, GetAllQuerySpecs>(request);
```

### 3) Apply in persistence or infrastructure

```csharp
// Persistence: shape IQueryable<TEntity>
var query = querySpecsEngine.Apply<GetAllQuerySpecs, IQueryable<MyEntity>>(dbSet, specs);

// Infrastructure: build URL path/query
var path = querySpecsEngine.Apply<GetAllQuerySpecs, string>("/entities", specs);
```

## Processor example

```csharp
using qckdev.QuerySpecs;

internal sealed class GetAllQuerySpecsProcessor : IQuerySpecsProcessor<GetAllQuerySpecs, string>
{
    public string Apply(string target, GetAllQuerySpecs specs)
    {
        if (specs.Page.HasValue && specs.Page.Value <= 0)
        {
            throw new ArgumentException("Page must be greater than 0.", nameof(specs));
        }

        return specs.Page.HasValue
            ? $"{target}?page={specs.Page.Value}"
            : target;
    }
}
```

## Guidance for developers

Recommended implementation sequence for list endpoints:

1. Define `GetAllQuery` (input contract)
2. Define `GetAllQuerySpecs` (internal specs contract)
3. Configure `cfg.Map` for query -> specs mapping
4. Implement `IQuerySpecsProcessor<GetAllQuerySpecs, TTarget>` in the target layer
5. Keep specs validation in `Apply`
6. Use `Parse` in handlers and `Apply` in repositories/adapters

## Tests

This repository includes unit tests for:

1. Parse behavior (valid mapping and configuration errors)
2. Apply behavior (processor resolution and execution)
3. Assembly scanning through `AddQuerySpecs`
4. Runtime overloads and parse-into-existing-instance behavior

## License

MIT
