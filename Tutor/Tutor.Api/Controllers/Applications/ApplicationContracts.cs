using Tutor.Domain.Applications;

namespace Tutor.Controllers.Applications;

public record PlaceApproveRequest( 
    Guid Id,
    bool StatusApprove,
    string? ReasonForRefusal);
    
public sealed record AppResponse(string Message, int Status, AppDetailDto? appDetail, List<AppListDto>? listApp, List<AppListForTutorDto>? listForTutorApp);