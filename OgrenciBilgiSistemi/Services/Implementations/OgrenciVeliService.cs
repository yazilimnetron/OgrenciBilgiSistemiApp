using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Models;
using OgrenciBilgiSistemi.Services.Interfaces;

namespace OgrenciBilgiSistemi.Services.Implementations
{
    public sealed class OgrenciVeliService : IOgrenciVeliService
    {
        private readonly AppDbContext _db;

        public OgrenciVeliService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> EkleAsync(OgrenciVeliModel model, CancellationToken ct = default)
        {
            await _db.OgrenciVeliler.AddAsync(model, ct);  
            await _db.SaveChangesAsync(ct);
            return model.OgrenciVeliId;
        }

        public async Task GuncelleAsync(OgrenciVeliModel model, CancellationToken ct = default)
        {
            _db.OgrenciVeliler.Update(model);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SilAsync(int ogrenciVeliId, CancellationToken ct = default)
        {
            var veli = await _db.OgrenciVeliler
                .FirstOrDefaultAsync(v => v.OgrenciVeliId == ogrenciVeliId, ct);

            if (veli is null) return;

            _db.OgrenciVeliler.Remove(veli);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<OgrenciVeliModel?> GetByIdAsync(int ogrenciVeliId, CancellationToken ct = default)
        {
            return await _db.OgrenciVeliler
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.OgrenciVeliId == ogrenciVeliId, ct);
        }
    }
}