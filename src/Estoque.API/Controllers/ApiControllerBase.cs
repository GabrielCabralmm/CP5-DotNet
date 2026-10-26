using Estoque.API.Extensions;
using Estoque.API.Hateoas;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool IsAdmin => User.IsInRole(Perfis.Admin);

    protected ObjectResult FromError(Error error)
    {
        var problem = error.ToProblemDetails(HttpContext);
        return new ObjectResult(problem)
        {
            StatusCode = problem.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    protected Link LinkTo(string routeName, object? values, string rel, string method) =>
        new(Url.Link(routeName, values) ?? string.Empty, rel, method);

    protected PagedResource<T> ToPagedResource<T>(
        PageResult<T> page,
        Func<T, IReadOnlyList<Link>> itemLinks,
        string routeName,
        object? filtros = null)
    {
        var items = page.Items
            .Select(item => new Resource<T>(item, itemLinks(item)))
            .ToList();

        var links = new List<Link>
        {
            PageLink(routeName, page.PageNumber, page.PageSize, filtros, "self"),
            PageLink(routeName, 1, page.PageSize, filtros, "first")
        };

        if (page.HasPrevious)
            links.Add(PageLink(routeName, page.PageNumber - 1, page.PageSize, filtros, "previous"));

        if (page.HasNext)
            links.Add(PageLink(routeName, page.PageNumber + 1, page.PageSize, filtros, "next"));

        if (page.TotalPages > 0)
            links.Add(PageLink(routeName, page.TotalPages, page.PageSize, filtros, "last"));

        return new PagedResource<T>(
            items,
            page.PageNumber,
            page.PageSize,
            page.TotalItems,
            page.TotalPages,
            page.HasPrevious,
            page.HasNext,
            links);
    }

    private Link PageLink(string routeName, int pageNumber, int pageSize, object? filtros, string rel)
    {
        var values = new RouteValueDictionary(filtros)
        {
            ["pageNumber"] = pageNumber,
            ["pageSize"] = pageSize
        };

        return LinkTo(routeName, values, rel, "GET");
    }
}
