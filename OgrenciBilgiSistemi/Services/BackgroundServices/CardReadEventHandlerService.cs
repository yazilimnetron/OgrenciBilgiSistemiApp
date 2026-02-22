using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Models.Enums;
using OgrenciBilgiSistemi.Services.Interfaces;

public class CardReadEventHandlerService : IHostedService
{
    private readonly IZKTecoService _zkTecoService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CardReadEventHandlerService> _logger;

    public CardReadEventHandlerService(
        IZKTecoService zkTecoService,
        IServiceScopeFactory scopeFactory,
        ILogger<CardReadEventHandlerService> logger)
    {
        _zkTecoService = zkTecoService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // (opsiyonel) bağlantı kur
        try { await _zkTecoService.ConnectAsync(); } catch { /* loglanabilir */ }

        _zkTecoService.OnCardReadAsync -= CardReadHandlerAsync; // güvenlik
        _zkTecoService.OnCardReadAsync += CardReadHandlerAsync;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _zkTecoService.OnCardReadAsync -= CardReadHandlerAsync;
        return Task.CompletedTask;
    }

    private static string Normalize(string s) => (s ?? string.Empty).Trim().TrimStart('0');

    private async Task CardReadHandlerAsync(string kartNo)
    {
        var now = DateTime.Now;
        var norm = Normalize(kartNo);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var svc = scope.ServiceProvider.GetRequiredService<IGecisService>();

            // 1) Öğrenciyi bul
            var ogr = await db.Ogrenciler.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OgrenciKartNo == norm && o.OgrenciDurum == true);
            if (ogr is null)
            {
                _logger.LogWarning("ZKTeco kart tanımsız: {Kart}", norm);
                return;
            }

            // 2) Bu worker’ın kullanacağı ZKTeco cihazını seç
            // (Basit: ilk aktif ZKTeco. İstersen appsettings ile spesifik CihazKodu/Id bağlayabilirsin.)
            var cihaz = await db.Cihazlar.AsNoTracking()
                .Where(c => c.DonanimTipi == DonanimTipi.ZKTeco && c.Aktif)
                .OrderBy(c => c.CihazId)
                .FirstOrDefaultAsync();

            if (cihaz is null)
            {
                _logger.LogWarning("Aktif ZKTeco cihazı bulunamadı.");
                return;
            }

            // 3) Aynı gün + aynı kapı için açık kayıt var mı? → yön kararı
            var dayStart = now.Date;
            var dayEnd = dayStart.AddDays(1);

            var acikVar = await db.OgrenciDetaylar.AsNoTracking().AnyAsync(x =>
                x.OgrenciId == ogr.OgrenciId &&
                x.IstasyonTipi == cihaz.IstasyonTipi &&
                x.OgrenciCTarih == null &&
                x.OgrenciGTarih >= dayStart &&
                x.OgrenciGTarih < dayEnd);

            var girisMi = !acikVar; // açık yoksa GİRİŞ, varsa ÇIKIŞ

            // 4) Geçişi kaydet
            await svc.KaydetAsync(cihaz.CihazId, ogr.OgrenciId, cihaz.IstasyonTipi, now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kart okuma işleminde hata. Kart: {Kart}", norm);
        }
    }
}