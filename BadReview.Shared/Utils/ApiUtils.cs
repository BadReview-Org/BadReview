namespace BadReview.Shared.Utils;
public static class APIUTILS
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
        public const string DEVELOPERS = "developers";
        public const string DEVELOPER = "developer";
        public const string TRENDING = "games/trending";
        public const string PUBLICGAME = "games/public";
        public const string PRIVATEGAME = "games/private";
        public const string REVIEWS = "reviews";
        public const string USERS = "users";
        public const string PROFILE = "profile?pagesize=6";
        public const string UPDATEPROFILE = "profile";
    }
}

public static class APICONSTANTS
{
    public const int DEF_PAGE = 0;
    public const int DEF_PAGESIZE = 12;    
}