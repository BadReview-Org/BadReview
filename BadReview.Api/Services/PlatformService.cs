using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Api.Models;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;

namespace BadReview.Api.Services;

public class PlatformService : IPlatformService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public PlatformService(IIGDBService igdb, BadReviewContext db)
    {
        _igdb = igdb;
        _db = db;
    }


    public async Task<PagedResult<PlatformDto>> GetPlatformsAsync(IgdbRequest query, PaginationRequest pag)
    {
        var igdbPlatforms = await _igdb.GetPlatformsAsync(query, pag);
        
        List<PlatformDto> platformList = igdbPlatforms.Data.Select(p => CreatePlatformDto(p)).ToList();

        var platformPage = new PagedResult<PlatformDto>(platformList, igdbPlatforms.TotalCount, igdbPlatforms.Page, igdbPlatforms.PageSize);

        return platformPage;
    }

    public async Task<PlatformDto?> GetPlatformByIdAsync(int id, bool cache)
    {
        Platform? platformDB = await _db.Platforms.Where(p => p.Id == id).FirstOrDefaultAsync();

        if (platformDB is not null) Console.WriteLine($"Fetching platform: {platformDB.Name}, from DB");
        if (platformDB is not null) return CreatePlatformDto(platformDB);

        var query = new IgdbRequest { Filters = $"id = {id}" };
        //query.SetDefaults();

        PagedResult<PlatformIgdbDto> response =
            await _igdb.GetAsync<PlatformIgdbDto>(query, new PaginationRequest(), IGDBCONSTANTS.URIS.PLATFORMS);

        PlatformIgdbDto? platformIGDB = response.Data.FirstOrDefault();

        if (platformIGDB is null) return null;

        //mapear a Platform y persistir (si cache == true)
        if (cache)
        {
            var newPlatform = CreatePlatformEntity(platformIGDB);
            _db.Platforms.Add(newPlatform);

            if (await _db.SafeSaveChangesAsync())
                throw new WritingToDBException("Exception while saving new platform from IGDB to DB.");

            Console.WriteLine($"Cached IGDB platform: {platformIGDB.Name} into the database");
        }

        return CreatePlatformDto(platformIGDB);
    }
}