using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTour.Models;
using QuanLyTour.Models.KhachSan;
using QuanLyTour.Models.VeMayBay;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanLyTour.Controllers
{
    public class VeMayBayController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<VeMayBayController> _logger;

        public VeMayBayController(ILogger<VeMayBayController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Hiển thị danh sách vé máy bay đang còn (TrangThai = 1)
        public IActionResult Index()
        {
            // Kiểm tra thông tin người dùng từ Session1
            var tenNguoiDung = HttpContext.Session.GetString("TenNguoiDung");
            ViewBag.TenNguoiDung = tenNguoiDung;

            var VeMayBaysNoiDia = new List<VeMayBayViewModel>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var sql = @"
            SELECT 
                v.MaVe, 
                v.IdChuyenBay,
                v.HangVe, 
                (v.GiaNet + v.ThuePhi) AS TongTien,
                v.SoLuongVe,
                cb.MaChuyenBay,
                cb.DiemDi, 
                cb.DiemDen, 
                cb.GioKhoiHanh, 
                cb.GioHaCanh,
                hh.TenHang, 
                hh.LogoUrl,
                cb.CoTheDoiVe,
                cb.CoTheHoanVe,
                cb.PhiDoiVe,
                cb.PhiHoanVe
            FROM VeMayBay v
            JOIN ChuyenBay cb ON v.IdChuyenBay = cb.IdChuyenBay
            JOIN HangHangKhong hh ON cb.IdHangHangKhong = hh.Id
            WHERE v.TrangThai = 1
            ORDER BY cb.GioKhoiHanh";

                using var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    VeMayBaysNoiDia.Add(new VeMayBayViewModel
                    {
                        MaVe = reader.GetInt32(0),
                        IdChuyenBay = reader.GetInt32(1),
                        HangVe = reader.GetString(2),
                        TongTien = reader.GetDecimal(3),
                        SoLuongVe = reader.GetInt32(4),
                        MaChuyenBay = reader.GetString(5),
                        DiemDi = reader.GetString(6),
                        DiemDen = reader.GetString(7),
                        GioKhoiHanh = reader.GetDateTime(8),
                        GioHaCanh = reader.GetDateTime(9),
                        TenHang = reader.GetString(10),
                        LogoUrl = reader.GetString(11),
                        CoTheDoiVe = reader.GetBoolean(12),
                        CoTheHoanVe = reader.GetBoolean(13),
                        PhiDoiVe = reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                        PhiHoanVe = reader.IsDBNull(15) ? null : reader.GetDecimal(15)
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            // Tạo ViewModel tổng hợp
            var viewModel = new VeMayBayIndexViewModel
            {
                VeMayBaysNoiDia = VeMayBaysNoiDia
            };

            return View(viewModel);
        }
        // Hiển thị chi tiết vé máy bay và danh sách vé tương tự (cùng chuyến bay)
        public IActionResult ChiTietVeMayBay(int maVe, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            VeMayBayViewModel veMayBayHienTai = null;
            List<VeMayBayViewModel> dsVeMayBayTuongTu = new List<VeMayBayViewModel>();

            // Lấy thông tin người dùng từ session
            var tenNguoiDung = HttpContext.Session.GetString("UserName");
            var soDienThoai = HttpContext.Session.GetString("UserPhone");
            var diaChi = HttpContext.Session.GetString("UserAd");
            var maNguoiDung = HttpContext.Session.GetInt32("UserId");

            ViewBag.TenNguoiDung = tenNguoiDung;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.DiaChi = diaChi;
            ViewBag.MaNguoiDung = maNguoiDung;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Truy vấn thông tin vé máy bay hiện tại
                string sql = @"
                    SELECT 
                        v.MaVe, v.IdChuyenBay, v.HangVe, v.GiaNet, v.ThuePhi, (v.GiaNet + v.ThuePhi) AS TongTien,
                        v.HanhLyXachTayKg, v.HanhLyKyGuiKg, v.TrangThai,
                        cb.MaChuyenBay, cb.DiemDi, cb.DiemDen, cb.GioKhoiHanh, cb.GioHaCanh,
                        cb.CoTheDoiVe, cb.CoTheHoanVe, cb.PhiDoiVe, cb.PhiHoanVe,
                        hh.TenHang, hh.LogoUrl, hh.HanhLyXachTayKg AS DefaultHanhLyXachTayKg, hh.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
                    FROM VeMayBay v
                    INNER JOIN ChuyenBay cb ON v.IdChuyenBay = cb.IdChuyenBay
                    INNER JOIN HangHangKhong hh ON cb.IdHangHangKhong = hh.Id
                    WHERE v.MaVe = @maVe";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@maVe", maVe);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            veMayBayHienTai = new VeMayBayViewModel
                            {
                                MaVe = reader.GetInt32(0),
                                IdChuyenBay = reader.GetInt32(1),
                                HangVe = reader.GetString(2),
                                GiaNet = reader.GetDecimal(3),
                                ThuePhi = reader.GetDecimal(4),
                                TongTien = reader.GetDecimal(5),
                                HanhLyXachTayKg = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                HanhLyKyGuiKg = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                TrangThai = reader.GetBoolean(8),
                                MaChuyenBay = reader.GetString(9),
                                DiemDi = reader.GetString(10),
                                DiemDen = reader.GetString(11),
                                GioKhoiHanh = reader.GetDateTime(12),
                                GioHaCanh = reader.GetDateTime(13),
                                CoTheDoiVe = reader.GetBoolean(14),
                                CoTheHoanVe = reader.GetBoolean(15),
                                PhiDoiVe = reader.IsDBNull(16) ? (decimal?)null : reader.GetDecimal(16),
                                PhiHoanVe = reader.IsDBNull(17) ? (decimal?)null : reader.GetDecimal(17),
                                TenHang = reader.GetString(18),
                                LogoUrl = reader.GetString(19),
                                DefaultHanhLyXachTayKg = reader.GetInt32(20),
                                DefaultHanhLyKyGuiKg = reader.GetInt32(21)
                            };
                        }
                    }
                }

                if (veMayBayHienTai != null)
                {
                    // Truy vấn các vé cùng chuyến bay (khác vé hiện tại)
                    string sqlTuongTu = @"
                        SELECT 
                            v.MaVe, v.IdChuyenBay, v.HangVe, v.GiaNet, v.ThuePhi, (v.GiaNet + v.ThuePhi) AS TongTien,
                            v.HanhLyXachTayKg, v.HanhLyKyGuiKg, v.TrangThai,
                            cb.MaChuyenBay, cb.DiemDi, cb.DiemDen, cb.GioKhoiHanh, cb.GioHaCanh,
                            cb.CoTheDoiVe, cb.CoTheHoanVe, cb.PhiDoiVe, cb.PhiHoanVe,
                            hh.TenHang, hh.LogoUrl, hh.HanhLyXachTayKg AS DefaultHanhLyXachTayKg, hh.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
                        FROM VeMayBay v
                        INNER JOIN ChuyenBay cb ON v.IdChuyenBay = cb.IdChuyenBay
                        INNER JOIN HangHangKhong hh ON cb.IdHangHangKhong = hh.Id
                        WHERE v.MaVe <> @maVe AND v.IdChuyenBay = @idChuyenBay";

                    using (var command = new SqlCommand(sqlTuongTu, connection))
                    {
                        command.Parameters.AddWithValue("@maVe", maVe);
                        command.Parameters.AddWithValue("@idChuyenBay", veMayBayHienTai.IdChuyenBay);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsVeMayBayTuongTu.Add(new VeMayBayViewModel
                                {
                                    MaVe = reader.GetInt32(0),
                                    IdChuyenBay = reader.GetInt32(1),
                                    HangVe = reader.GetString(2),
                                    GiaNet = reader.GetDecimal(3),
                                    ThuePhi = reader.GetDecimal(4),
                                    TongTien = reader.GetDecimal(5),
                                    HanhLyXachTayKg = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                    HanhLyKyGuiKg = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                    TrangThai = reader.GetBoolean(8),
                                    MaChuyenBay = reader.GetString(9),
                                    DiemDi = reader.GetString(10),
                                    DiemDen = reader.GetString(11),
                                    GioKhoiHanh = reader.GetDateTime(12),
                                    GioHaCanh = reader.GetDateTime(13),
                                    CoTheDoiVe = reader.GetBoolean(14),
                                    CoTheHoanVe = reader.GetBoolean(15),
                                    PhiDoiVe = reader.IsDBNull(16) ? (decimal?)null : reader.GetDecimal(16),
                                    PhiHoanVe = reader.IsDBNull(17) ? (decimal?)null : reader.GetDecimal(17),
                                    TenHang = reader.GetString(18),
                                    LogoUrl = reader.GetString(19),
                                    DefaultHanhLyXachTayKg = reader.GetInt32(20),
                                    DefaultHanhLyKyGuiKg = reader.GetInt32(21)
                                });
                            }
                        }
                    }
                }
            }

            if (veMayBayHienTai == null)
            {
                return NotFound();
            }

            var pagedTuongTu = dsVeMayBayTuongTu.ToPagedList(pageNumber, pageSize);

            var viewModel = new DetailViewModel
            {
                VeMayBayHienTai = veMayBayHienTai,
                VeMayBayTuongTu = pagedTuongTu
            };

            return View(viewModel);
        }

        public IActionResult DanhMucVeMayBay(int pageNoiDia = 1, int pageQuocTe = 1, int pageSize = 12)
        {
            var model = new VeMayBayTabsViewModel();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string queryNoiDia = @"
                SELECT 
                    v.MaVe, 
                    v.IdChuyenBay, 
                    v.HangVe, 
                    v.GiaNet, 
                    v.ThuePhi, 
                    v.TongTien,
                    v.HanhLyXachTayKg, 
                    v.HanhLyKyGuiKg, 
                    v.TrangThai, 
                    v.SoLuongVe,
                    cb.MaChuyenBay, 
                    cb.DiemDi, 
                    cb.DiemDen, 
                    cb.GioKhoiHanh, 
                    cb.GioHaCanh,
                    cb.CoTheDoiVe, 
                    cb.CoTheHoanVe, 
                    cb.PhiDoiVe, 
                    cb.PhiHoanVe,
                    hh.TenHang, 
                    hh.LogoUrl, 
                    hh.HanhLyXachTayKg AS DefaultHanhLyXachTayKg, 
                    hh.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
                FROM VeMayBay v
                INNER JOIN ChuyenBay cb ON v.IdChuyenBay = cb.IdChuyenBay
                INNER JOIN HangHangKhong hh ON cb.IdHangHangKhong = hh.Id
                WHERE cb.LoaiChuyenBay = 1 AND v.TrangThai = 1 AND v.SoLuongVe > 0";

                    var dsNoiDia = GetVeMayBays(connection, queryNoiDia);
                    model.VeMayBayNoiDia = dsNoiDia.ToPagedList(pageNoiDia, pageSize);

                    // --- Vé máy bay quốc tế (LoạiChuyenBay = 2) ---
                    string queryQuocTe = @"
                SELECT 
                    v.MaVe, 
                    v.IdChuyenBay, 
                    v.HangVe, 
                    v.GiaNet, 
                    v.ThuePhi, 
                    v.TongTien,
                    v.HanhLyXachTayKg, 
                    v.HanhLyKyGuiKg, 
                    v.TrangThai, 
                    v.SoLuongVe,
                    cb.MaChuyenBay, 
                    cb.DiemDi, 
                    cb.DiemDen, 
                    cb.GioKhoiHanh, 
                    cb.GioHaCanh,
                    cb.CoTheDoiVe, 
                    cb.CoTheHoanVe, 
                    cb.PhiDoiVe, 
                    cb.PhiHoanVe,
                    hh.TenHang, 
                    hh.LogoUrl, 
                    hh.HanhLyXachTayKg AS DefaultHanhLyXachTayKg, 
                    hh.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
                FROM VeMayBay v
                INNER JOIN ChuyenBay cb ON v.IdChuyenBay = cb.IdChuyenBay
                INNER JOIN HangHangKhong hh ON cb.IdHangHangKhong = hh.Id
                WHERE cb.LoaiChuyenBay = 2 AND v.TrangThai = 1 AND v.SoLuongVe > 0";

                    var dsQuocTe = GetVeMayBays(connection, queryQuocTe);
                    model.VeMayBayQuocTe = dsQuocTe.ToPagedList(pageQuocTe, pageSize);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            return View(model);
        }

        private List<VeMayBayViewModel> GetVeMayBays(SqlConnection connection, string query)
        {
            var ds = new List<VeMayBayViewModel>();

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ds.Add(new VeMayBayViewModel
                        {
                            MaVe = reader.GetInt32(0),
                            IdChuyenBay = reader.GetInt32(1),
                            HangVe = reader.GetString(2),
                            GiaNet = reader.GetDecimal(3),
                            ThuePhi = reader.GetDecimal(4),
                            TongTien = reader.GetDecimal(5),
                            HanhLyXachTayKg = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                            HanhLyKyGuiKg = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                            TrangThai = reader.GetBoolean(8),
                            SoLuongVe = reader.GetInt32(9),
                            MaChuyenBay = reader.GetString(10),
                            DiemDi = reader.GetString(11),
                            DiemDen = reader.GetString(12),
                            GioKhoiHanh = reader.GetDateTime(13),
                            GioHaCanh = reader.GetDateTime(14),
                            CoTheDoiVe = reader.GetBoolean(15),
                            CoTheHoanVe = reader.GetBoolean(16),
                            PhiDoiVe = reader.IsDBNull(17) ? (decimal?)null : reader.GetDecimal(17),
                            PhiHoanVe = reader.IsDBNull(18) ? (decimal?)null : reader.GetDecimal(18),
                            TenHang = reader.GetString(19),
                            LogoUrl = reader.GetString(20),
                            DefaultHanhLyXachTayKg = reader.GetInt32(21),
                            DefaultHanhLyKyGuiKg = reader.GetInt32(22)
                        });
                    }
                }
            }

            return ds;
        }

        // Tìm kiếm vé máy bay theo từ khóa (theo mã chuyến bay hoặc điểm đi/đến)
        public IActionResult TimKiemVeMayBay(string keyword, int page = 1, int pageSize = 6)
        {
            var dsVe = new List<VeMayBayViewModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT 
                            v.MaVe, v.IdChuyenBay, v.HangVe, v.GiaNet, v.ThuePhi, (v.GiaNet + v.ThuePhi) AS TongTien,
                            v.HanhLyXachTayKg, v.HanhLyKyGuiKg, v.TrangThai,
                            cb.MaChuyenBay, cb.DiemDi, cb.DiemDen, cb.GioKhoiHanh, cb.GioHaCanh,
                            cb.CoTheDoiVe, cb.CoTheHoanVe, cb.PhiDoiVe, cb.PhiHoanVe,
                            hh.TenHang, hh.LogoUrl, hh.HanhLyXachTayKg AS DefaultHanhLyXachTayKg, hh.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
                        FROM VeMayBay v
                        INNER JOIN ChuyenBay cb ON v.IdChuyenBay = cb.IdChuyenBay
                        INNER JOIN HangHangKhong hh ON cb.IdHangHangKhong = hh.Id
                        WHERE cb.MaChuyenBay LIKE @Keyword OR cb.DiemDi LIKE @Keyword OR cb.DiemDen LIKE @Keyword";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsVe.Add(new VeMayBayViewModel
                                {
                                    MaVe = reader.GetInt32(0),
                                    IdChuyenBay = reader.GetInt32(1),
                                    HangVe = reader.GetString(2),
                                    GiaNet = reader.GetDecimal(3),
                                    ThuePhi = reader.GetDecimal(4),
                                    TongTien = reader.GetDecimal(5),
                                    HanhLyXachTayKg = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                    HanhLyKyGuiKg = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                    TrangThai = reader.GetBoolean(8),
                                    MaChuyenBay = reader.GetString(9),
                                    DiemDi = reader.GetString(10),
                                    DiemDen = reader.GetString(11),
                                    GioKhoiHanh = reader.GetDateTime(12),
                                    GioHaCanh = reader.GetDateTime(13),
                                    CoTheDoiVe = reader.GetBoolean(14),
                                    CoTheHoanVe = reader.GetBoolean(15),
                                    PhiDoiVe = reader.IsDBNull(16) ? (decimal?)null : reader.GetDecimal(16),
                                    PhiHoanVe = reader.IsDBNull(17) ? (decimal?)null : reader.GetDecimal(17),
                                    TenHang = reader.GetString(18),
                                    LogoUrl = reader.GetString(19),
                                    DefaultHanhLyXachTayKg = reader.GetInt32(20),
                                    DefaultHanhLyKyGuiKg = reader.GetInt32(21)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi tìm kiếm vé máy bay: {0}", ex.Message);
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            ViewBag.Keyword = keyword;
            var pagedVe = dsVe.ToPagedList(page, pageSize);
            return View(pagedVe);
        }

        // Đặt vé máy bay (booking)
        public IActionResult DatVeMayBay(int maVe, int soLuongVe)
        {
            // Kiểm tra đăng nhập
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                return RedirectToAction("DangNhap", "Home");
            }

            // Kiểm tra số lượng vé hợp lệ
            if (soLuongVe <= 0)
            {
                ModelState.AddModelError("", "Số lượng vé đặt phải lớn hơn 0.");
                return RedirectToAction("ChiTietVeMayBay", new { maVe });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 1. Lấy giá vé và thuế
                    decimal giaNet = 0, thuePhi = 0;
                    string sqlGetGia = "SELECT GiaNet, ThuePhi FROM VeMayBay WHERE MaVe = @maVe";
                    using (var command = new SqlCommand(sqlGetGia, connection))
                    {
                        command.Parameters.AddWithValue("@maVe", maVe);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                giaNet = reader.GetDecimal(0);
                                thuePhi = reader.GetDecimal(1);
                            }
                        }
                    }

                    decimal tongTien = soLuongVe * (giaNet + thuePhi);

                    // 2. Lấy thông tin người dùng từ session
                    var tenNguoiDung = HttpContext.Session.GetString("UserName");
                    var soDienThoai = HttpContext.Session.GetString("UserPhone");
                    var diaChi = HttpContext.Session.GetString("UserAd"); // Kiểm tra lại nếu key chính xác
                    var email = HttpContext.Session.GetString("UserEmail"); // Lấy email nếu có
                    var maNguoiDung = userId; // Đã có từ session

                    // Gán các thông tin vào ViewBag (nếu cần dùng cho view hiển thị lại)
                    ViewBag.TenNguoiDung = tenNguoiDung;
                    ViewBag.SoDienThoai = soDienThoai;
                    ViewBag.DiaChi = diaChi;
                    ViewBag.MaNguoiDung = maNguoiDung;

                    // 3. Insert vào bảng VeMayBayBooking
                    string sqlInsert = @"
INSERT INTO VeMayBayBooking 
(MaVe, MaNguoiDung, HoTenKhachHang, SoDienThoai, DiaChi, Email, NgayDat, SoLuongVe, GiaTaiThoiDiemDat, ThuePhiTaiThoiDiemDat, TongTien, TrangThai)
VALUES 
(@maVe, @MaNguoiDung, @HoTenKhachHang, @SoDienThoai, @DiaChi, @Email, @NgayDat, @SoLuongVe, @GiaTaiThoiDiemDat, @ThuePhiTaiThoiDiemDat, @TongTien, @TrangThai)";

                    using (var command = new SqlCommand(sqlInsert, connection))
                    {
                        command.Parameters.AddWithValue("@maVe", maVe);
                        command.Parameters.AddWithValue("@MaNguoiDung", userId);
                        command.Parameters.AddWithValue("@HoTenKhachHang", tenNguoiDung);
                        command.Parameters.AddWithValue("@SoDienThoai", soDienThoai);
                        command.Parameters.AddWithValue("@DiaChi", diaChi);
                        command.Parameters.AddWithValue("@Email", email ?? "");  // Sử dụng email từ session hoặc chuỗi rỗng nếu null
                        command.Parameters.AddWithValue("@NgayDat", DateTime.Now);
                        command.Parameters.AddWithValue("@SoLuongVe", soLuongVe);
                        command.Parameters.AddWithValue("@GiaTaiThoiDiemDat", giaNet);
                        command.Parameters.AddWithValue("@ThuePhiTaiThoiDiemDat", thuePhi);
                        command.Parameters.AddWithValue("@TongTien", tongTien);
                        command.Parameters.AddWithValue("@TrangThai", "Chờ xác nhận");
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi đặt vé máy bay: {0}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình đặt vé: " + ex.Message;
                return RedirectToAction("ChiTietVeMayBay", new { maVe });
            }

            TempData["SuccessMessage"] = "Đặt vé và thanh toán thành công!";
            return RedirectToAction("ThanhCongView", "VeMayBay");
        }
        public IActionResult ThanhCongView()
        {
            return View();
        }
        // Hiển thị thông tin các vé máy bay mà người dùng đã đặt
        public IActionResult ThongTinVeMayBayDat()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("DangNhap", "Home");

            var dsBooking = new List<VeMayBayBookingViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT 
                Id, MaVe, HoTenKhachHang, SoDienThoai, Email, NgayDat, SoLuongVe, 
                GiaTaiThoiDiemDat, ThuePhiTaiThoiDiemDat, TongTien, TrangThai, DiaChi
            FROM VeMayBayBooking
            WHERE MaNguoiDung = @UserId
            ORDER BY NgayDat DESC
        ";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsBooking.Add(new VeMayBayBookingViewModel
                            {
                                Id = reader.GetInt32(0),
                                MaVe = reader.GetInt32(1),
                                HoTenKhachHang = reader.GetString(2),
                                SoDienThoai = reader.GetString(3),
                                Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                NgayDat = reader.GetDateTime(5),
                                SoLuongVe = reader.GetInt32(6),
                                GiaTaiThoiDiemDat = reader.GetDecimal(7),
                                ThuePhiTaiThoiDiemDat = reader.GetDecimal(8),
                                TongTien = reader.GetDecimal(9),
                                TrangThai = reader.GetString(10),
                                DiaChi = reader.IsDBNull(11) ? "" : reader.GetString(11)
                            });
                        }
                    }
                }
            }

            return View(dsBooking);
        }
    }
}
//