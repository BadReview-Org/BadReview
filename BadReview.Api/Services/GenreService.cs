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

public class GenreService : IGenreService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public GenreService(IIGDBService igdb, BadReviewContext db)
    {
        _igdb = igdb;
        _db = db;
    }


    public async Task<PagedResult<GenreDto>> GetGenresAsync(IgdbRequest query, PaginationRequest pag)
    {
        var igdbGenres = await _igdb.GetGenresAsync(query, pag);
        
        List<GenreDto> genreList = igdbGenres.Data.Select(gen => CreateGenreDto(gen)).ToList();

        var genresPage = new PagedResult<GenreDto>(genreList, igdbGenres.TotalCount, igdbGenres.Page, igdbGenres.PageSize);

        return genresPage;
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int id, bool cache)
    {
        Genre? genreDB = await _db.Genres.Where(gen => gen.Id == id).FirstOrDefaultAsync();

        if (genreDB is not null) Console.WriteLine($"Fetching genre: {genreDB.Name}, from DB");
        if (genreDB is not null) return CreateGenreDto(genreDB);

        var query = new IgdbRequest { Filters = $"id = {id}" };
        //query.SetDefaults();

        PagedResult<GenreIgdbDto> response =
            await _igdb.GetAsync<GenreIgdbDto>(query, new PaginationRequest(), IGDBCONSTANTS.URIS.GENRES);

        GenreIgdbDto? genreIGDB = response.Data.FirstOrDefault();

        if (genreIGDB is null) return null;

        //mapear a Genre y persistir (si cache == true)
        if (cache)
        {
            var newGenre = CreateGenreEntity(genreIGDB);
            _db.Genres.Add(newGenre);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Cached IGDB genre: {genreIGDB.Name} into the database");
        }

        return CreateGenreDto(genreIGDB);
    }
}