using System;

namespace QuanLyTour.Models.VeMayBay
{
    public class VeMayBayBookingViewModel
    {
        public int Id { get; set; }
        public int MaVe { get; set; }
        public string HoTenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }        // nếu bạn lưu địa chỉ
        public DateTime NgayDat { get; set; }
        public int SoLuongVe { get; set; }
        public decimal GiaTaiThoiDiemDat { get; set; }
        public decimal ThuePhiTaiThoiDiemDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
    }
}
