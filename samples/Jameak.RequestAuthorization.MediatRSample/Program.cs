using Jameak.RequestAuthorization.Adapter.MediatR;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.MediatRSample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddSingleton<IAuthService, FakeAuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRequestAuthorizationCore()
    .AddRequirementHandlerTypesFromAssembly(typeof(Program).Assembly)
    .AddRequirementBuilderTypesFromAssembly(typeof(Program).Assembly)
    .AddGlobalRequirementBuilderTypesFromAssembly(typeof(Program).Assembly)
    .AddMediatRPipelineAdapter();

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
