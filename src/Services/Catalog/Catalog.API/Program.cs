using BuildingBlocks.Behaviours;
using Carter;
using FluentValidation;
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
});

//custom middleware validator

//FluentValidator
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMarten(opt =>
{
    opt.Connection(builder.Configuration.GetConnectionString("Database"));

}).UseLightweightSessions();


var app = builder.Build();
//MIDDLEWARE

//custom middleware validator nu a mers
//app.UseMiddleware<ValidationCustomMiddleware>();

//Build HTTP request pipeline

app.MapCarter();

app.UseExceptionHandler(appBuilder => {

    appBuilder.Run(async (context) => {

        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if(exception == null)
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Title = exception.Message,
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.StackTrace
        };

        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger.LogError(exception, exception.Message);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    });


});


app.Run();
