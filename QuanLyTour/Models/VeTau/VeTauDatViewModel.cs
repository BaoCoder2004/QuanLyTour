namespace QuanLyTour.Models.VeTau
{
    public class VeTauDatViewModel
    {
        public int DatVeID { get; set; }
        public int ChuyenTauID { get; set; }
        public int? MaNguoiDung { get; set; }
        public string MaDatVe { get; set; } 
        public DateTime NgayDatVe { get; set; }
        public string HinhThucThanhToan { get; set; } 
        public string TrangThaiThanhToan { get; set; } 
        public string TrangThaiVe { get; set; } 
        public DateTime? NgayHetHanVe { get; set; }
        public string SoDienThoaiLienHe { get; set; } 
        public string EmailLienHe { get; set; } 
        public string? GhiChu { get; set; }
        public virtual VeTauViewModel? ThongTinChuyenTau { get; set; }
    }
}