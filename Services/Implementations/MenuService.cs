using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.DTOs;
using OgrenciBilgiSistemi.Models;
using OgrenciBilgiSistemi.Services.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace OgrenciBilgiSistemi.Services.Implementations
{
    public class MenuService : IMenuService
    {
        private static readonly List<MenuOgeModel> ListEmpty = new(0);

        private readonly AppDbContext _db;
        private readonly ILogger<MenuService> _log;

        public MenuService(AppDbContext db, ILogger<MenuService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<List<MenuOgeDto>> GetSidebarForUserAsync(
    int kullaniciId,
    ClaimsPrincipal user,
    CancellationToken ct = default)
        {
            // 1) Kullanıcı & atanmış menüler
            var kullanici = await _db.Kullanicilar
                .AsNoTracking()
                .Include(k => k.KullaniciMenuler)
                .FirstOrDefaultAsync(k => k.KullaniciId == kullaniciId, ct);

            if (kullanici is null)
            {
                _log.LogWarning("Kullanıcı bulunamadı: {Id}", kullaniciId);
                return new();
            }

            // 2) Admin / roller / atamalar (rol >= atama politikası korunuyor)
            var isAdmin = kullanici.AdminMi || user.IsInRole("Admin");
            var assignedMenuIds = kullanici.KullaniciMenuler.Select(x => x.MenuOgeId).ToHashSet();
            var userRoleSet = GetUserRoles(user); // lower-case set

            // 3) Tüm menüler
            var allMenus = await _db.MenuOgeler.AsNoTracking().ToListAsync(ct);
            if (allMenus.Count == 0) return new();

            // 4) ToLookup ile ağaç: null kökler güvenli, gruplar O(1)
            var lookup = allMenus
                .OrderBy(m => m.Sirala)
                .ToLookup(m => m.AnaMenuId);          // ILookup<int?, MenuOgeModel>

            // Yardımcılar
            IEnumerable<MenuOgeModel> Roots() => lookup[null];       // kökler (AnaMenuId == null)
            IEnumerable<MenuOgeModel> ChildrenOf(int id) => lookup[id];

            // 5) Görünürlük + memoization + cycle-guard
            var canSeeMemo = new Dictionary<int, bool>();
            var visibleMemo = new Dictionary<int, bool>();

            bool CanSeeNode(MenuOgeModel m)
            {
                if (canSeeMemo.TryGetValue(m.Id, out var cached)) return cached;

                bool result;
                if (isAdmin)
                {
                    result = true;
                }
                else
                {
                    // Rol kısıtı varsa ve kullanıcı rolü taşımıyorsa: görünmez
                    if (!string.IsNullOrWhiteSpace(m.GerekliRole))
                    {
                        var required = SplitRoles(m.GerekliRole);
                        if (!userRoleSet.Overlaps(required))
                        {
                            canSeeMemo[m.Id] = false;
                            return false;
                        }
                    }
                    // Rol uygun ise: doğrudan ataması varsa görülebilir
                    result = assignedMenuIds.Contains(m.Id);
                }

                canSeeMemo[m.Id] = result;
                return result;
            }

            bool IsVisibleWithChildrenFlat(MenuOgeModel node, HashSet<int>? visited = null)
            {
                if (visibleMemo.TryGetValue(node.Id, out var cached)) return cached;

                visited ??= new HashSet<int>();
                if (!visited.Add(node.Id))
                {
                    _log.LogWarning("Menu döngüsü tespit edildi. NodeId={Id}", node.Id);
                    visibleMemo[node.Id] = false;
                    return false;
                }

                if (CanSeeNode(node))
                {
                    visibleMemo[node.Id] = true;
                    visited.Remove(node.Id);
                    return true;
                }

                foreach (var child in ChildrenOf(node.Id))
                {
                    if (IsVisibleWithChildrenFlat(child, visited))
                    {
                        visibleMemo[node.Id] = true;
                        visited.Remove(node.Id);
                        return true;
                    }
                }

                visibleMemo[node.Id] = false;
                visited.Remove(node.Id);
                return false;
            }

            MenuOgeDto ToDtoFilteredFlat(MenuOgeModel node, HashSet<int>? visited = null)
            {
                visited ??= new HashSet<int>();
                if (!visited.Add(node.Id))
                {
                    _log.LogWarning("Menu DTO dönüşümünde döngü tespit edildi. NodeId={Id}", node.Id);
                    return new MenuOgeDto
                    {
                        Id = node.Id,
                        Baslik = node.Baslik,
                        Controller = node.Controller,
                        Action = node.Action,
                        AnaMenuId = node.AnaMenuId,
                        Sirala = node.Sirala,
                        Children = new()
                    };
                }

                var dto = new MenuOgeDto
                {
                    Id = node.Id,
                    Baslik = node.Baslik,
                    Controller = node.Controller,
                    Action = node.Action,
                    AnaMenuId = node.AnaMenuId,
                    Sirala = node.Sirala
                };

                var kids = ChildrenOf(node.Id)
                    .Where(m => IsVisibleWithChildrenFlat(m))
                    .Select(m => ToDtoFilteredFlat(m, visited))
                    .ToList();

                dto.Children = kids;

                visited.Remove(node.Id);
                return dto;
            }

            // 6) Kökler → görünür filtre → DTO (method group yerine lambda!)
            var visibleRoots = Roots().Where(m => IsVisibleWithChildrenFlat(m));
            var result = visibleRoots.Select(r => ToDtoFilteredFlat(r)).ToList();
            return result;
        }

        private static HashSet<string> GetUserRoles(ClaimsPrincipal user)
        {
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToHashSet();

            return roles;
        }

        private static HashSet<string> SplitRoles(string rolesCsv)
        {
            var roles = (rolesCsv ?? string.Empty)
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim().ToLowerInvariant())
                .ToHashSet();

            return roles;
        }
    }
}



//using Microsoft.EntityFrameworkCore;
//using OgrenciBilgiSistemi.Data;
//using OgrenciBilgiSistemi.DTOs;
//using OgrenciBilgiSistemi.Models;
//using OgrenciBilgiSistemi.Services.Interfaces;
//using System.Security.Claims;

//namespace OgrenciBilgiSistemi.Services.Implementations
//{
//    public class MenuService : IMenuService
//    {
//        private readonly AppDbContext _db;
//        private readonly ILogger<MenuService> _log;

//        public MenuService(AppDbContext db, ILogger<MenuService> log)
//        {
//            _db = db;
//            _log = log;
//        }

//        public async Task<List<MenuOgeDto>> GetSidebarForUserAsync(
//    int kullaniciId,
//    ClaimsPrincipal user,
//    CancellationToken ct = default)
//        {
//            // 1) Kullanıcı, atanmış menüler
//            var kullanici = await _db.Kullanicilar
//                .AsNoTracking()
//                .Include(k => k.KullaniciMenuler)
//                .FirstOrDefaultAsync(k => k.KullaniciId == kullaniciId, ct);

//            if (kullanici is null)
//            {
//                _log.LogWarning("Kullanıcı bulunamadı: {Id}", kullaniciId);
//                return new();
//            }

//            // 2) Admin kontrolü hem claim hem DB bayrağıyla
//            var isAdmin = kullanici.AdminMi || user.IsInRole("Admin");

//            var assignedMenuIds = kullanici.KullaniciMenuler
//                .Select(x => x.MenuOgeId)
//                .ToHashSet();

//            var userRoleSet = GetUserRoles(user); // lower-case set

//            // 3) Tüm menüleri TEK QUERY ile düz liste olarak çek
//            var allMenus = await _db.MenuOgeler
//                .AsNoTracking()
//                .ToListAsync(ct);

//            // Çocuk bulucu: navigation'a değil düz listeye bak
//            List<MenuOgeModel> ChildrenOf(int parentId) =>
//                allMenus.Where(x => x.AnaMenuId == parentId)
//                        .OrderBy(x => x.Sirala)
//                        .ToList();

//            bool CanSeeNode(MenuOgeModel m)
//            {
//                if (isAdmin) return true;

//                // Role kısıtı
//                if (!string.IsNullOrWhiteSpace(m.GerekliRole))
//                {
//                    var required = SplitRoles(m.GerekliRole);
//                    if (!userRoleSet.Overlaps(required))
//                        return false;
//                }

//                // Doğrudan atandıysa
//                if (assignedMenuIds.Contains(m.Id))
//                    return true;

//                // Değilse, herhangi bir torunu atandıysa parent görünmeli (derin arama)
//                return HasAssignedDescendant(m.Id);
//            }

//            bool HasAssignedDescendant(int nodeId)
//            {
//                foreach (var child in ChildrenOf(nodeId))
//                {
//                    if (assignedMenuIds.Contains(child.Id))
//                        return true;
//                    if (HasAssignedDescendant(child.Id))
//                        return true;
//                }
//                return false;
//            }

//            bool IsVisibleWithChildrenFlat(MenuOgeModel node)
//            {
//                if (CanSeeNode(node)) return true;
//                foreach (var child in ChildrenOf(node.Id))
//                    if (IsVisibleWithChildrenFlat(child))
//                        return true;
//                return false;
//            }

//            MenuOgeDto ToDtoFilteredFlat(MenuOgeModel node)
//            {
//                var dto = new MenuOgeDto
//                {
//                    Id = node.Id,
//                    Baslik = node.Baslik,
//                    Controller = node.Controller,
//                    Action = node.Action,
//                    AnaMenuId = node.AnaMenuId,
//                    Sirala = node.Sirala
//                };

//                var kids = ChildrenOf(node.Id)
//                    .Where(IsVisibleWithChildrenFlat)
//                    .Select(ToDtoFilteredFlat)
//                    .ToList();

//                if (kids.Count > 0)
//                    dto.Children = kids;

//                return dto;
//            }

//            // 4) Top-level kökler + görünürlük + DTO
//            var roots = allMenus
//                .Where(m => m.AnaMenuId == null)
//                .OrderBy(m => m.Sirala)
//                .Where(IsVisibleWithChildrenFlat)
//                .ToList();

//            var result = roots.Select(ToDtoFilteredFlat).ToList();
//            return result;
//        }

//        // En az bir child görünür mü? Kendisi veya çocuklarından biri görülebiliyorsa true
//        private static bool IsVisibleWithChildren(MenuOgeModel node, Func<MenuOgeModel, bool> canSee)
//        {
//            if (canSee(node)) return true;
//            if (node.AltMenuler == null || node.AltMenuler.Count == 0) return false;
//            return node.AltMenuler.Any(child => IsVisibleWithChildren(child, canSee));
//        }

//        private static MenuOgeDto ToDtoFiltered(MenuOgeModel node, Func<MenuOgeModel, bool> canSee)
//        {
//            var dto = new MenuOgeDto
//            {
//                Id = node.Id,
//                Baslik = node.Baslik,
//                Controller = node.Controller,
//                Action = node.Action,
//                Sirala = node.Sirala
//            };

//            if (node.AltMenuler != null && node.AltMenuler.Count > 0)
//            {
//                var children = node.AltMenuler
//                    .Where(c => IsVisibleWithChildren(c, canSee))
//                    .OrderBy(c => c.Sirala)
//                    .Select(c => ToDtoFiltered(c, canSee))
//                    .ToList();

//                dto.Children = children;
//            }

//            return dto;
//        }

//        private static HashSet<string> GetUserRoles(ClaimsPrincipal user)
//        {
//            var roles = user.Claims
//                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
//                .Select(c => c.Value)
//                .Where(v => !string.IsNullOrWhiteSpace(v))
//                .Select(v => v.Trim())
//                .Select(v => v.ToLowerInvariant())
//                .ToHashSet();

//            return roles;
//        }

//        private static HashSet<string> SplitRoles(string rolesCsv)
//        {
//            var roles = (rolesCsv ?? string.Empty)
//                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
//                .Select(r => r.Trim().ToLowerInvariant())
//                .ToHashSet();

//            return roles;
//        }
//    }
//}
