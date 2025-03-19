using System.ComponentModel.DataAnnotations;

namespace QuanLyTour.Models
{
    public class ThongTinNguoiDung
    {
        public int MaNguoiDung { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string? TenDangNhap { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string? MatKhau { get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string? TenNguoiDung { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }
        public bool TrangThai  { get; set; }
		[Required(ErrorMessage = "Email không được để trống")]
		public string? Email { get; set; }

		[Required(ErrorMessage = "Ngày sinh không được để trống")]
		[DataType(DataType.Date, ErrorMessage = "Ngày sinh không hợp lệ")]
		public DateTime? NgaySinh { get; set; }

		[Required(ErrorMessage = "Giới tính không được để trống")]
		public string? GioiTinh { get; set; } // Có thể là "Nam", "Nữ", "Khác"

	}
}
