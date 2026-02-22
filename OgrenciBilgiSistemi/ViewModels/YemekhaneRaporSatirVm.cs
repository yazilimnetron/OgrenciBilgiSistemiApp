namespace OgrenciBilgiSistemi.ViewModels
{
    public class YemekhaneRaporSatirVm
    {
        public int OgrenciId { get; set; }
        public string OgrenciAdSoyad { get; set; } = "";
        public int Yil { get; set; }

        public decimal AylikTutar { get; set; }
        public int AktifAySayisi { get; set; }

        public decimal Borc { get; set; }
        public decimal Odenen { get; set; }
        public decimal Kalan => Math.Max(0, Borc - Odenen);
    }

    public class YemekhaneRaporListeVm
    {
        public IEnumerable<int> KullanilabilirYillar { get; set; } = Enumerable.Empty<int>();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public decimal ToplamBorc { get; set; }
        public decimal ToplamOdenen { get; set; }
        public decimal ToplamKalan => Math.Max(0, ToplamBorc - ToplamOdenen);

        public List<YemekhaneRaporSatirVm> Satirlar { get; set; } = new();
    }

    public class YemekhaneRaporVm
    {
        public int? Yil { get; set; }
        public string? Query { get; set; }
        public YemekhaneRaporListeVm Rapor { get; set; } = new();
    }
}
