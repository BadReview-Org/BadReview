using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record ApiRequest
{
    public string? URI { get; set; }
    public string? Filters { get; set; }
    public string? OrderBy { get; set; }
    public SortOrder? Order { get; set; }

    public int? Page { get; set; }

    public int? PageSize { get; set; }

    // Must call SetDefaults after initialization
    public void SetDefaults()
    {
        this.Order ??= APIUTILS.DEF_SORTORDER;
        this.Page ??= APICONSTANTS.DEF_PAGE;
        this.PageSize ??= APICONSTANTS.DEF_PAGESIZE;
    }
}