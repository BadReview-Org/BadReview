using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record PaginationRequest
{
    // need nullable fields for .net parameters, in case the client does not provide these params
    public int? Page { get; set; } = CONSTANTS.DEF_PAGE;
    public int? PageSize { get; set; } = CONSTANTS.DEF_PAGESIZE;

    public PaginationRequest(int? page, int? pageSize)
    {
        Page = page ?? CONSTANTS.DEF_PAGE;
        PageSize = pageSize ?? CONSTANTS.DEF_PAGESIZE;
    }

    public PaginationRequest()
    {
        Page = CONSTANTS.DEF_PAGE;
        PageSize = CONSTANTS.DEF_PAGESIZE;
    }

    public void SetDefaults()
    {
        Page ??= CONSTANTS.DEF_PAGE;
        PageSize ??= CONSTANTS.DEF_PAGESIZE;
    }
}