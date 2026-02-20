//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using OgrenciBilgiSistemi.Models;

//namespace OgrenciBilgiSistemi.Services
//{
//    public interface IOgretmenService
//    {
//        Task<PaginatedListModel<OgretmenModel>> GetPagedAsync(
//            string? searchString, int page, int pageSize, CancellationToken ct = default);

//        Task<List<SelectListItem>> GetBirimlerSelectListAsync(CancellationToken ct = default);

//        Task<(bool Ok, string? ErrorMessage)> CreateAsync(
//            OgretmenModel model, IFormFile? ogretmenGorselFile, CancellationToken ct = default);

//        Task<OgretmenModel?> FindAsync(int id, CancellationToken ct = default);

//        Task<(bool Ok, string? ErrorMessage)> UpdateAsync(
//            OgretmenModel model, IFormFile? ogretmenGorselFile, CancellationToken ct = default);

//        Task SoftDeleteAsync(int id, CancellationToken ct = default);

//        Task<OgretmenModel?> GetDetailAsync(int id, CancellationToken ct = default);

//        Task<byte[]> ExportToExcelAsync(string? sortOrder, string? searchString, CancellationToken ct = default);
//    }
//}