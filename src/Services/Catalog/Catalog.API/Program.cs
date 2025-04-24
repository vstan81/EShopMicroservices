using BuildingBlocks.Behaviours;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using Catalog.API.Data;
using FluentValidation;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

//ADd Services to container
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    //inregistrare pipeline behaviour cu fluentvalidations -- partea misto e ca se activeaza doar la mediator.Send()
    //nu la orice request ca la custommiddleware
    config.AddOpenBehavior(typeof(ValidationBehaviours<,>));
    config.AddOpenBehavior(typeof(LoggingBehaviour<,>));
});

//custom middleware validator

//FluentValidator
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMarten(opt =>
{
    opt.Connection(builder.Configuration.GetConnectionString("Database"));

}).UseLightweightSessions();

//seed 
if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

//global IExceptionHandler registered as a singleton service
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

//healthcheck for posgresql database
builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("Database"));

var app = builder.Build();
//MIDDLEWARE

//custom middleware validator nu a mers
//app.UseMiddleware<ValidationCustomMiddleware>();

//Build HTTP request pipeline

app.MapCarter();

//am reununtat la asta si am facut cu IExceptionHandler vezi in BuildingBlocks
//app.UseExceptionHandler(appBuilder => {

//    appBuilder.Run(async (context) => {

//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

//        if(exception == null)
//        {
//            return;
//        }

//        var problemDetails = new ProblemDetails
//        {
//            Title = exception.Message,
//            Status = StatusCodes.Status500InternalServerError,
//            Detail = exception.StackTrace
//        };

//        var logger = context.RequestServices.GetService<ILogger<Program>>();
//        logger.LogError(exception, exception.Message);

//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//        context.Response.ContentType = "application/json";

//        await context.Response.WriteAsJsonAsync(problemDetails);
//    });


//});
//ca sa foloseasca custom exception Handelrele pe care le gaseste in pipeline
//l-am inregistrat pe asta CustomExceptionHandler
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
    
    );


app.Run();
