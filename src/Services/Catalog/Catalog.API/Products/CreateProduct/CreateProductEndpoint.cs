using Carter;
using FluentValidation;
using Mapster;
using MediatR;

namespace Catalog.API.Products.CreateProduct
{
    //public class CreateProductCommandValidator : AbstractValidator<CreateProductRequest>
    //{
    //    public CreateProductCommandValidator()
    //    {
    //        RuleFor(x => x.Name).NotEmpty().WithMessage("Name should not be empty");
    //        RuleFor(x => x.Description).MinimumLength(10).WithMessage("Description Should have minimum 10 characters long");
    //        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price should be greater than zero");
    //    }
    //}

    public record CreateProductRequest(string Name, List<string> Category, string Description, string ImageFile, decimal Price);
    public record CreateProductResponse(Guid Id);
    public class CreateProductEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {//params in this delegate are both taken from request body and injected at the same time
            app.MapPost("/products", async (CreateProductRequest request, ISender sender/*, IValidator<CreateProductRequest> createProductValidator*/) =>
            {
                //old way -> new way is with custom middleware either MediatR beheviour with FLuentValidations
                //var validation = await createProductValidator.ValidateAsync(request);

                //if (!validation.IsValid)
                //    return Results.BadRequest(validation.Errors.Select(x => x.ErrorMessage).ToList());

                var command = request.Adapt<CreateProductCommand>();

                var result = await sender.Send(command); //Send triggers the pipeline behaviour with validations
                 
                var response = result.Adapt<CreateProductResponse>();

                return Results.Created($"/products/{response.Id}", response);

            })
            .WithDescription("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest).WithDescription("Bad Request")  //in case of fail 
            .WithSummary("CreateProduct")
            .WithDescription("Create Product");


            //app.MapPost("/products/{productId}", async (long productId, ISender sender) =>
            //{
            //    return Results.NotFound();

            //});
        }
    }
}
