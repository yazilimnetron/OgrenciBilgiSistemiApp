namespace OgrenciBilgiSistemi.ViewModels
{
    public class KullaniciMenuAtamaVm
    {
        public int KullaniciId { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public List<MenuOgeAssignmentVm> Menuler { get; set; } = new();
        public List<int> SelectedMenuIds { get; set; } = new();
    }
}
