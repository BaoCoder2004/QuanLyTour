namespace QuanLyTour.Models.KhachSan
{
    public class KhachSanViewModel
    {
        public int MaKhachSan { get; set; }
        public string? TenKhachSan { get; set; }
        public int? MaLoaiKhachSan { get; set; }
        public bool TrangThai { get; set; }
        public string? SoNgay { get; set; }
        public string? DiaDiem { get; set; }
        public decimal GiaKhachSan { get; set; }
        public string? HinhAnh1 { get; set; }
        public string? HinhAnh2 { get; set; }
        public string? HinhAnh3 { get; set; }
        public string? TenLoaiKhachSan { get; set; }    
        public string? MoTa { get; set; }
        public string? LichTrinh { get; set; }
    }
}
