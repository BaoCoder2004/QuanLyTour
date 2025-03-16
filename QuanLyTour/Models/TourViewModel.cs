namespace QuanLyTour.Models
{
    public class TourViewModel
    {
        public int MaTour { get; set; }
        public string? TenTour { get; set; }
        public int? MaLoaiTour { get; set; }
        public bool TrangThai { get; set; }
		public string? SoNgay { get; set; }
		public string? DiaDiem { get; set; }
		public decimal GiaTour { get; set; }
        public string? HinhAnh1 { get; set; }
        public string? HinhAnh2 { get; set; }
        public string? HinhAnh3 { get; set; }
        public string? TenLoaiTour { get; set; }
        public string? MoTa {  get; set; }
        public string? LichTrinh { get;set; }
    }
}
