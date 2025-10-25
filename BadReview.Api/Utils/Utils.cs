namespace BadReview.Api.Utils;

public enum ReviewState { PLAYED, PLAYING, WISHLIST, NONE };
public enum SortOrder { ASC, DSC };

public static class CONSTANTS
{
    public const int DEF_PAGE = 1;
    public const int DEF_PAGESIZE = 10;
    public const SortOrder DEF_SORTORDER = SortOrder.ASC;
}