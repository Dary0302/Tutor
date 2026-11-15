namespace Tutor.Domain.Articles;

public record TitleResponse(
    string Title,
    Guid Id,
    Guid AuthorId);

public record ListTitlesResponse( 
    List<TitleResponse> Requests,
    int TotalCount,
    int PageNumber,
    int PageSize);