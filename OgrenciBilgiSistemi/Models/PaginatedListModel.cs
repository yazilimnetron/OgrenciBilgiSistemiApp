using Microsoft.EntityFrameworkCore;

public class PaginatedListModel<T> : List<T>
{
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    // YENİ EKLEDİĞİMİZ:
    public int PageSize { get; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public bool IsFirstPage => PageIndex == 1;
    public bool IsLastPage => PageIndex == TotalPages;

    private PaginatedListModel(List<T> items, int count, int pageIndex, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        TotalPages = count > 0
            ? (int)Math.Ceiling(count / (double)pageSize)
            : 1;

        PageIndex = pageIndex; // pageIndex'i CreateAsync içinde zaten clamp ediyoruz

        AddRange(items);
    }

    public static async Task<PaginatedListModel<T>> CreateAsync(
        IQueryable<T> source,
        int pageIndex,
        int pageSize,
        CancellationToken ct)
    {
        // Güvenlik: minimum 1
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Max(1, pageSize);

        var count = await source.CountAsync(ct);

        // Toplam sayfa hesapla
        var totalPages = count > 0
            ? (int)Math.Ceiling(count / (double)pageSize)
            : 1;

        // İstenilen sayfa, toplam sayfadan büyükse son sayfaya çek
        if (pageIndex > totalPages)
            pageIndex = totalPages;

        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedListModel<T>(items, count, pageIndex, pageSize);
    }
}