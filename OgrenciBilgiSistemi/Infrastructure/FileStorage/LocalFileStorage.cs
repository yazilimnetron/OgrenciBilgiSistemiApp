using OgrenciBilgiSistemi.Abstractions;

namespace OgrenciBilgiSistemi.Infrastructure.FileStorage
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly long _maxBytes = 2 * 1024 * 1024;
        private static readonly string[] _allowed = [".jpg", ".jpeg", ".png"];

        // İzin verilen dosya formatlarının magic byte (imza) değerleri
        private static readonly Dictionary<string, byte[]> _magicBytes = new()
        {
            { ".jpg",  new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 } }
        };

        public async Task<string?> SaveImageAsync(IFormFile file, string? existingPath = null, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0) return existingPath;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowed.Contains(ext))
                throw new InvalidOperationException("Yalnızca JPG ve PNG dosyalarına izin verilir.");
            if (file.Length > _maxBytes)
                throw new InvalidOperationException("Dosya boyutu en fazla 2MB olabilir.");

            // Magic byte doğrulaması — uzantı değiştirilerek yüklenen dosyaları engeller
            if (!await IsValidImageAsync(file, ext, ct))
                throw new InvalidOperationException("Dosya içeriği belirtilen formatla uyuşmuyor.");

            // eski dosyayı sil
            if (!string.IsNullOrEmpty(existingPath))
            {
                var old = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPath.TrimStart('/'));
                if (File.Exists(old)) File.Delete(old);
            }

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var name = Guid.NewGuid() + ext;
            var path = Path.Combine(dir, name);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            return "/uploads/" + name;
        }

        private static async Task<bool> IsValidImageAsync(IFormFile file, string ext, CancellationToken ct)
        {
            if (!_magicBytes.TryGetValue(ext, out var signature))
                return false;

            var buffer = new byte[signature.Length];
            using var stream = file.OpenReadStream();
            int read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);

            if (read < signature.Length)
                return false;

            for (int i = 0; i < signature.Length; i++)
            {
                if (buffer[i] != signature[i])
                    return false;
            }

            return true;
        }
    }
}
