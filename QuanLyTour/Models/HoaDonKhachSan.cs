namespace QuanLyTour.Models
{
	public class HoaDonKhachSan
	{
		public int MaHoaDon { get; set; }
		public int MaNguoiDung { get; set; }
		public int MaKhachSan { get; set; }
		public string? TenkhachSan { get; set; }
		public string? TenNguoiDung { get; set; }
		public DateTime NgayDatKhachSan { get; set; }
		public decimal TongTien { get; set; }
		public DateTime NgayThanhToan { get; set; }
	}
}
