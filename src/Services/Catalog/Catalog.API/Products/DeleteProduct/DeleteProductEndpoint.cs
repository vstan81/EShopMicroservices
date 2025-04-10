using Carter;
using Catalog.API.Products.CreateProduct;
using JasperFx.CodeGeneration.Model;
using Mapster;
using MediatR;

namespace Catalog.API.Products.DeleteProduct
{
    public class DeleteProductEndpoint : ICarterModule
    {
        //public record DeleteProductRequest(Guid Id);
        public record DeleteProductResult(bool IsSuccess);

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/products/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id));

                var response = result.Adapt<DeleteProductResult>();

                return Results.Ok(response);    

            })
            .WithName("DeleteProductResult")
            .Produces<CreateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)  //in case of fail 
            .WithSummary("DeleteProductResult")
            .WithDescription("Delete ProductResult");
        }
    }
}
