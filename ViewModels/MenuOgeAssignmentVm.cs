namespace OgrenciBilgiSistemi.ViewModels
{
    public class MenuOgeAssignmentVm
    {
        public int MenuOgeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }

        public int? AnaMenuId { get; set; }
        public List<MenuOgeAssignmentVm> Children { get; set; } = new();
    }
}
