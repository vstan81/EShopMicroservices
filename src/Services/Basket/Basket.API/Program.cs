using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.Behaviours;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using static Discount.Grpc.DiscountProtoService;

var builder = WebApplication.CreateBuilder(args);

//Add Services to the container
builder.Services.AddCarter();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviours<,>));
    config.AddOpenBehavior(typeof(LoggingBehaviour<,>));
});

builder.Services.AddMarten(opt =>
{
    opt.Connection(builder.Configuration.GetConnectionString("Database")!);
    opt.Schema.For<ShoppingCart>().Identity(x => x.UserName); //set username as unique key

}).UseLightweightSessions();


builder.Services.AddScoped<IBasketRepository, BasketRepository>();
//scrutor library to decorate existing repository
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();


//IDistributedCache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

//Grpc Client

builder.Services.AddGrpcClient<DiscountProtoServiceClient>(options => {

    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);


});

builder.Services.AddHealthChecks()
      .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
      .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();


//request pipeline
app.MapCarter();

app.UseExceptionHandler(options => {});

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
