using BadReview.Api.DTOs.External;
using BadReview.Api.Services;

namespace BadReview.Api.Utils;

public enum ReviewState { PLAYED, PLAYING, WISHLIST, NONE };
public enum SortOrder { ASC, DSC };
public enum IGDBFieldsEnum { BASE, DETAIL };

public static class CONSTANTS
{
    public const int DEF_PAGE = 0;
    public const int DEF_PAGESIZE = 10;
    public const SortOrder DEF_SORTORDER = SortOrder.ASC;
    public const IGDBFieldsEnum DEF_DETAIL = IGDBFieldsEnum.BASE;
}

public static class ExtensionMethods
{
    public static string SortOrderStr(this SortOrder? order)
    {
        return order switch
        {
            SortOrder.ASC => "asc",
            SortOrder.DSC => "desc",
            _ => "",
        };
    }
}