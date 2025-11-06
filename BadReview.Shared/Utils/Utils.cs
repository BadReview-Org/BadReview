namespace BadReview.Shared.Utils;

public enum ReviewState { PLAYED, PLAYING, WISHLIST, NONE };
public enum SortOrder { ASC, DSC };
public enum IGDBGameDetail { BASE, DETAIL };

public static class IGDBCONSTANTS
{
    //public const int DEF_PAGE = 0;
    //public const int DEF_PAGESIZE = 10;
    public const SortOrder DEF_SORTORDER = SortOrder.DSC;
    public const IGDBGameDetail DEF_GAMEDETAIL = IGDBGameDetail.BASE;

    public static class URIS
    {
        public const string GAMES = "games";
        public const string GENRES = "genres";
        public const string PLATFORMS = "platforms";
        public const string DEVELOPERS = "involved_companies";
        public const string TRENDING = "popularity_primitives";
    }
}

public static class CONSTANTS
{
    public const int DEF_PAGE = 0;
    public const int DEF_PAGESIZE = 10;    
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