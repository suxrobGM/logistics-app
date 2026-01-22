using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("blog/posts")]
[Produces("application/json")]
public class BlogPostController(IMediator mediator) : ControllerBase
{
    #region Public Endpoints

    [HttpGet("published", Name = "GetPublishedBlogPosts")]
    [ProducesResponseType(typeof(PagedResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedBlogPosts([FromQuery] GetPublishedBlogPostsQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<BlogPostDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpGet("published/{slug}", Name = "GetPublishedBlogPostBySlug")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedBlogPostBySlug(string slug)
    {
        var result = await mediator.Send(new GetPublishedBlogPostBySlugQuery { Slug = slug });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Admin Endpoints

    [HttpGet("{id:guid}", Name = "GetBlogPostById")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.BlogPost.View)]
    public async Task<IActionResult> GetBlogPostById(Guid id)
    {
        var result = await mediator.Send(new GetBlogPostQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetBlogPosts")]
    [ProducesResponseType(typeof(PagedResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.BlogPost.View)]
    public async Task<IActionResult> GetBlogPosts([FromQuery] GetBlogPostsQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<BlogPostDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost(Name = "CreateBlogPost")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.BlogPost.Manage)]
    public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? CreatedAtRoute("GetBlogPostById", new { id = result.Value }, result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateBlogPost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.BlogPost.Manage)]
    public async Task<IActionResult> UpdateBlogPost(Guid id, [FromBody] UpdateBlogPostCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("{id:guid}/publish", Name = "PublishBlogPost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.BlogPost.Manage)]
    public async Task<IActionResult> PublishBlogPost(Guid id)
    {
        var result = await mediator.Send(new PublishBlogPostCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpPost("{id:guid}/unpublish", Name = "UnpublishBlogPost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.BlogPost.Manage)]
    public async Task<IActionResult> UnpublishBlogPost(Guid id)
    {
        var result = await mediator.Send(new UnpublishBlogPostCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteBlogPost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.BlogPost.Manage)]
    public async Task<IActionResult> DeleteBlogPost(Guid id)
    {
        var result = await mediator.Send(new DeleteBlogPostCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion
}
