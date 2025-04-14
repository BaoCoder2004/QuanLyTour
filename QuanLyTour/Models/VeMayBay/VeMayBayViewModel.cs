using System;

namespace QuanLyTour.Models.VeMayBay
{
    public class VeMayBayViewModel
    {
        // Vé
        public int MaVe { get; set; }
        public int IdChuyenBay { get; set; }
        public string HangVe { get; set; }
        public decimal GiaNet { get; set; }
        public decimal ThuePhi { get; set; }
        public decimal TongTien { get; set; }
        public int SoLuongVe { get; set; }
        public bool TrangThai { get; set; }
        public int? HanhLyXachTayKg { get; set; }
        public int? HanhLyKyGuiKg { get; set; }

        // Chuyến bay - chiều đi
        public string MaChuyenBay { get; set; }
        public string DiemDi { get; set; }
        public string DiemDen { get; set; }
        public DateTime GioKhoiHanh { get; set; }
        public DateTime GioHaCanh { get; set; }
        public bool CoTheDoiVe { get; set; }
        public bool CoTheHoanVe { get; set; }
        public decimal? PhiDoiVe { get; set; }
        public decimal? PhiHoanVe { get; set; }

        // Hãng hàng không
        public string TenHang { get; set; }
        public string LogoUrl { get; set; }
        public int DefaultHanhLyXachTayKg { get; set; }
        public int DefaultHanhLyKyGuiKg { get; set; }

        // Phân loại chuyến bay: 1 - nội địa, 2 - quốc tế
        public int LoaiChuyenBay { get; internal set; }

        // Thông tin chiều về (nếu có)
        public DateTime? GioKhoiHanhVe { get; set; }
        public DateTime? GioHaCanhVe { get; set; }
    }
}
