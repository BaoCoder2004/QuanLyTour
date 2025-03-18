namespace QuanLyTour.Models.Tour
{
    public class PhieuDatTour
    {
        public int MaPhieu { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaTour { get; set; }
        public string? TenTour { get; set; }
        public decimal GiaTour { get; set; }
        public int SoLuong { get; set; }
        public string? TenNguoiDung { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public DateTime NgayDatTour { get; set; }
        public DateTime NgayDi { get; set; }

    }
}
