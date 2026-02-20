using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemi.Models.Enums
{
    public enum PersonelTipi : byte
    {
        [Display(Name = "Öğretmen")]
        Ogretmen = 1,

        [Display(Name = "Personel")]
        Personel = 2,

        [Display(Name = "Ziyaretçi")]
        Ziyaretci = 3
    }

    public enum PersonelFiltre
    {
        Aktif = 1,
        Pasif = 2,
        Tum = 0
    }
}
