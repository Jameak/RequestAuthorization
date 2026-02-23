using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.Mediator.Tests.Singleton;

public class SingletonIntegrationTests
{
    [Fact]
    public async Task SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Singleton);
        await BaseMediatorIntegrationTest.SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Singleton);
        await BaseMediatorIntegrationTest.SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Singleton);
        await BaseMediatorIntegrationTest.SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Singleton);
        await BaseMediatorIntegrationTest.SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Singleton);
        await BaseMediatorIntegrationTest.SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Singleton);
        await BaseMediatorIntegrationTest.SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Singleton);
    }
}
