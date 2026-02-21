using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OgrenciBilgiSistemi.Controllers.Api
{
    [ApiController]
    [Route("api/ogrenciler")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OgrencilerApiController : ControllerBase
    {
        private readonly IOgrenciService _ogrenciService;

        public OgrencilerApiController(IOgrenciService ogrenciService)
        {
            _ogrenciService = ogrenciService;
        }

        // GET /api/ogrenciler?sayfa=1&aramaMetni=ali&birimId=2&pasiflerDahil=false
        [HttpGet]
        public async Task<IActionResult> Liste(
            [FromQuery] int sayfa = 1,
            [FromQuery] string? aramaMetni = null,
            [FromQuery] int? birimId = null,
            [FromQuery] bool pasiflerDahil = false,
            [FromQuery] int sayfaBoyutu = 20,
            CancellationToken ct = default)
        {
            var sonuc = await _ogrenciService.SearchPagedAsync(
                sortOrder: null,
                searchString: aramaMetni,
                pageNumber: sayfa,
                birimId: birimId,
                includePasif: pasiflerDahil,
                pageSize: sayfaBoyutu,
                ct: ct);

            var items = sonuc.Select(o => new
            {
                o.OgrenciId,
                o.OgrenciAdSoyad,
                o.OgrenciNo,
                o.OgrenciKartNo,
                o.OgrenciDurum,
                o.OgrenciCikisDurumu,
                BirimAdi = o.Birim?.BirimAd,
                OgretmenAdi = o.Personel?.PersonelAdSoyad,
                Fotograf = string.IsNullOrEmpty(o.OgrenciGorsel) ? null : o.OgrenciGorsel
            });

            return Ok(new
            {
                toplam = sonuc.TotalCount,
                sayfaSayisi = sonuc.TotalPages,
                mevcutSayfa = sonuc.PageIndex,
                items
            });
        }

        // GET /api/ogrenciler/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detay(int id, CancellationToken ct)
        {
            var ogrenci = await _ogrenciService.GetByIdAsync(id, includeVeli: true, ct: ct);

            if (ogrenci == null)
                return NotFound(new { hata = $"Öğrenci bulunamadı (id={id})." });

            return Ok(new
            {
                ogrenci.OgrenciId,
                ogrenci.OgrenciAdSoyad,
                ogrenci.OgrenciNo,
                ogrenci.OgrenciKartNo,
                ogrenci.OgrenciDurum,
                ogrenci.OgrenciCikisDurumu,
                BirimAdi = ogrenci.Birim?.BirimAd,
                OgretmenAdi = ogrenci.Personel?.PersonelAdSoyad,
                Fotograf = string.IsNullOrEmpty(ogrenci.OgrenciGorsel) ? null : ogrenci.OgrenciGorsel,
                Veli = ogrenci.OgrenciVeli == null ? null : new
                {
                    ogrenci.OgrenciVeli.OgrenciVeliId,
                    ogrenci.OgrenciVeli.VeliAdSoyad,
                    ogrenci.OgrenciVeli.VeliTelefon
                }
            });
        }
    }
}
