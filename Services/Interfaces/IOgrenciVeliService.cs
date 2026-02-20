using OgrenciBilgiSistemi.Models;

namespace OgrenciBilgiSistemi.Services.Interfaces
{
    public interface IOgrenciVeliService
    {
        Task<int> EkleAsync(OgrenciVeliModel model, CancellationToken ct = default);
        Task GuncelleAsync(OgrenciVeliModel model, CancellationToken ct = default);
        Task SilAsync(int ogrenciVeliId, CancellationToken ct = default);
        Task<OgrenciVeliModel?> GetByIdAsync(int ogrenciVeliId, CancellationToken ct = default);
    }
}