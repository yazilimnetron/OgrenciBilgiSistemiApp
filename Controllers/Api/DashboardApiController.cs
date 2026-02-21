using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Dtos;
using OgrenciBilgiSistemi.Models.Enums;

namespace OgrenciBilgiSistemi.Controllers.Api
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashboardApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardApiController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> Stats(CancellationToken ct)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var bugunYemekhaneGiris = await _db.OgrenciDetaylar.AsNoTracking()
                .Where(x => x.IstasyonTipi == IstasyonTipi.Yemekhane
                         && x.OgrenciGecisTipi == "GİRİŞ"
                         && x.OgrenciGTarih != null
                         && x.OgrenciGTarih! >= today
                         && x.OgrenciGTarih! < tomorrow)
                .CountAsync(ct);

            var bugunAnakapiCikis = await _db.OgrenciDetaylar.AsNoTracking()
                .Where(x => x.IstasyonTipi == IstasyonTipi.AnaKapi
                         && x.OgrenciGecisTipi == "ÇIKIŞ"
                         && x.OgrenciCTarih != null
                         && x.OgrenciCTarih! >= today
                         && x.OgrenciCTarih! < tomorrow)
                .CountAsync(ct);

            var toplamOgrenci = await _db.Ogrenciler.AsNoTracking().CountAsync(ct);

            return Ok(new DashboardStatsDto
            {
                ToplamOgrenci = toplamOgrenci,
                BugunYemekhaneGiris = bugunYemekhaneGiris,
                BugunAnakapiCikis = bugunAnakapiCikis
            });
        }

        // GET /api/dashboard/seriler?yil=2025&ay=11
        [HttpGet("seriler")]
        public async Task<IActionResult> Seriler(
            [FromQuery] int? yil,
            [FromQuery] int? ay,
            CancellationToken ct)
        {
            var now = DateTime.Now;
            int y = yil ?? now.Year;
            int m = ay ?? now.Month;

            var start = new DateTime(y, m, 1);
            var end = start.AddMonths(1);
            int days = DateTime.DaysInMonth(y, m);

            var ymk = await _db.OgrenciDetaylar.AsNoTracking()
                .Where(x => x.IstasyonTipi == IstasyonTipi.Yemekhane
                         && x.OgrenciGecisTipi == "GİRİŞ"
                         && x.OgrenciGTarih != null
                         && x.OgrenciGTarih! >= start
                         && x.OgrenciGTarih! < end)
                .GroupBy(x => new { d = x.OgrenciGTarih!.Value.Date })
                .Select(g => new { Gun = g.Key.d, Adet = g.Count() })
                .ToListAsync(ct);

            var ank = await _db.OgrenciDetaylar.AsNoTracking()
                .Where(x => x.IstasyonTipi == IstasyonTipi.AnaKapi
                         && x.OgrenciGecisTipi == "ÇIKIŞ"
                         && x.OgrenciCTarih != null
                         && x.OgrenciCTarih! >= start
                         && x.OgrenciCTarih! < end)
                .GroupBy(x => new { d = x.OgrenciCTarih!.Value.Date })
                .Select(g => new { Gun = g.Key.d, Adet = g.Count() })
                .ToListAsync(ct);

            var ymkMap = ymk.ToDictionary(x => x.Gun, x => x.Adet);
            var ankMap = ank.ToDictionary(x => x.Gun, x => x.Adet);

            var dto = new DashboardSeriesDto { Yil = y, Ay = m };

            for (int d = 1; d <= days; d++)
            {
                var day = new DateTime(y, m, d);
                dto.GunEtiketleri.Add(d.ToString());
                dto.YemekhaneGiris.Add(ymkMap.TryGetValue(day, out var yg) ? yg : 0);
                dto.AnakapiCikis.Add(ankMap.TryGetValue(day, out var ac) ? ac : 0);
            }

            return Ok(dto);
        }
    }
}
