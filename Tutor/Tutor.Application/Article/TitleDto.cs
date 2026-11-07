namespace Tutor.Application.Article;

public record TitleResponse(
    string Title,
    Guid Id,
    Guid AuthorId);

public record ListTitlesResponse( 
    List<TitleResponse> Requests,
    int TotalCount,
    int PageNumber,
    int PageSize);