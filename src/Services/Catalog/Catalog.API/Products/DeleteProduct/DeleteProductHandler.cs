﻿using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;
using System.Windows.Input;

namespace Catalog.API.Products.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResult>;

    public record DeleteProductResult(bool IsSuccess);

    public class DeleteProductHandler (IDocumentSession session, ILogger<DeleteProductHandler> logger)
        : ICommandHandler<DeleteProductCommand, DeleteProductResult>
    {
        public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("DeleteProductHandler.Handle => ");

            session.Delete<Product>(command.Id);

            await session.SaveChangesAsync(cancellationToken);

            return new DeleteProductResult(true);


        }
    }
}
