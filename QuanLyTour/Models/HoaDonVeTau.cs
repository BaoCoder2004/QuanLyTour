namespace QuanLyTour.Models
{
    public class HoaDonVeTau
    {
        public int MaHoaDon { get; set; }
        public int MaNguoiDung { get; set; }
        public int ChuyenTauID { get; set; }
        public string? TenChuyenTau { get; set; }
        public string? TenNguoiDung { get; set; }
        public DateTime NgayDatVe { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayThanhToan { get; set; }
    }
}
