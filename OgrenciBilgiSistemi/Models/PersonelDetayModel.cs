using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Models.Enums;

namespace OgrenciBilgiSistemi.Models
{
    public class PersonelDetayModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PersonelDetayId { get; set; }

        [Required]
        public int PersonelId { get; set; }

        [ForeignKey(nameof(PersonelId))]
        public virtual PersonelModel Personel { get; set; } = default!;

        [Required]
        public IstasyonTipi IstasyonTipi { get; set; }

        public DateTime? PersonelGTarih { get; set; }
        public DateTime? PersonelCTarih { get; set; }
        [StringLength(5)]
        [Column(TypeName = "nvarchar(5)")]
        public string? PersonelGecisTipi { get; set; }

        public bool? PersonelSmsGonderildi { get; set; } = false;

        [Column(TypeName = "nvarchar(255)")]
        public string? PersonelResimYolu { get; set; }

        public int? CihazId { get; set; }

        [ForeignKey(nameof(CihazId))]
        public virtual CihazModel? Cihaz { get; set; }
    }
}