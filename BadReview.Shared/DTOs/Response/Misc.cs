namespace BadReview.Shared.DTOs.Response;

public record PagedResult<T>(List<T> Data, int TotalCount, int Page, int PageSize)
{
    public int TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
}
