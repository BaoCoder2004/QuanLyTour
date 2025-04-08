using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTour.Models;
using QuanLyTour.Models.VeTau;
using X.PagedList;
using X.PagedList.Extensions;

namespace QuanLyTour.Controllers
{
    public class VeTauController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<VeTauController> _logger;

        public VeTauController(ILogger<VeTauController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            // Kiểm tra thông tin người dùng từ Session
            var tenNguoiDung = HttpContext.Session.GetString("UserName");
            ViewBag.TenNguoiDung = tenNguoiDung;

            var veTaus = new List<VeMayBayViewModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                    SELECT ChuyenTauID, TenTau, GaDi, GaDen, NgayDi, NgayDen, SoLuongVe, GiaVe 
                    FROM ThongTinChuyenTau 
                    WHERE NgayDi > GETDATE()";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                veTaus.Add(new VeMayBayViewModel
                                {
                                    ChuyenTauID = reader.GetInt32(0),
                                    TenTau = reader.GetString(1),
                                    GaDi = reader.GetString(2),
                                    GaDen = reader.GetString(3),
                                    NgayDi = reader.GetDateTime(4),
                                    NgayDen = reader.GetDateTime(5),
                                    GiaVe = reader.GetDecimal(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            // Tạo ViewModel tổng hợp
            var viewModel = new VeMayBaySearchViewModel
            {
                VeTaus = veTaus.ToPagedList(1, 10)
            };

            return View(viewModel);
        }

        public IActionResult ChiTietVeTau(int chuyenTauID, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            VeMayBayViewModel chuyenTau = null;
            List<VeMayBayViewModel> chuyenTauTuongTu = new List<VeMayBayViewModel>();

            // Lấy thông tin người dùng từ session
            var tenNguoiDung = HttpContext.Session.GetString("UserName");
            var soDienThoai = HttpContext.Session.GetString("UserPhone");
            var email = HttpContext.Session.GetString("UserEmail");
            var maNguoiDung = HttpContext.Session.GetInt32("UserId");

            ViewBag.TenNguoiDung = tenNguoiDung;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.Email = email;
            ViewBag.MaNguoiDung = maNguoiDung;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Truy vấn thông tin chuyến tàu hiện tại
                string sql = "SELECT ChuyenTauID, TenTau, GaDi, GaDen, NgayDi, NgayDen, SoLuongVe, GiaVe " +
                             "FROM ThongTinChuyenTau " +
                             "WHERE ChuyenTauID = @ChuyenTauID";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ChuyenTauID", chuyenTauID);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            chuyenTau = new VeMayBayViewModel
                            {
                                ChuyenTauID = !reader.IsDBNull(0) ? reader.GetInt32(0) : 0,
                                TenTau = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                                GaDi = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                                GaDen = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                                NgayDi = !reader.IsDBNull(4) ? reader.GetDateTime(4) : DateTime.MinValue,
                                NgayDen = !reader.IsDBNull(5) ? reader.GetDateTime(5) : DateTime.MinValue,
                                GiaVe = !reader.IsDBNull(7) ? reader.GetDecimal(7) : 0
                            };
                        }
                    }
                }

                // Truy vấn các chuyến tàu tương tự (cùng ga đi và ga đến)
                string sqlTuongTu = "SELECT ChuyenTauID, TenTau, GaDi, GaDen, NgayDi, NgayDen, SoLuongVe, GiaVe " +
                                   "FROM ThongTinChuyenTau " +
                                   "WHERE ChuyenTauID != @ChuyenTauID AND GaDi = @GaDi AND GaDen = @GaDen";

                using (var command = new SqlCommand(sqlTuongTu, connection))
                {
                    command.Parameters.AddWithValue("@ChuyenTauID", chuyenTauID);
                    command.Parameters.AddWithValue("@GaDi", chuyenTau?.GaDi ?? string.Empty);
                    command.Parameters.AddWithValue("@GaDen", chuyenTau?.GaDen ?? string.Empty);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chuyenTauTuongTu.Add(new VeMayBayViewModel
                            {
                                ChuyenTauID = reader.GetInt32(0),
                                TenTau = reader.GetString(1),
                                GaDi = reader.GetString(2),
                                GaDen = reader.GetString(3),
                                NgayDi = reader.GetDateTime(4),
                                NgayDen = reader.GetDateTime(5),
                                GiaVe = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }

            if (chuyenTau == null)
            {
                return NotFound();
            }

            return View();
        }      

        // Hàm lấy danh sách vé tàu
        private List<VeMayBayViewModel> GetVeTau(SqlConnection connection, string query)
        {
            var veTauList = new List<VeMayBayViewModel>();

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        veTauList.Add(new VeMayBayViewModel
                        {
                            ChuyenTauID = reader.GetInt32(0),
                            TenTau = reader.GetString(1),
                            GaDi = reader.GetString(2),
                            GaDen = reader.GetString(3),
                            NgayDi = reader.GetDateTime(4),
                            NgayDen = reader.GetDateTime(5),
                            GiaVe = reader.GetDecimal(7)
                        });
                    }
                }
            }
            return veTauList;
        }

        [HttpPost]
        public IActionResult DatVeTau(int MaNguoiDung, string HoTen, string SoDienThoaiLienHe, string EmailLienHe, int ChuyenTauID, string HinhThucThanhToan)
        {
            if (string.IsNullOrWhiteSpace(SoDienThoaiLienHe) || string.IsNullOrWhiteSpace(EmailLienHe))
            {
                ModelState.AddModelError("", "Thông tin liên hệ không được để trống.");
                return RedirectToAction("ChiTietChuyenTau", new { chuyenTauID = ChuyenTauID });
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Tạo mã đặt vé
                string maDatVe = $"VT{DateTime.Now:yyyyMMddHHmmss}{MaNguoiDung}";

                // Thêm thông tin đặt vé vào bảng DatVeTau
                string sqlInsertDatVe = @"
                INSERT INTO DatVeTau (MaNguoiDung, ChuyenTauID, MaDatVe, NgayDatVe, HinhThucThanhToan, 
                                      TrangThaiThanhToan, TrangThaiVe, NgayHetHanVe, SoDienThoaiLienHe, EmailLienHe) 
                VALUES (@MaNguoiDung, @ChuyenTauID, @MaDatVe, @NgayDatVe, @HinhThucThanhToan, 
                        @TrangThaiThanhToan, @TrangThaiVe, @NgayHetHanVe, @SoDienThoaiLienHe, @EmailLienHe); 
                SELECT SCOPE_IDENTITY();";

                int datVeID;

                using (var command = new SqlCommand(sqlInsertDatVe, connection))
                {
                    command.Parameters.AddWithValue("@MaNguoiDung", MaNguoiDung);
                    command.Parameters.AddWithValue("@ChuyenTauID", ChuyenTauID);
                    command.Parameters.AddWithValue("@MaDatVe", maDatVe);
                    command.Parameters.AddWithValue("@NgayDatVe", DateTime.Now);
                    command.Parameters.AddWithValue("@HinhThucThanhToan", HinhThucThanhToan);
                    command.Parameters.AddWithValue("@TrangThaiThanhToan", "Đã thanh toán");
                    command.Parameters.AddWithValue("@TrangThaiVe", "Đã đặt");
                    command.Parameters.AddWithValue("@NgayHetHanVe", DateTime.Now.AddDays(30));
                    command.Parameters.AddWithValue("@SoDienThoaiLienHe", SoDienThoaiLienHe);
                    command.Parameters.AddWithValue("@EmailLienHe", EmailLienHe);

                    datVeID = Convert.ToInt32(command.ExecuteScalar());
                }

                // Cập nhật số lượng vé còn lại
                string sqlUpdateVeTau = "UPDATE ThongTinChuyenTau SET SoLuongVe = SoLuongVe - 1 WHERE ChuyenTauID = @ChuyenTauID";
                using (var command = new SqlCommand(sqlUpdateVeTau, connection))
                {
                    command.Parameters.AddWithValue("@ChuyenTauID", ChuyenTauID);
                    command.ExecuteNonQuery();
                }

                // Tính tổng tiền
                decimal giaVe = 0;
                string sqlGetGiaVe = "SELECT GiaVe FROM ThongTinChuyenTau WHERE ChuyenTauID = @ChuyenTauID";
                using (var command = new SqlCommand(sqlGetGiaVe, connection))
                {
                    command.Parameters.AddWithValue("@ChuyenTauID", ChuyenTauID);
                    giaVe = Convert.ToDecimal(command.ExecuteScalar());
                }

                // Thêm hóa đơn
                string sqlInsertHoaDon = @"
                INSERT INTO HoaDonVeTau (MaNguoiDung, ChuyenTauID, TenNguoiDung, NgayDatVe, TongTien, NgayThanhToan)
                VALUES (@MaNguoiDung, @ChuyenTauID, @TenNguoiDung, @NgayDatVe, @TongTien, @NgayThanhToan)";

                using (var command = new SqlCommand(sqlInsertHoaDon, connection))
                {
                    command.Parameters.AddWithValue("@MaNguoiDung", MaNguoiDung);
                    command.Parameters.AddWithValue("@ChuyenTauID", ChuyenTauID);
                    command.Parameters.AddWithValue("@TenNguoiDung", HoTen);
                    command.Parameters.AddWithValue("@NgayDatVe", DateTime.Now);
                    command.Parameters.AddWithValue("@TongTien", giaVe);
                    command.Parameters.AddWithValue("@NgayThanhToan", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Đặt vé tàu và thanh toán thành công!";
            return RedirectToAction("ThanhCongView", "VeTau");
        }

        public IActionResult TimKiemVeTau(string keyword, int page = 1, int pageSize = 6)
        {
            var veTaus = new List<VeMayBayViewModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Truy vấn dữ liệu vé tàu dựa vào từ khóa
                    string query = @"
                    SELECT ChuyenTauID, TenTau, GaDi, GaDen, NgayDi, NgayDen, SoLuongVe, GiaVe
                    FROM ThongTinChuyenTau
                    WHERE (TenTau LIKE @Keyword OR GaDi LIKE @Keyword OR GaDen LIKE @Keyword)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                veTaus.Add(new VeMayBayViewModel
                                {
                                    ChuyenTauID = reader.GetInt32(0),
                                    TenTau = reader.GetString(1),
                                    GaDi = reader.GetString(2),
                                    GaDen = reader.GetString(3),
                                    NgayDi = reader.GetDateTime(4),
                                    NgayDen = reader.GetDateTime(5),
                                    GiaVe = reader.GetDecimal(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
            }

            // Phân trang kết quả tìm kiếm
            var pagedVeTaus = veTaus.ToPagedList(page, pageSize);

            // Gửi từ khóa và danh sách vé tàu tìm được về View
            ViewBag.Keyword = keyword;
            return View(pagedVeTaus);
        }

        public IActionResult ThanhCongView()
        {
            return View();
        }

        public IActionResult ThongTinVeTauDat()
        {
            int maNguoiDung = HttpContext.Session.GetInt32("UserId") ?? 0; // Lấy mã người dùng từ session

            if (maNguoiDung == 0)
            {
                return RedirectToAction("DangNhap", "Home"); // Nếu chưa đăng nhập, chuyển tới trang đăng nhập
            }

            List<HoaDonVeTau> danhSachVeDat = new List<HoaDonVeTau>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                SELECT hd.MaHoaDon, hd.MaNguoiDung, hd.ChuyenTauID, ct.TenTau, 
                       hd.TenNguoiDung, hd.NgayDatVe, hd.TongTien, hd.NgayThanhToan
                FROM HoaDonVeTau hd
                JOIN ThongTinChuyenTau ct ON hd.ChuyenTauID = ct.ChuyenTauID
                WHERE hd.MaNguoiDung = @MaNguoiDung";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachVeDat.Add(new HoaDonVeTau
                            {
                                MaHoaDon = reader.GetInt32(0),
                                MaNguoiDung = reader.GetInt32(1),
                                ChuyenTauID = reader.GetInt32(2),
                                TenNguoiDung = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                NgayDatVe = reader.GetDateTime(5),
                                TongTien = reader.GetDecimal(6),
                                NgayThanhToan = reader.GetDateTime(7)
                            });
                        }
                    }
                }
            }
            return View(danhSachVeDat);
        }
    }
}