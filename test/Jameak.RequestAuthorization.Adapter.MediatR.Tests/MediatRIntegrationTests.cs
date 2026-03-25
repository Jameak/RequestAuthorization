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

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await mediator.Send(requestObj);
            Assert.NotNull(result);
        });

        Assert.Null(exception);
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
        private readonly IRequestAuthorizationResultAccessor _authorizationResultAccessor;

        public SampleRequestHandler(IRequestAuthorizationResultAccessor authorizationResultAccessor)
        {
            _authorizationResultAccessor = authorizationResultAccessor;
        }

        public Task<SampleResponse> Handle(SampleRequest request, CancellationToken cancellationToken)
        {
            Assert.NotNull(_authorizationResultAccessor.AuthorizationResult);
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

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            await mediator.Send(requestObj);
        });

        Assert.Null(exception);
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
        private readonly IRequestAuthorizationResultAccessor _authorizationResultAccessor;

        public SampleVoidRequestHandler(IRequestAuthorizationResultAccessor authorizationResultAccessor)
        {
            _authorizationResultAccessor = authorizationResultAccessor;
        }

        public Task Handle(SampleVoidRequest request, CancellationToken cancellationToken)
        {
            Assert.NotNull(_authorizationResultAccessor.AuthorizationResult);
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
        private readonly IRequestAuthorizationResultAccessor _authorizationResultAccessor;

        public SampleStreamRequestHandler(IRequestAuthorizationResultAccessor authorizationResultAccessor)
        {
            _authorizationResultAccessor = authorizationResultAccessor;
        }

        public async IAsyncEnumerable<SampleStreamResponse> Handle(SampleStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Assert.NotNull(_authorizationResultAccessor.AuthorizationResult);
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


    [Fact]
    public static async Task BuilderWithDerivedRequests()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = ServiceLifetime.Scoped;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Scoped)
            //.AddRequirementBuilderType<GetDataRequestRequirementBuilder<GetDataResponse>, GetDataRequest>()
            .AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(CustomerRequestRequirementBuilder<>), typeof(ICustomerRequest<>), typeof(MediatRIntegrationTests).Assembly)
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new GetCustomerDataRequest();

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            await mediator.Send(requestObj);
        });

        Assert.Null(exception);
    }

    public interface ICustomerRequest<T> : IRequest<T>
    {
        public Guid CustomerId { get; }
    }

    public class GetCustomerDataRequest : ICustomerRequest<GetCustomerDataResponse>
    {
        public Guid CustomerId { get; }
    }

    public class GetCustomerDataResponse;

    public class GetDataRequestHandler : IRequestHandler<GetCustomerDataRequest, GetCustomerDataResponse>
    {
        public Task<GetCustomerDataResponse> Handle(GetCustomerDataRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new GetCustomerDataResponse());
        }
    }

    public class CustomerRequestRequirementBuilder<T> : IRequestAuthorizationRequirementBuilder<ICustomerRequest<T>>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            ICustomerRequest<T> request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
        }
    }

    [Fact]
    public static async Task BuilderWithDerivedVoidRequests()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MediatRIntegrationTests>();
            cfg.Lifetime = ServiceLifetime.Scoped;
        });
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Scoped)
            //.AddRequirementBuilderType<GetDataVoidRequestRequirementBuilder, GetDataRequest>()
            .AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(CustomerVoidRequestRequirementBuilder), typeof(ICustomerVoidRequest), typeof(MediatRIntegrationTests).Assembly)
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddMediatRPipelineAdapter();
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var requestObj = new GetCustomerDataVoidRequest();

        // Act & Assert. Should not throw auth exception.
        var exception = await Record.ExceptionAsync(async () =>
        {
            await mediator.Send(requestObj);
        });

        Assert.Null(exception);
    }

    public interface ICustomerVoidRequest : IRequest
    {
        public Guid CustomerId { get; }
    }

    public class GetCustomerDataVoidRequest : ICustomerVoidRequest
    {
        public Guid CustomerId { get; }
    }


    public class GetCustomerDataVoidRequestHandler : IRequestHandler<GetCustomerDataVoidRequest>
    {
        public Task Handle(GetCustomerDataVoidRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class CustomerVoidRequestRequirementBuilder : IRequestAuthorizationRequirementBuilder<ICustomerVoidRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            ICustomerVoidRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
        }
    }
}
