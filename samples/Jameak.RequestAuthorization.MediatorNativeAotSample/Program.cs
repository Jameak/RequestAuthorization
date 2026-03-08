using Jameak.RequestAuthorization.Adapter.Mediator;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.MediatorNativeAotSample;
using Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;
using Jameak.RequestAuthorization.MediatorNativeAotSample.Requirements;
using Jameak.RequestAuthorization.MediatorNativeAotSample.Services;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

builder.Services.AddSingleton<IAuthService, FakeAuthService>();
builder.Services.AddSingleton<IDocumentService, FakeDocumentService>();
builder.Services.AddSingleton<IUserAccessor, FakeUserAccessor>();
builder.Services.AddRequestAuthorizationCore()
    .AddRequirementHandlerType<HasReadAccessToDocumentRequirementHandler, HasReadAccessToDocument>()
    .AddRequirementBuilderType<GetDocumentRequestRequirementBuilder, GetDocumentRequest>();

builder.Services.AddMediator(options =>
{
    // Instead of using the services.AddRequestAuthorizationCore.AddMediatorPipelineAdapter() registration-method,
    // you should register the behaviors like this if you're using NativeAOT.
    options.PipelineBehaviors = [typeof(RequestAuthorizationPipelineBehavior<,>)];
    options.StreamPipelineBehaviors = [typeof(RequestAuthorizationStreamPipelineBehavior<,>)];
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

var app = builder.Build();

var documentsApi = app.MapGroup("/documents");

documentsApi.MapGet("/", async Task<Results<Ok<GetDocumentResponse>, UnauthorizedHttpResult>> (
    [AsParameters] GetDocumentRequest documentRequest,
    [FromServices] IMediator mediator,
    [FromServices] ILogger<Program> logger,
    CancellationToken token) =>
{
    try
    {
        return TypedResults.Ok(await mediator.Send(documentRequest, token));
    }
    catch (UnauthorizedException ex)
    {
        logger.LogInformation(ex, "Unauthorized request");
        return TypedResults.Unauthorized();
    }
});

await app.RunAsync();
