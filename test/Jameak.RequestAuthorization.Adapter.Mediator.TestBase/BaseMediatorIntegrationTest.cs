using System.Runtime.CompilerServices;
using Jameak.RequestAuthorization.Adapter.Mediator.TestBase;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.Mediator.Tests;

public class BaseMediatorIntegrationTest
{
    [AssertionMethod]
    public static async Task SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult(ServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        // Arrange
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleRequestSuccessRequirementBuilder, SampleRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatorPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleRequest();

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await mediator.Send(requestObj);
            Assert.NotNull(result);
        });

        Assert.Null(exception);
    }

    [AssertionMethod]
    public static async Task SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(ServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        // Arrange
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleRequestFailureRequirementBuilder, SampleRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddMediatorPipelineAdapter();
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
        public async ValueTask<SampleResponse> Handle(
            SampleRequest request,
            CancellationToken cancellationToken)
        {
            return new SampleResponse();
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

    [AssertionMethod]
    public static async Task SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult(ServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        // Arrange
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleVoidRequestSuccessRequirementBuilder, SampleVoidRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatorPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleVoidRequest();

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await mediator.Send(requestObj);
            Assert.Equal(Unit.Value, result);
        });

        Assert.Null(exception);
    }

    [AssertionMethod]
    public static async Task SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(ServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        // Arrange
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleVoidRequestFailureRequirementBuilder, SampleVoidRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddMediatorPipelineAdapter();
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
        public ValueTask<Unit> Handle(SampleVoidRequest request, CancellationToken cancellationToken)
        {
            return Unit.ValueTask;
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

    [AssertionMethod]
    public static async Task SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult(ServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        // Arrange
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleStreamRequestSuccessRequirementBuilder, SampleStreamRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatorPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new SampleStreamRequest();

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await mediator.CreateStream(requestObj).ToListAsync(); // Should not throw auth error
            Assert.NotEmpty(result);
        });

        Assert.Null(exception);
    }

    [AssertionMethod]
    public static async Task SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(ServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        // Arrange
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: serviceLifetime)
            .AddRequirementBuilderType<SampleStreamRequestFailureRequirementBuilder, SampleStreamRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddMediatorPipelineAdapter();
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
