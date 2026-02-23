# Jameak.RequestAuthorization
[![CI](https://github.com/Jameak/RequestAuthorization/actions/workflows/ci.yml/badge.svg)](https://github.com/Jameak/RequestAuthorization/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A composable, framework-agnostic request authorization library for mediator-style pipelines.

This library provides a structured way to:
* Define explicit authorization requirements per request
* Compose requirements into complex logical trees
* Evaluate them through a consistent authorization pipeline

And it integrates with any mediator-style pipeline and is NativeAOT compatible.

## Packages
| Package | NuGet | Dependency |
| ------- | ----- | -----------|
| Jameak.RequestAuthorization.Core | ![NuGet](https://img.shields.io/nuget/v/Jameak.RequestAuthorization.Core.svg?label=NuGet) | Microsoft.Extensions.DependencyInjection.Abstractions<ul><li>≥ 10.0.0</li></ul> |
| Jameak.RequestAuthorization.Adapter.AspNetCore | ![NuGet](https://img.shields.io/nuget/v/Jameak.RequestAuthorization.Adapter.AspNetCore.svg?label=NuGet) | Microsoft.AspNetCore.Authorization<ul><li>≥ 10.0.0</li></ul>Microsoft.AspNetCore.Http.Abstractions<ul><li>≥ 2.3.0</li></ul> |
| Jameak.RequestAuthorization.Adapter.Mediator | ![NuGet](https://img.shields.io/nuget/v/Jameak.RequestAuthorization.Adapter.Mediator.svg?label=NuGet) | [Mediator.Abstractions](https://github.com/martinothamar/Mediator)<ul><li>≥ 3.0.1</li></ul> |
| Jameak.RequestAuthorization.Adapter.MediatR | ![NuGet](https://img.shields.io/nuget/v/Jameak.RequestAuthorization.Adapter.MediatR.svg?label=NuGet) | [MediatR](https://github.com/LuckyPennySoftware/MediatR)<ul><li>≥ 12.5.0</li></ul> |

Need to integrate with a different mediator-style pipeline library? See [this section](#writing-your-own-mediator-adapter) for details on how.

## Getting started
Authorization in this library consists of 3 parts:
1. A **requirement**: A data object that describes an authorization rule.
2. A **requirement builder**: Translates a request into a set of requirements that the caller must satisfy.
3. A **requirement handler**: Evaluates a requirement.

### Our basic Mediator request
We have an endpoint that lets us get the contents of a document:

The request type:
```csharp
public record GetDocumentRequest(Guid DocumentId) : IRequest<GetDocumentResponse>;
```

The response type:
```csharp
public record GetDocumentResponse(string Content);
```

The basic Mediator request handler:
```csharp
public class GetDocumentRequestHandler : IRequestHandler<GetDocumentRequest, GetDocumentResponse>
{
    private readonly IDocumentService _documentService;

    public GetDocumentRequestHandler(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public async ValueTask<GetDocumentResponse> Handle(
        GetDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocument(request.DocumentId);
        return new GetDocumentResponse(document.Content);
    }
}
```

All of the above is standard Mediator code, where our request, response, and handler classes remain free of authorization concerns. 

### Creating an authorization requirement
A requirement represents a single authorization rule. They are simple data objects that contain no logic.

```csharp
public record HasReadAccessToDocumentRequirement(
    Guid DocumentId,
    Guid UserId)
    : IRequestAuthorizationRequirement;
```

> [!NOTE]
> A requirement doesn't need to have any data or properties.


### Creating a requirement builder
A requirement builder translates a request into one or more requirements. The builder produces requirements that the caller must satisfy but does not evaluate authorization.

Requirement builders must implement the `IRequestAuthorizationRequirementBuilder<T>` interface where `T` is the request-type that the builder creates requirements for.
```csharp
public class GetDocumentRequestRequirementBuilder
    : IRequestAuthorizationRequirementBuilder<GetDocumentRequest>
{
    private readonly IUserAccessor _userAccessor;

    public GetDocumentRequestRequirementBuilder(IUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        GetDocumentRequest request,
        CancellationToken token)
    {
        IRequestAuthorizationRequirement requirement = new HasReadAccessToDocumentRequirement(
            request.DocumentId,
            _userAccessor.CurrentUserId);

        return Task.FromResult(requirement);
    }
}
```

### Creating a requirement handler
A requirement handler evaluates a specific requirement. They contain authorization logic and return explicit success or failure.

Requirement handlers must inherit from the `RequestAuthorizationHandlerBase<T>` base-class where `T` is the handled requirement.
```csharp
public class HasReadAccessToDocumentRequirementHandler
    : RequestAuthorizationHandlerBase<HasReadAccessToDocumentRequirement>
{
    private readonly IAuthService _authService;

    public HasReadAccessToDocumentRequirementHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(
        HasReadAccessToDocumentRequirement requirement,
        CancellationToken token)
    {
        if (await _authService.UserCanAccessDocument(
            requirement.UserId,
            requirement.DocumentId))
        {
            return RequestAuthorizationResult.Success(requirement);
        }

        return RequestAuthorizationResult.Fail(
            requirement,
            failureReason: "User is not allowed to access document");
    }
}
```

### Dependency injection registration
You will need to register...
* the core library services via `AddRequestAuthorizationCore`
* your requirement builder types via `AddRequirementBuilderType`
* your requirement handler types via `AddRequirementHandlerType`
* as well as the correct pipeline adapter for your mediator-library of choice.

```csharp
builder.Services
    .AddRequestAuthorizationCore()
    .AddRequirementBuilderType<GetDocumentRequestRequirementBuilder, GetDocumentRequest>()
    .AddRequirementHandlerType<HasReadAccessToDocumentRequirementHandler, HasReadAccessToDocument>();
```

Alternatively, you can scan an assembly for requirement handlers and builders:
```csharp
builder.Services
    .AddRequestAuthorizationCore()
    .AddRequirementBuilderTypesFromAssembly(typeof(Program).Assembly)
    .AddRequirementHandlerTypesFromAssembly(typeof(Program).Assembly);
```

> [!IMPORTANT]
> To register the authorization pipeline for your mediator-library of choice, see the [Adapters section](#adapters).


## Global requirements
The library supports global requirements that are evaluated for every requests. Global requirement builders must implement the `IGlobalRequestAuthorizationRequirementBuilder` interface.

```
public class MustBeAuthenticatedRequirementBuilder : IGlobalRequestAuthorizationRequirementBuilder
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(
        TRequest request,
        CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new MustBeAuthenticatedRequirement());
    }
}
```

Register the global builder like so:
```csharp
builder.Services
    .AddRequestAuthorizationCore()
    .AddGlobalRequirementBuilderType<MustBeAuthenticatedRequirementBuilder>();
```

## Requirement composition
Requirements can be composed, enabling more complex authorization checks. The library provides built-in composition primitives:

- `Require.All` (logical AND)
- `Require.Any` (logical OR)

Evaluation is short-circuiting and preserves the evaluation tree, which makes complex authorization failures easy to diagnose.

You can compose requirements like so:
```csharp
var authenticatedAndHasWriteScope = Require.All(
    new AuthenticatedRequirement(),
    new ScopeRequirement("orders:write"));

var managerOrAdminRequirement = Require.Any(
    new RoleRequirement("Admin"),
    new RoleRequirement("Manager"));
```

> [!TIP]
> To debug complex composed requirements, you can export a `RequestAuthorizationResult` to a Graphviz graph for visualization using the `AuthorizationDiagnosticExporter`-class.

## Overwriting the default authorized/unauthorized behavior.
The library evaluates requirements and produces a `RequestAuthorizationResult`. You can customize what happens when authorization succeeds or fails.

By default, when a requirement is not met, the default behavior is to throw an `UnauthorizedException`. You can change this by creating a class which implements the `IUnauthorizedResultHandler` interface. You may want to override the default unauthorized behavior if you need to throw a different exception type, or if you're using a discriminated union library to return strongly-typed errors.

When all requirements are met, the default behavior is to do nothing and pass the request along in the pipeline. You can change this by creating a class which implements the `IAuthorizedResultHandler` interface. This allows global side-effects to occur after authorization succeeds, such as logging or auditing where access to the successful `RequestAuthorizationResult` is desired. Note that this handler does not influence the response and should not throw to alter authorization outcomes.

To make the library use your custom handlers, add them like so during dependency injection registration:
```csharp
builder.Services.AddRequestAuthorizationCore()
    .WithAuthorizedResultHandler<CustomizedAuthorizedHandler>()
    .WithUnauthorizedResultHandler<CustomizedUnauthorizedHandler>()
```

## Adapter libraries
The library contains several adapters to integrate the core framework-agnostic request authorization with your mediator-framework of choice.

### Mediator.SourceGenerator adapter
To integrate with the [Mediator.SourceGenerator](https://github.com/martinothamar/Mediator) library, add a reference to the `Jameak.RequestAuthorization.Adapter.Mediator` package and register the Mediator-pipeline adapters as shown below.

Note that the default service-lifetime of Mediator is `Singleton` while the default service-lifetime of this library is `Scoped`. You must decide which lifetime is correct for your usage and configure the same lifetime in both libraries. For example, to configure both as `ServiceLifetime.Scoped`:
```csharp
// Configuring Mediator.SourceGenerator lifetime:
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Configuring lifetime for this library and registering the adapter:
builder.Services
    .AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Scoped)
    .AddMediatorPipelineAdapter();
```

> [!IMPORTANT]
> When using NativeAOT with Mediator, you must instead register the pipeline behaviors in the `AddMediator(...)` call as described in the Mediator documentation. See the xmldoc documentation [on this class TODO LINK]() for more information.

### MediatR adapter
To integrate with the [MediatR](https://github.com/LuckyPennySoftware/MediatR) library, add a reference to the `Jameak.RequestAuthorization.Adapter.MediatR` package and register the MediatR-pipeline adapters like so:
```csharp
builder.Services.AddRequestAuthorizationCore()
    .AddMediatRPipelineAdapter();
```

### ASP.NET Core adapter
If you're already using ASP.NET Core `IAuthorizationRequirement`s or Policies to handle authorization in other parts of your application, you can plug these into the request authorization pipeline via the `Jameak.RequestAuthorization.Adapter.AspNetCore` package.

Register the adapters like so:
```csharp
builder.Services.AddRequestAuthorizationCore()
    .AddAspNetAdapter();
```

And then use `AspNetAuthorizationRequirement` or `AspNetAuthorizationPolicyRequirement` to wrap your existing ASP.NET Core authorization logic.

> [!NOTE]
> You must also register a `IHttpContextAccessor` in your dependency container. This is usually done via the standard `builder.Services.AddHttpContextAccessor()` ASP.NET Core extension method.

### Writing your own mediator adapter
To integrate with another mediator library, simply wrap the core `IAuthorizationPipelineStep` inside a pipeline behavior appropriate for your framework. See [TODO LINK]() and [TODO LINK]() for examples.

## Why use this library rather than standard ASP.NET Core authorization
`Jameak.RequestAuthorization` is designed:
* For application-layer request pipelines
* For CQRS / mediator-based architectures
* For fine-grained, strongly-typed, per-request authorization composition
* To be independent of transport (HTTP, messaging, tests, background workers)

ASP.NET Core authorization works well for HTTP endpoints with simple role-/claim-/policy-based requirements. However, for more complex resource-based authorization needs, ASP.NET Core requires you to explicitly invoke authorization via `IAuthorizationService`:
```csharp
var result = await _authorizationService.AuthorizeAsync(user, resource, policy);

if (!result.Succeeded)
{
    return Forbid();
}
```

This approach has several downsides:
* Every handler must remember to call `AuthorizeAsync`
* Every handler must remember to check the result
* There is no framework-level enforcement that authorization was performed

`Jameak.RequestAuthorization` removes this risk by integrating authorization directly into the mediator pipeline. This ensures that:
* Authorization is a first-class part of request execution
* Authorization is enforced consistently for every request
* **Every** request processed by the pipeline has an associated authorization requirement
