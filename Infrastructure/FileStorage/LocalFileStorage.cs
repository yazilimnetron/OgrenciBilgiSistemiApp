using OgrenciBilgiSistemi.Abstractions;

public class LocalFileStorage : IFileStorage
{
    private readonly long _maxBytes = 2 * 1024 * 1024;
    private static readonly string[] _allowed = [".jpg", ".jpeg", ".png"];

    public async Task<string?> SaveImageAsync(IFormFile file, string? existingPath = null, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0) return existingPath;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowed.Contains(ext)) throw new InvalidOperationException("Yalnızca JPG ve PNG dosyalarına izin verilir.");
        if (file.Length > _maxBytes) throw new InvalidOperationException("Dosya boyutu en fazla 2MB olabilir.");

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
}