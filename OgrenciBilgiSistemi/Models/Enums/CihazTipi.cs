namespace OgrenciBilgiSistemi.Models.Enums
{
    // Donanım türü (değişebilir)
    public enum DonanimTipi : byte
    {
        UsbRfid = 1,
        ZKTeco = 2,
        QrOkuyucu = 3,
        Diger = 9
    }

    // İstasyon/kapı türü (raporlama bununla yapılır)
    public enum IstasyonTipi : short
    {
        Bilinmiyor = 0,

        AnaKapi = 10,
        Yemekhane = 20,

        //// İleride yaygın ekler
        //ArkaKapi = 30,
        //SporSalonu = 40,
        //Kutuphane = 50,
        //YurtGiris = 60,

        //Diger = 999
    }
}