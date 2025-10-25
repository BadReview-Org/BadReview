using BadReview.Api.Utils;
using Microsoft.AspNetCore.Mvc;

namespace BadReview.Api.DTOs.Request;

public record SelectGamesRequest
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
        this.Order = this.Order ?? CONSTANTS.DEF_SORTORDER;
        this.Page = this.Page ?? CONSTANTS.DEF_PAGE;
        this.PageSize = this.PageSize ?? CONSTANTS.DEF_PAGESIZE;
    }
}