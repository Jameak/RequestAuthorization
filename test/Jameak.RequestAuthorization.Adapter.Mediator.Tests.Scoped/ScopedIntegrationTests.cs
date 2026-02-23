using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.Mediator.Tests.Scoped;

public class ScopedIntegrationTests
{
    [Fact]
    public async Task SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        await BaseMediatorIntegrationTest.SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        await BaseMediatorIntegrationTest.SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        await BaseMediatorIntegrationTest.SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        await BaseMediatorIntegrationTest.SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        await BaseMediatorIntegrationTest.SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        await BaseMediatorIntegrationTest.SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Scoped);
    }
}
