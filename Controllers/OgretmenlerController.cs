//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Logging;
//using OgrenciBilgiSistemi.Models;
//using OgrenciBilgiSistemi.Services;
//using System.Threading;
//using Microsoft.AspNetCore.Http;


//namespace OgrenciBilgiSistemi.Controllers
//{
//    public class OgretmenlerController : Controller
//    {
//        private readonly IOgretmenService _ogretmenService;
//        private readonly ILogger<OgretmenlerController> _logger;

//        public OgretmenlerController(IOgretmenService ogretmenService, ILogger<OgretmenlerController> logger)
//        {
//            _ogretmenService = ogretmenService;
//            _logger = logger;
//        }

//        // /Ogretmenler?searchString=ali&page=1
//        public async Task<IActionResult> Index(string? searchString, int page = 1, CancellationToken ct = default)
//        {
//            ViewData["CurrentFilter"] = searchString;
//            var paged = await _ogretmenService.GetPagedAsync(searchString, page, 25, ct);
//            return View(paged);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Ekle(CancellationToken ct)
//        {
//            var model = new OgretmenModel
//            {
//                Birimler = await _ogretmenService.GetBirimlerSelectListAsync(ct)
//            };
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Ekle(OgretmenModel model, IFormFile? OgretmenGorselFile, CancellationToken ct)
//        {
//            if (!ModelState.IsValid)
//            {
//                model.Birimler = await _ogretmenService.GetBirimlerSelectListAsync(ct);
//                return View(model);
//            }

//            // (İsteğe bağlı) hafif normalizasyon
//            model.OgretmenKartNo = (model.OgretmenKartNo ?? "").Trim();

//            var (ok, err) = await _ogretmenService.CreateAsync(model, OgretmenGorselFile, ct);
//            if (!ok)
//            {
//                if (!string.IsNullOrWhiteSpace(err))
//                    ModelState.AddModelError(string.Empty, err);

//                model.Birimler = await _ogretmenService.GetBirimlerSelectListAsync(ct);
//                return View(model);
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpGet]
//        public async Task<IActionResult> Guncelle(int id, CancellationToken ct)
//        {
//            var ogretmen = await _ogretmenService.FindAsync(id, ct);
//            if (ogretmen == null) return NotFound();

//            ogretmen.Birimler = await _ogretmenService.GetBirimlerSelectListAsync(ct);
//            return View(ogretmen);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Guncelle(OgretmenModel model, IFormFile? OgretmenGorselFile, CancellationToken ct)
//        {
//            if (!ModelState.IsValid)
//            {
//                model.Birimler = await _ogretmenService.GetBirimlerSelectListAsync(ct);
//                return View(model);
//            }

//            // (İsteğe bağlı) hafif normalizasyon
//            model.OgretmenKartNo = (model.OgretmenKartNo ?? "").Trim();

//            var (ok, err) = await _ogretmenService.UpdateAsync(model, OgretmenGorselFile, ct);
//            if (!ok)
//            {
//                if (!string.IsNullOrWhiteSpace(err))
//                    ModelState.AddModelError(string.Empty, err);

//                model.Birimler = await _ogretmenService.GetBirimlerSelectListAsync(ct);
//                return View(model);
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Sil(int id, CancellationToken ct)
//        {
//            try
//            {
//                await _ogretmenService.SoftDeleteAsync(id, ct);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Öğretmen silinirken hata oluştu.");
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        public async Task<IActionResult> Detay(int id, CancellationToken ct)
//        {
//            var ogretmen = await _ogretmenService.GetDetailAsync(id, ct);
//            if (ogretmen == null) return NotFound();
//            return View(ogretmen);
//        }

//        public async Task<IActionResult> ExportToExcel(string? sortOrder, string? searchString, CancellationToken ct)
//        {
//            var bytes = await _ogretmenService.ExportToExcelAsync(sortOrder, searchString, ct);
//            return File(
//                bytes,
//                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                "OgretmenListesi.xlsx"
//            );
//        }
//    }
//}