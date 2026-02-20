using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Models.Enums;
using OgrenciBilgiSistemi.Services.Interfaces;
using System.Runtime.InteropServices;
using zkemkeeper;

namespace OgrenciBilgiSistemi.Services.Implementations
{
    public class ZKTecoService : IZKTecoService
    {
        private const int MACHINE_NO = 1;

        private readonly ILogger<ZKTecoService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private CZKEM _zkemkeeper;
        private readonly SemaphoreSlim _connLock = new(1, 1);

        private event Func<string, Task>? _internalCardHandler;

        // Bağlanılan cihaz bilgisi (tek cihaz senaryosu için)
        public int? CurrentCihazId { get; private set; }
        public IstasyonTipi? CurrentIstasyonTipi { get; private set; }

        public bool IsConnected { get; private set; }

        public ZKTecoService(ILogger<ZKTecoService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
            _zkemkeeper = new CZKEM();
        }

        public event Func<string, Task> OnCardReadAsync
        {
            add => _internalCardHandler += value;
            remove => _internalCardHandler -= value;
        }

        public async Task<bool> ConnectAsync()
        {
            await _connLock.WaitAsync();
            try
            {
                if (IsConnected) return true;

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // 🔧 DonanimTipi'ni kullan
                var cihaz = await db.Cihazlar.AsNoTracking()
                    .Where(c => c.Aktif
                             && c.DonanimTipi == DonanimTipi.ZKTeco
                             && !string.IsNullOrWhiteSpace(c.IpAdresi)
                             && c.PortNo.HasValue && c.PortNo.Value > 0)
                    .OrderBy(c => c.CihazId)
                    .FirstOrDefaultAsync();

                if (cihaz is null)
                {
                    _logger.LogWarning("Uygun ZKTeco cihazı bulunamadı (Aktif/ZKTeco/IP/Port).");
                    return false;
                }

                // Her bağlanışta temiz bir COM nesnesi
                TryDisposeCom();
                _zkemkeeper = new CZKEM();

                if (!_zkemkeeper.Connect_Net(cihaz.IpAdresi, cihaz.PortNo!.Value))
                {
                    _logger.LogWarning("ZKTeco cihazına bağlanılamadı: {Ip}:{Port}", cihaz.IpAdresi, cihaz.PortNo);
                    return false;
                }

                // Tüm eventler
                _zkemkeeper.RegEvent(MACHINE_NO, 0xFFFF);

                // Çift kayıt engeli
                _zkemkeeper.OnAttTransactionEx -= zkem_OnAttTransactionEx;
                _zkemkeeper.OnHIDNum -= zkem_OnHIDNum;

                _zkemkeeper.OnAttTransactionEx += zkem_OnAttTransactionEx;
                _zkemkeeper.OnHIDNum += zkem_OnHIDNum;

                // Bağlanılan cihazı hatırla
                CurrentCihazId = cihaz.CihazId;
                CurrentIstasyonTipi = cihaz.IstasyonTipi;

                IsConnected = true;
                _logger.LogInformation("ZKTeco bağlandı: {Ip}:{Port} (CihazId:{Id}, Istasyon:{Ist})",
                    cihaz.IpAdresi, cihaz.PortNo, cihaz.CihazId, cihaz.IstasyonTipi);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZKTeco ConnectAsync hatası.");
                return false;
            }
            finally
            {
                _connLock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await _connLock.WaitAsync();
            try
            {
                if (!IsConnected) return;

                try
                {
                    _zkemkeeper.OnAttTransactionEx -= zkem_OnAttTransactionEx;
                    _zkemkeeper.OnHIDNum -= zkem_OnHIDNum;
                }
                catch { /* yut */ }

                try { _zkemkeeper.Disconnect(); } catch { /* yut */ }
                TryDisposeCom();
                _zkemkeeper = new CZKEM();

                IsConnected = false;
                _internalCardHandler = null;
                CurrentCihazId = null;
                CurrentIstasyonTipi = null;

                _logger.LogInformation("ZKTeco bağlantısı kapatıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZKTeco DisconnectAsync hatası.");
            }
            finally
            {
                _connLock.Release();
            }
        }

        public async Task MonitorConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected)
                    {
                        _logger.LogDebug("ZKTeco bağlı değil. Yeniden bağlanma deneniyor…");
                        var ok = await ConnectAsync();
                        if (ok) _logger.LogInformation("ZKTeco yeniden bağlandı.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MonitorConnections döngü hatası.");
                }

                try { await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken); }
                catch (TaskCanceledException) { /* iptal edildi */ }
            }
        }

        // --------- Event handlers ---------

        // Genel geçiş event’i
        private void zkem_OnAttTransactionEx(
            string EnrollNumber,
            int IsInValid,
            int AttState,
            int VerifyMethod,
            int Year,
            int Month,
            int Day,
            int Hour,
            int Minute,
            int Second,
            int Workcode)
        {
            var kartNo = (EnrollNumber ?? string.Empty).Trim();

            _logger.LogInformation(
                "ZK AttTransaction: Kart={Kart}, Zaman={Y}-{M}-{D} {H}:{Min}:{S}, Verify={Verify}, State={State}, CihazId={CihazId}, Istasyon={Ist}",
                kartNo, Year, Month, Day, Hour, Minute, Second, VerifyMethod, AttState, CurrentCihazId, CurrentIstasyonTipi);

            if (string.IsNullOrWhiteSpace(kartNo) || _internalCardHandler is null) return;

            foreach (var handler in _internalCardHandler.GetInvocationList())
            {
                if (handler is Func<string, Task> asyncHandler)
                {
                    _ = Task.Run(async () =>
                    {
                        try { await asyncHandler(kartNo); }
                        catch (Exception ex) { _logger.LogError(ex, "OnCardReadAsync handler hatası."); }
                    });
                }
            }
        }

        // Bazı cihazlarda kart numarası bu event ile gelir
        private void zkem_OnHIDNum(int CardNumber)
        {
            var kartNo = CardNumber.ToString();
            _logger.LogInformation("ZK HIDNum: Kart={Kart}, CihazId={CihazId}, Istasyon={Ist}", kartNo, CurrentCihazId, CurrentIstasyonTipi);

            if (string.IsNullOrWhiteSpace(kartNo) || _internalCardHandler is null) return;

            foreach (var handler in _internalCardHandler.GetInvocationList())
            {
                if (handler is Func<string, Task> asyncHandler)
                {
                    _ = Task.Run(async () =>
                    {
                        try { await asyncHandler(kartNo); }
                        catch (Exception ex) { _logger.LogError(ex, "OnCardReadAsync handler hatası (HIDNum)."); }
                    });
                }
            }
        }

        // --------- Helpers ---------

        private void TryDisposeCom()
        {
            try
            {
                if (_zkemkeeper is not null && Marshal.IsComObject(_zkemkeeper))
                {
                    Marshal.FinalReleaseComObject(_zkemkeeper);
                }
            }
            catch { /* yut */ }
        }
    }
}






//using Microsoft.EntityFrameworkCore;
//using OgrenciBilgiSistemi.Data;
//using zkemkeeper;
//using System.Runtime.InteropServices;

//namespace OgrenciBilgiSistemi.Services
//{
//    public class ZKTecoService : IZKTecoService
//    {
//        private readonly ILogger<ZKTecoService> _logger;
//        private readonly IServiceScopeFactory _serviceScopeFactory;
//        private CZKEM _zkemkeeper;

//        private event Func<string, Task>? _internalCardHandler;

//        public bool IsConnected { get; private set; }

//        public ZKTecoService(ILogger<ZKTecoService> logger, IServiceScopeFactory serviceScopeFactory)
//        {
//            _logger = logger;
//            _serviceScopeFactory = serviceScopeFactory;
//            _zkemkeeper = new CZKEM();
//        }

//        public event Func<string, Task> OnCardReadAsync
//        {
//            add => _internalCardHandler += value;
//            remove => _internalCardHandler -= value;
//        }

//        public async Task<bool> ConnectAsync()
//        {
//            try
//            {
//                using var scope = _serviceScopeFactory.CreateScope();
//                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//                var cihaz = await context.Cihazlar.AsNoTracking().FirstOrDefaultAsync();

//                if (cihaz == null)
//                {
//                    _logger.LogError("ZKTeco cihaz bilgisi veritabanında bulunamadı.");
//                    return false;
//                }

//                bool connected = await Task.Run(() => _zkemkeeper.Connect_Net(cihaz.IpAdresi, cihaz.PortNo));
//                IsConnected = connected;

//                if (IsConnected)
//                {
//                    _logger.LogInformation("ZKTeco cihazına bağlanıldı: {Ip}:{Port}", cihaz.IpAdresi, cihaz.PortNo);

//                    // Makine numarası veritabanından alınmalı
//                    int machineNumber = cihaz.CihazId > 0 ? cihaz.CihazId : 1;
//                    _zkemkeeper.RegEvent(machineNumber, 65535);

//                    _zkemkeeper.OnAttTransactionEx += new _IZKEMEvents_OnAttTransactionExEventHandler(zkem_OnAttTransactionEx);
//                }
//                else
//                {
//                    _logger.LogError("ZKTeco cihazına bağlanılamadı: {Ip}:{Port}", cihaz.IpAdresi, cihaz.PortNo);
//                }

//                return IsConnected;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "ZKTeco cihazına bağlanırken hata oluştu.");
//                return false;
//            }
//        }

//        public async Task DisconnectAsync()
//        {
//            if (!IsConnected)
//                return;

//            try
//            {
//                _zkemkeeper.OnAttTransactionEx -= new _IZKEMEvents_OnAttTransactionExEventHandler(zkem_OnAttTransactionEx);
//                _zkemkeeper.Disconnect();

//                if (Marshal.IsComObject(_zkemkeeper))
//                {
//                    Marshal.ReleaseComObject(_zkemkeeper);
//                }

//                IsConnected = false;
//                _internalCardHandler = null;

//                _logger.LogInformation("ZKTeco cihazı bağlantısı kesildi.");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "ZKTeco cihazından çıkış yapılırken hata oluştu.");
//            }

//            await Task.CompletedTask;
//        }

//        private void zkem_OnAttTransactionEx(
//            string EnrollNumber,
//            int IsInValid,
//            int AttState,
//            int VerifyMethod,
//            int Year,
//            int Month,
//            int Day,
//            int Hour,
//            int Minute,
//            int Second,
//            int Workcode)
//        {
//            string kartNo = EnrollNumber?.Trim() ?? string.Empty;

//            _logger.LogInformation("ZKTeco kart okuma: {KartNo} [{Y}-{M}-{D} {H}:{Min}:{S}]",
//                kartNo, Year, Month, Day, Hour, Minute, Second);

//            if (string.IsNullOrWhiteSpace(kartNo) || _internalCardHandler == null)
//                return;

//            foreach (var handler in _internalCardHandler.GetInvocationList())
//            {
//                if (handler is Func<string, Task> asyncHandler)
//                {
//                    _ = Task.Run(async () =>
//                    {
//                        try
//                        {
//                            await asyncHandler(kartNo);
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogError(ex, "OnCardReadAsync handler çalıştırılırken hata oluştu.");
//                        }
//                    });
//                }
//            }
//        }

//        public async Task MonitorConnectionsAsync(CancellationToken cancellationToken)
//        {
//            while (!cancellationToken.IsCancellationRequested)
//            {
//                if (!IsConnected)
//                {
//                    _logger.LogWarning("ZKTeco bağlantısı kesilmiş. Yeniden bağlanma deneniyor...");
//                    bool reconnected = await ConnectAsync();
//                    if (reconnected)
//                        _logger.LogInformation("ZKTeco cihazına yeniden bağlanıldı.");
//                    else
//                        _logger.LogWarning("Yeniden bağlanma başarısız. 3 saniye sonra tekrar deneniyor...");
//                }

//                await Task.Delay(3000, cancellationToken);
//            }
//        }
//    }
//}
