﻿using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;
using Marten.Linq.QueryHandlers;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Linq.Expressions;

namespace Catalog.API.Products.GetProductById
{
    public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

    public record GetProductByIdResult(Product Product);

    public class GetProductByIdHandler(IDocumentSession session, ILogger<GetProductByIdHandler> logger) 
        : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
    {
        public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductByIdResult.Handle -> Started porcessing query {@Query}", query);

            var result = await session.LoadAsync<Product>(query.Id, cancellationToken);

            if (result is null)
                throw new ProductNotFoundException();

            return new GetProductByIdResult(result);

        }
    }
}
