//using ClosedXML.Excel;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using OgrenciBilgiSistemi.Data;
//using OgrenciBilgiSistemi.Models;

//namespace OgrenciBilgiSistemi.Services
//{
//    public class OgretmenService : IOgretmenService
//    {
//        private readonly AppDbContext _context;
//        private readonly ILogger<OgretmenService> _logger;
//        private readonly IWebHostEnvironment _env;

//        public OgretmenService(AppDbContext context, ILogger<OgretmenService> logger, IWebHostEnvironment env)
//        {
//            _context = context;
//            _logger = logger;
//            _env = env;
//        }

//        public async Task<PaginatedListModel<OgretmenModel>> GetPagedAsync(
//            string? searchString, int page, int pageSize, CancellationToken ct = default)
//        {
//            page = Math.Max(1, page);
//            pageSize = Math.Max(1, pageSize);

//            var query = _context.Ogretmenler
//                                .Include(o => o.Birim)
//                                .AsNoTracking()
//                                .Where(o => o.OgretmenDurum);

//            if (!string.IsNullOrWhiteSpace(searchString))
//            {
//                var s = searchString.Trim();
//                var pattern = $"%{s}%";
//                query = query.Where(o =>
//                    EF.Functions.Like(o.OgretmenAdSoyad, pattern) ||
//                    EF.Functions.Like(o.OgretmenKartNo ?? "", pattern) // Kart No ile de ara
//                );
//            }

//            query = query.OrderBy(o => o.OgretmenAdSoyad)
//                         .ThenBy(o => o.OgretmenId);

//            return await PaginatedListModel<OgretmenModel>.CreateAsync(query, page, pageSize, ct);
//        }

//        public async Task<List<SelectListItem>> GetBirimlerSelectListAsync(CancellationToken ct = default)
//        {
//            return await _context.Birimler
//                .Where(b => b.BirimDurum == true)
//                .OrderBy(b => b.BirimAd)
//                .Select(b => new SelectListItem
//                {
//                    Value = b.BirimId.ToString(),
//                    Text = b.BirimAd
//                })
//                .ToListAsync(ct);
//        }

//        public async Task<(bool Ok, string? ErrorMessage)> CreateAsync(
//            OgretmenModel model, IFormFile? ogretmenGorselFile, CancellationToken ct = default)
//        {
//            try
//            {
//                // Kart no normalize
//                model.OgretmenKartNo = NormalizeKartNo(model.OgretmenKartNo);

//                // Görsel yükle
//                model.OgretmenGorsel = await UploadImageAsync(ogretmenGorselFile, null, ct);

//                _context.Ogretmenler.Add(model);
//                await _context.SaveChangesAsync(ct);
//                return (true, null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Öğretmen eklenirken hata oluştu.");
//                return (false, "Öğretmen eklenirken bir hata oluştu.");
//            }
//        }

//        public async Task<OgretmenModel?> FindAsync(int id, CancellationToken ct = default)
//        {
//            return await _context.Ogretmenler.FindAsync(new object?[] { id }, ct);
//        }

//        public async Task<(bool Ok, string? ErrorMessage)> UpdateAsync(
//            OgretmenModel model, IFormFile? ogretmenGorselFile, CancellationToken ct = default)
//        {
//            try
//            {
//                var ogretmen = await _context.Ogretmenler.FindAsync(new object?[] { model.OgretmenId }, ct);
//                if (ogretmen == null)
//                    return (false, "Öğretmen bulunamadı.");

//                ogretmen.OgretmenAdSoyad = model.OgretmenAdSoyad;
//                ogretmen.OgretmenDurum = model.OgretmenDurum;
//                ogretmen.BirimId = model.BirimId;

//                // Kart no normalize + kaydet
//                ogretmen.OgretmenKartNo = NormalizeKartNo(model.OgretmenKartNo);

//                if (ogretmenGorselFile is not null)
//                {
//                    ogretmen.OgretmenGorsel = await UploadImageAsync(ogretmenGorselFile, ogretmen.OgretmenGorsel, ct);
//                }

//                await _context.SaveChangesAsync(ct);
//                return (true, null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Öğretmen güncellenirken hata oluştu.");
//                return (false, "Öğretmen güncellenirken bir hata oluştu.");
//            }
//        }

//        public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
//        {
//            try
//            {
//                var ogretmen = await _context.Ogretmenler.FindAsync(new object?[] { id }, ct);
//                if (ogretmen == null) return;

//                ogretmen.OgretmenDurum = false;
//                await _context.SaveChangesAsync(ct);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Öğretmen silinirken hata oluştu.");
//            }
//        }

//        public async Task<OgretmenModel?> GetDetailAsync(int id, CancellationToken ct = default)
//        {
//            return await _context.Ogretmenler
//                .Include(p => p.Ogrenciler)
//                    .ThenInclude(o => o.Birim)
//                .FirstOrDefaultAsync(p => p.OgretmenId == id, ct);
//        }

//        public async Task<byte[]> ExportToExcelAsync(string? sortOrder, string? searchString, CancellationToken ct = default)
//        {
//            var query = _context.Ogretmenler
//                .Include(p => p.Birim)
//                .AsNoTracking()
//                .Where(p => p.OgretmenDurum == true);

//            if (!string.IsNullOrWhiteSpace(searchString))
//            {
//                var s = searchString.Trim();
//                var pattern = $"%{s}%";
//                query = query.Where(p =>
//                    EF.Functions.Like(p.OgretmenAdSoyad, pattern) ||
//                    EF.Functions.Like(p.OgretmenKartNo ?? "", pattern) // Kart No ile de filtrele
//                );
//            }

//            query = sortOrder == "AdSoyad_desc"
//                ? query.OrderByDescending(p => p.OgretmenAdSoyad).ThenByDescending(p => p.OgretmenId)
//                : query.OrderBy(p => p.OgretmenAdSoyad).ThenBy(p => p.OgretmenId);

//            var list = await query.ToListAsync(ct);

//            using var wb = new XLWorkbook();
//            var ws = wb.Worksheets.Add("Öğretmen Listesi");

//            ws.Cell(1, 1).Value = "ID";
//            ws.Cell(1, 2).Value = "Ad Soyad";
//            ws.Cell(1, 3).Value = "Kart No";
//            ws.Cell(1, 4).Value = "Birim";
//            ws.Cell(1, 5).Value = "Durum";

//            var row = 2;
//            foreach (var o in list)
//            {
//                ws.Cell(row, 1).Value = o.OgretmenId;
//                ws.Cell(row, 2).Value = o.OgretmenAdSoyad;
//                ws.Cell(row, 3).Value = o.OgretmenKartNo ?? "";
//                ws.Cell(row, 4).Value = o.Birim?.BirimAd ?? "Birim Yok";
//                ws.Cell(row, 5).Value = o.OgretmenDurum ? "Aktif" : "Pasif";
//                row++;
//            }

//            using var ms = new MemoryStream();
//            wb.SaveAs(ms);
//            return ms.ToArray();
//        }

//        // ---- Private helpers ----
//        private async Task<string?> UploadImageAsync(IFormFile? file, string? existingPath, CancellationToken ct)
//        {
//            if (file is null || file.Length == 0)
//                return existingPath;

//            try
//            {
//                // Klasör
//                var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
//                                               "uploads", "ogretmenler");
//                Directory.CreateDirectory(uploadsRoot);

//                // Eski dosyayı sil
//                if (!string.IsNullOrWhiteSpace(existingPath))
//                {
//                    var oldFull = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
//                                               existingPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
//                    if (File.Exists(oldFull))
//                        File.Delete(oldFull);
//                }

//                // Yeni dosya adı (GUID + orijinal uzantı)
//                var safeExt = Path.GetExtension(file.FileName);
//                var fileName = $"{Guid.NewGuid():N}{safeExt}";
//                var fullPath = Path.Combine(uploadsRoot, fileName);

//                using (var stream = new FileStream(fullPath, FileMode.Create))
//                    await file.CopyToAsync(stream, ct);

//                // Web yolu (ogretmenler klasörü)
//                var webPath = $"/uploads/{fileName}";
//                return webPath;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Görsel yükleme sırasında hata oluştu.");
//                return existingPath; // hata halinde eskisi korunur
//            }
//        }

//        private static string NormalizeKartNo(string? input)
//        {
//            if (string.IsNullOrWhiteSpace(input))
//                return string.Empty;

//            // Baştaki 0'ları at
//            var trimmed = input.TrimStart('0');

//            // Sadece harf ve rakamları bırak
//            var clean = new string(trimmed.Where(char.IsLetterOrDigit).ToArray());

//            return clean.Trim();
//        }
//    }
//}