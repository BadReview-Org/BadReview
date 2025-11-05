using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record IgdbRequest
{
    //filters=company:ubisoft;name:wolverine
    public string? Filters { get; set; }
    public string? OrderBy { get; set; }
    public SortOrder? Order { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }

    // Must call SetDefaults after initialization
    public void SetDefaults()
    {
        this.Order ??= IGDBCONSTANTS.DEF_SORTORDER;
        this.Page ??= IGDBCONSTANTS.DEF_PAGE;
        this.PageSize ??= IGDBCONSTANTS.DEF_PAGESIZE;
    }
}