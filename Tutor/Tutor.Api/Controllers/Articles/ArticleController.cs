using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor.Application.Article;
using Tutor.Domain.Articles;

namespace Tutor.Controllers.Articles;

[ApiController]
[Route("api/articles")]
public sealed class ArticleController(IArticleService service): TutorControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ArticleDetailDto>> GetAllTitles(
        [FromQuery] int pageNumber, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var listTitles = await service.GetAllTitels(pageNumber, pageSize, cancellationToken);
        return Reply(StatusCodes.Status200OK, "", null, listTitles);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var article = await service.GetByIdAsync(id, cancellationToken);
        return Reply(StatusCodes.Status200OK, "", article);
    }
    
    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<ArticleDetailDto>> Create(
        [FromBody] CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var authorId = GetIdAuthorizedUser();
        
        if (authorId == null)
        {
            return Unauthorized("User ID not found in token"); 
        }

        var newArticle = await service.CreateArticle(request.Title, 
                                                                request.Content, authorId, cancellationToken);
        return Reply(StatusCodes.Status201Created, "created", newArticle);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Tutor")]
    public async Task<ActionResult<ArticleDetailDto>> Update(
        Guid id, [FromBody] UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, request.Content, cancellationToken);

        return Reply(StatusCodes.Status200OK, "updated");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Reply(StatusCodes.Status204NoContent, "deleted");
    }
    
    private ObjectResult Reply(int status, string message, ArticleDetailDto? articleDetail = null, ListTitlesResponse? listTitles = null) =>
        StatusCode(status, new ArticleResponse(message, status, articleDetail,  listTitles));
}