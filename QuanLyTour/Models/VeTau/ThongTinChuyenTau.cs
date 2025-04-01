using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ThongTinChuyenTau
{
    public int ChuyenTauID { get; set; }

    public string TenChuyenTau { get; set; }

    public string GaDi { get; set; }

    public string GaDen { get; set; }

    public DateTime NgayKhoiHanh { get; set; }

    public DateTime NgayDen { get; set; }

    public int SoLuongVe { get; set; }

    public decimal GiaVe { get; set; }

    // Liên kết danh sách vé đã đặt
    public virtual ICollection<ThongTinDatVeTau> DanhSachVe { get; set; }
}
