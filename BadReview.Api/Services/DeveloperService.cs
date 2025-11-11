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

public class DeveloperService : IDeveloperService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public DeveloperService(IIGDBService igdb, BadReviewContext db)
    {
        _igdb = igdb;
        _db = db;
    }


    public async Task<PagedResult<BasicDeveloperDto>> GetDevelopersAsync(IgdbRequest query, PaginationRequest pag)
    {
        var igdbDevs = await _igdb.GetDevelopersAsync(query, pag);
        
        List<BasicDeveloperDto> devList = igdbDevs.Data.Select(dev => CreateDeveloperDto(dev)).ToList();

        var developersPage = new PagedResult<BasicDeveloperDto>(devList, igdbDevs.TotalCount, igdbDevs.Page, igdbDevs.PageSize);

        return developersPage;
    }

    public async Task<DetailDeveloperDto?> GetDeveloperByIdAsync(int id, bool cache)
    {
        Developer? devDB = await _db.Developers.Where(dev => dev.Id == id).FirstOrDefaultAsync();

        if (devDB is not null) Console.WriteLine($"Fetching developer: {devDB.Name}, from DB");
        if (devDB is not null) return CreateDeveloperDto(devDB);

        var query = new IgdbRequest { Filters = $"id = {id} & developed != null" };
        //query.SetDefaults();

        PagedResult<DetailCompanyIgdbDto> response =
            await _igdb.GetAsync<DetailCompanyIgdbDto>(query, new PaginationRequest(), IGDBCONSTANTS.URIS.DEVELOPERS);

        DetailCompanyIgdbDto? devIGDB = response.Data.FirstOrDefault();

        if (devIGDB is null) return null;

        //mapear a Developer y persistir (si cache == true)
        if (cache)
        {
            var newDev = CreateDeveloperEntity(devIGDB);
            _db.Developers.Add(newDev);

            if (!await _db.SafeSaveChangesAsync())
                throw new WritingToDBException("Exception while saving new developer from IGDB to DB.");

            Console.WriteLine($"Cached IGDB developer: {devIGDB.Name} into the database");
        }

        return CreateDeveloperDto(devIGDB);
    }
}