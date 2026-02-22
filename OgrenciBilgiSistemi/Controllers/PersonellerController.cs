using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Models;
using OgrenciBilgiSistemi.Models.Enums;
using OgrenciBilgiSistemi.Services.Interfaces;

namespace OgrenciBilgiSistemi.Controllers
{
    public class PersonellerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PersonellerController> _logger;
        private readonly IPersonelService _personelService;
        private readonly IBirimService _birimService;


        public PersonellerController(
            AppDbContext db,
            ILogger<PersonellerController> logger,
            IPersonelService personelService,
            IBirimService birimService)
        {
            _db = db;
            _logger = logger;
            _personelService = personelService;
            _birimService = birimService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            int pageNumber = 1,
            PersonelFiltre durum = PersonelFiltre.Aktif,
            CancellationToken ct = default)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["Durum"] = durum;

            var model = await _personelService.SearchPagedAsync(
                searchString: searchString,
                page: pageNumber,
                pageSize: 50,
                filtre: durum,
                ct: ct);

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Ekle(CancellationToken ct = default)
        {
            var model = new PersonelModel();
            ViewBag.Birimler = await GetBirimlerSelectListAsync(null, includeAllOption: false, ct);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(PersonelModel model, IFormFile? PersonelGorselFile, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Birimler = await GetBirimlerSelectListAsync(model.BirimId, includeAllOption: false, ct);
                return View(model);
            }

            try
            {
                await _personelService.AddAsync(model, PersonelGorselFile, ct);
                TempData["Mesaj"] = "Personel eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Personel eklenirken hata oluştu.");
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Birimler = await GetBirimlerSelectListAsync(model.BirimId, includeAllOption: false, ct);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Guncelle(int id, CancellationToken ct = default)
        {
            var p = await _db.Personeller.FindAsync(new object?[] { id }, ct);
            if (p is null) return NotFound();

            ViewBag.Birimler = await GetBirimlerSelectListAsync(p.BirimId, includeAllOption: false, ct);
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(PersonelModel model, IFormFile? PersonelGorselFile, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Birimler = await GetBirimlerSelectListAsync(model.BirimId, includeAllOption: false, ct);
                return View(model);
            }

            try
            {
                await _personelService.UpdateAsync(model, PersonelGorselFile, ct);
                TempData["Mesaj"] = "Personel güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Personel güncellenirken hata oluştu.");
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Birimler = await GetBirimlerSelectListAsync(model.BirimId, includeAllOption: false, ct);
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id, CancellationToken ct = default)
        {
            try
            {
                await _personelService.DeleteAsync(id, ct);
                TempData["Mesaj"] = "Personel pasif hale getirildi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Personel silinirken hata oluştu.");
                TempData["Mesaj"] = "Silme sırasında bir hata oluştu.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TopluPersonelGonder(int cihazId, CancellationToken ct)
        {
            var ok = await _personelService.CihazaGonderAsync(cihazId, sadeceAktifler: true, ct);
            TempData["Mesaj"] = ok
                ? "Tüm (aktif) personeller başarıyla cihaza gönderildi."
                : "Bazı personeller cihaza eklenemedi. Lütfen logları kontrol edin.";
            return RedirectToAction("Index", "Cihazlar");
        }


        private async Task<List<SelectListItem>> GetBirimlerSelectListAsync(
            int? selectedId,
            bool includeAllOption,
            CancellationToken ct)
        {
            var list = await _birimService.GetSelectListAsync(
                selectedId: selectedId,
                sinifMi: null,
                filtre: BirimFiltre.Aktif,
                ct: ct);

            return list;
        }
    }
}