using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.Mediator.Tests.Transient;

public class TransientIntegrationTests
{
    [Fact]
    public async Task SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        await BaseMediatorIntegrationTest.SampleRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Transient);
    }

    [Fact]
    public async Task SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        await BaseMediatorIntegrationTest.SampleRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Transient);
    }

    [Fact]
    public async Task SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        await BaseMediatorIntegrationTest.SampleVoidRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Transient);
    }

    [Fact]
    public async Task SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        await BaseMediatorIntegrationTest.SampleVoidRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Transient);
    }

    [Fact]
    public async Task SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        await BaseMediatorIntegrationTest.SampleStreamRequest_RunPipelineNotAot_SuccessRequirementProducesResult(serviceCollection, ServiceLifetime.Transient);
    }

    [Fact]
    public async Task SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        await BaseMediatorIntegrationTest.SampleStreamRequest_RunPipelineNotAot_FailingRequirementProducesUnauthException(serviceCollection, ServiceLifetime.Transient);
    }
}
