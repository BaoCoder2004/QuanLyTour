using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ThongTinDatVeTau
{
    public int DatVeID { get; set; }

    public int UserID { get; set; }

    public int ChuyenTauID { get; set; }

    public int? IDHanhKhach { get; set; }

    public string MaDatVe { get; set; }

    public DateTime NgayDatVe { get; set; } = DateTime.Now;

    public string HinhThucThanhToan { get; set; }

    public string TrangThaiThanhToan { get; set; } = "Chưa thanh toán";

    public string TrangThaiVe { get; set; } = "Đã đặt";

    public DateTime? NgayHetHanVe { get; set; }

    public string SoDienThoaiLienHe { get; set; }

    public string EmailLienHe { get; set; }

    public string GhiChu { get; set; }

    public virtual ThongTinChuyenTau ChuyenTau { get; set; }
}
