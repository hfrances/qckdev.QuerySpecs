[![NuGet Version](https://img.shields.io/nuget/v/qckdev.QuerySpecs.svg)](https://www.nuget.org/packages/qckdev.QuerySpecs)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=qckdev.QuerySpecs&metric=alert_status)](https://sonarcloud.io/dashboard?id=qckdev.QuerySpecs)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=qckdev.QuerySpecs&metric=coverage)](https://sonarcloud.io/dashboard?id=qckdev.QuerySpecs)
![Azure Pipelines Status](https://hfrances.visualstudio.com/Main/_apis/build/status/qckdev.QuerySpecs?branchName=master)

# qckdev.QuerySpecs

Toolkit for query-spec contracts and execution flow across Application, Persistence, and Infrastructure layers.

## 📦 Packages

This repository contains the following packable libraries:

- `qckdev.QuerySpecs`: runtime engine + DI registration.
- `qckdev.QuerySpecs.Abstractions`: contracts and shared query models.

## 🛠️ Installation

Install the runtime package in applications:

```bash
dotnet add package qckdev.QuerySpecs
```

Install only contracts when a project should not depend on the runtime engine:

```bash
dotnet add package qckdev.QuerySpecs.Abstractions
```

## ⚡ Quick Start

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

```csharp
var specs = querySpecsEngine.Parse<GetAllQuery, GetAllQuerySpecs>(request);
```

```csharp
var query = querySpecsEngine.Apply<GetAllQuerySpecs, IQueryable<MyEntity>>(dbSet, specs);
var path = querySpecsEngine.Apply<GetAllQuerySpecs, string>("/entities", specs);
```

## Processor Contract

The engine executes processors in this order:

1. `Validate(specs)`
2. `Apply(target, specs)`

```csharp
using qckdev.QuerySpecs;

internal sealed class GetAllQuerySpecsProcessor : IQuerySpecsProcessor<GetAllQuerySpecs, string>
{
    public void Validate(GetAllQuerySpecs specs)
    {
        if (specs.Page.HasValue && specs.Page.Value <= 0)
        {
            throw new ArgumentException("Page must be greater than 0.", nameof(specs));
        }
    }

    public string Apply(string target, GetAllQuerySpecs specs)
    {
        return specs.Page.HasValue
            ? $"{target}?page={specs.Page.Value}"
            : target;
    }
}
```

## Recommended Implementation Sequence

1. Define input query (`GetAllQuery`).
2. Define internal specs contract (`GetAllQuerySpecs`).
3. Configure `cfg.Map` for input-to-specs conversion.
4. Implement `IQuerySpecsProcessor<GetAllQuerySpecs, TTarget>`.
5. Put specs validation in `Validate` and transformation in `Apply`.
6. Use `Parse` in handlers and `Apply` in repositories/adapters.

## 🤝 Contributing
Issues and pull requests are welcome! See the contribution guidelines (coming soon).

## 📜 License
This project is licensed under the terms of the [MIT License](LICENSE).
