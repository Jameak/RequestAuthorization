using System.Runtime.CompilerServices;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.MediatR.Tests;

public class MediatRIntegrationTests
{
    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public static async Task SampleRequest_RunPipeline_SuccessRequirementProducesResult(ServiceLifetime serviceLifetime)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = serviceLifetime;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleRequestSuccessRequirementBuilder, SampleRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleRequest();

        // Act & Assert
        var result = await mediator.Send(requestObj); // Should not throw auth error
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public static async Task SampleRequest_RunPipeline_FailingRequirementProducesUnauthException(ServiceLifetime serviceLifetime)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = serviceLifetime;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleRequestFailureRequirementBuilder, SampleRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleRequest();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(async () => await mediator.Send(requestObj));
    }

    public record SampleRequest() : IRequest<SampleResponse>;

    public record SampleResponse();

    public class SampleRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
    {
        public Task<SampleResponse> Handle(SampleRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SampleResponse());
        }
    }

    public class SampleRequestSuccessRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            SampleRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
        }
    }

    public class SampleRequestFailureRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            SampleRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysFailureRequirement());
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public static async Task SampleVoidRequest_RunPipeline_SuccessRequirementProducesResult(ServiceLifetime serviceLifetime)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = serviceLifetime;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleVoidRequestSuccessRequirementBuilder, SampleVoidRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleVoidRequest();

        // Act & Assert
        await mediator.Send(requestObj); // Should not throw auth error
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public static async Task SampleVoidRequest_RunPipeline_FailingRequirementProducesUnauthException(ServiceLifetime serviceLifetime)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = serviceLifetime;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleVoidRequestFailureRequirementBuilder, SampleVoidRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleVoidRequest();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(async () => await mediator.Send(requestObj));
    }

    public record SampleVoidRequest() : IRequest;

    public class SampleVoidRequestHandler : IRequestHandler<SampleVoidRequest>
    {
        public Task Handle(SampleVoidRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class SampleVoidRequestSuccessRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleVoidRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            SampleVoidRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
        }
    }

    public class SampleVoidRequestFailureRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleVoidRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            SampleVoidRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysFailureRequirement());
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public static async Task SampleStreamRequest_RunPipeline_SuccessRequirementProducesResult(ServiceLifetime serviceLifetime)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = serviceLifetime;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleStreamRequestSuccessRequirementBuilder, SampleStreamRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleStreamRequest();

        // Act & Assert
        var result = await mediator.CreateStream(requestObj).ToListAsync(); // Should not throw auth error
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public static async Task SampleStreamRequest_RunPipeline_FailingRequirementProducesUnauthException(ServiceLifetime serviceLifetime)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = serviceLifetime;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleStreamRequestFailureRequirementBuilder, SampleStreamRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleStreamRequest();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(async () => await mediator.CreateStream(requestObj).ToListAsync());
    }

    public record SampleStreamRequest() : IStreamRequest<SampleStreamResponse>;

    public record SampleStreamResponse();

    public class SampleStreamRequestHandler : IStreamRequestHandler<SampleStreamRequest, SampleStreamResponse>
    {
        public async IAsyncEnumerable<SampleStreamResponse> Handle(SampleStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return new SampleStreamResponse();
            yield return new SampleStreamResponse();
        }
    }

    public class SampleStreamRequestSuccessRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleStreamRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            SampleStreamRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
        }
    }

    public class SampleStreamRequestFailureRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleStreamRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            SampleStreamRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysFailureRequirement());
        }
    }
}
