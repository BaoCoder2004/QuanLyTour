using System;

namespace QuanLyTour.Models.VeMayBay
{
    public class ChuyenBay
    {
        public int IdChuyenBay { get; set; }
        public int IdHangHangKhong { get; set; }
        public string MaChuyenBay { get; set; }
        public string DiemDi { get; set; }
        public string DiemDen { get; set; }
        public DateTime GioKhoiHanh { get; set; }
        public DateTime GioHaCanh { get; set; }
        public bool CoTheDoiVe { get; set; }
        public bool CoTheHoanVe { get; set; }
        public decimal? PhiDoiVe { get; set; }
        public decimal? PhiHoanVe { get; set; }
    }
}
