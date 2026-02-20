using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgrenciBilgiSistemi.Models
{
    public class OgretmenModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OgretmenId { get; set; }

        [Required(ErrorMessage = "Öğretmen Adı Soyadı alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olmalıdır.")]
        public string OgretmenAdSoyad { get; set; } = string.Empty;

        public string? OgretmenGorsel { get; set; }

        public bool OgretmenDurum { get; set; } = true; 
        public string? OgretmenKartNo { get; set; }
        public int? BirimId { get; set; }

        [ForeignKey("BirimId")]
        [ValidateNever]
        public virtual BirimModel? Birim { get; set; }

        // Öğretmen ile ilişkilendirilmiş Öğrenciler
        [ValidateNever]
        public virtual List<OgrenciModel> Ogrenciler { get; set; } = new();

        // UI İçin Kullanılacak Alanlar (veritabanına yansıtılmaz)
        [NotMapped]
        public IFormFile? OgretmenGorselFile { get; set; }

        [NotMapped]
        public List<SelectListItem> Birimler { get; set; } = new();
    }
}