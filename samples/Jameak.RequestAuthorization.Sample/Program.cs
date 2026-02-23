using Jameak.RequestAuthorization.Adapter.Mediator;
using Jameak.RequestAuthorization.Adapter.MediatR;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Sample;
using Jameak.RequestAuthorization.Sample.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddSingleton<IAuthService, FakeAuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRequestAuthorizationCore()
    .AddRequirementHandlerTypesFromAssembly(typeof(Program).Assembly)
    .AddRequirementBuilderTypesFromAssembly(typeof(Program).Assembly)
    .AddRequirementBuilderType<SampleRequestAuthRequirementBuilder, SampleRequest>()
    .AddRequirementBuilderType<SampleVoidRequestAuthRequirementBuilder, SampleVoidRequest>()
    .AddRequirementBuilderType<GetDocumentRequestRequirementBuilder, GetDocumentRequest>()
    .AddRequirementHandlerType<HasReadAccessToDocumentRequirementHandler, HasReadAccessToDocument>()
    .AddMediatorPipelineAdapter()
    .AddMediatRPipelineAdapter();
builder.Services.AddMediator(options =>
{
    // Instead of using the AddRequestAuthorization DI-registration method, you should register the behaviors like this if you're using NativeAOT.
    // options.PipelineBehaviors = [typeof(RequestAuthorizationPipelineBehavior<,>)];
    // options.StreamPipelineBehaviors = [typeof(RequestAuthorizationStreamPipelineBehavior<,>)];
    //options.ServiceLifetime = ServiceLifetime.Singleton;
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();
app.MapControllers();

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    return string.Join("\n", endpointSources
        .SelectMany(source => source.Endpoints)
        .Select(endpoint => endpoint is RouteEndpoint route ? route.RoutePattern.RawText + " (" + route + ")" : endpoint.ToString()));
});

await app.RunAsync();
