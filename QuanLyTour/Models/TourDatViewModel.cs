namespace QuanLyTour.Models
{
    public class TourDatViewModel
    {
        public string TenTour { get; set; }
        public string LoaiTour { get; set; }
        public DateTime NgayDat { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public DateTime NgayDi { get; set; }
    }
}
