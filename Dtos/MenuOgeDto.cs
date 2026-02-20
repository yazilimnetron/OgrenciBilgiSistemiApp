namespace OgrenciBilgiSistemi.DTOs
{
    public class MenuOgeDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public int? AnaMenuId { get; set; }
        public int Sirala { get; set; }

        public List<MenuOgeDto> Children { get; set; } = new();

        public bool IsLeaf =>
            !string.IsNullOrWhiteSpace(Controller) &&
            !string.IsNullOrWhiteSpace(Action);
    }
}
