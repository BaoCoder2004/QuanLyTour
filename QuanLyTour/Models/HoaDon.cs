namespace QuanLyTour.Models
{
    public class HoaDon
    {
        public int MaHoaDon { get; set; }
        public int MaPhieu { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaTour { get; set; }
        public string? TenTour { get; set; }
        public string? TenNguoiDung { get; set; }
        public DateTime NgayDatTour { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayThanhToan { get; set; }

    }
}
