using Microsoft.AspNetCore.Mvc.Rendering;
using OgrenciBilgiSistemi.Dtos;
using OgrenciBilgiSistemi.Models.Enums;

namespace OgrenciBilgiSistemi.ViewModels
{
    public sealed class ZiyaretciRaporVm
    {
        // --- Filtre Alanları ---
        public string? query { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? birimId { get; set; }
        public int? personelId { get; set; }
        public string? kartNo { get; set; }

        // --- Dropdownlar ---
        public List<SelectListItem> Birimler { get; set; } = new();
        public List<SelectListItem> Personeller { get; set; } = new();

        // --- Rapor Sonucu ---
        public List<ZiyaretciRaporDto> Rapor { get; set; } = new();
    }
}
