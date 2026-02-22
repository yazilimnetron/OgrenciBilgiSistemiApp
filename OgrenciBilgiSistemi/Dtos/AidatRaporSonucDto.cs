namespace OgrenciBilgiSistemi.Dtos
{
    public class AidatRaporSonucDto
    {
        public PaginatedListModel<AidatRaporDto> Satirlar { get; set; } = null!;

        public decimal ToplamBorc { get; set; }
        public decimal ToplamOdenenGosterilen { get; set; }
        public decimal ToplamKalan { get; set; }

        public List<int> KullanilabilirYillar { get; set; } = new();
    }
}
