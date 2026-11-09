using Tutor.Application.Article;
using Tutor.Domain.Articles;

namespace Tutor.Controllers.Articles;

public record GetAllTitlesRequest( 
    int PageNumber,
    int PageSize);

public record CreateArticleRequest(
    string Title,
    string Content);
    
public record UpdateArticleRequest(
    string Content);
    
public sealed record ArticleResponse(string Message, int Status, ArticleDetailDto? Data, ListTitlesResponse? ListTitles);