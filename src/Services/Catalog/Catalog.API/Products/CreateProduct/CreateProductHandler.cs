using BuildingBlocks.CQRS;
using Catalog.API.Models;
using FluentValidation;
using Marten;


namespace Catalog.API.Products.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name should not be empty");
            RuleFor(x => x.Description).MinimumLength(10).WithMessage("Description Should have minimum 10 characters long");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price should be greater than zero");
        }
    }

    public record CreateProductCommand(string Name, List<string> Category, string Description,  string ImageFile,  decimal Price) 
        : ICommand<CreateProductResult>;
    public record CreateProductResult(Guid Id);

    internal class CreateProductCommandHandler(IDocumentSession martenSession) : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            //create product entity from command object
            //save to db
            //return CreateProductResult result

            var product = new Product
            {
                Name = command.Name,
                Category = command.Category,
                Description = command.Description,
                ImageFile = command.ImageFile,
                Price = command.Price
            };

            martenSession.Store(product);
            await martenSession.SaveChangesAsync(cancellationToken);

            return new CreateProductResult(product.Id);
        }
    }
}
