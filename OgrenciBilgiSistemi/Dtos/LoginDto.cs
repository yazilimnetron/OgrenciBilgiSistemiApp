using System.ComponentModel.DataAnnotations;

namespace OgrenciBilgiSistemi.Dtos
{
    /// <summary>
    /// Giriş formu için özel DTO.
    /// KullaniciModel entity'si yerine bu DTO kullanılmalıdır;
    /// böylece model binding yalnızca giriş için gereken alanları alır.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = string.Empty;

        public bool BeniHatirla { get; set; } = false;
    }
}
