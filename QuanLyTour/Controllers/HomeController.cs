using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTour.Models;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace QuanLyTour.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            // Kiểm tra thông tin người dùng từ Session
            var tenNguoiDung = HttpContext.Session.GetString("TenNguoiDung");
            ViewBag.TenNguoiDung = tenNguoiDung;

            var toursTrongNuoc = new List<TourViewModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string queryTrongNuoc = @"
                SELECT MaTour, TenTour, MaLoaiTour, TrangThai, GiaTour, HinhAnh1 
                FROM Tour 
                WHERE TrangThai = 1";

                    using (var command = new SqlCommand(queryTrongNuoc, connection))
                    {
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toursTrongNuoc.Add(new TourViewModel
                                {
                                    MaTour = reader.GetInt32(0),
                                    TenTour = reader.GetString(1),
                                    MaLoaiTour = reader.GetInt32(2),
                                    TrangThai = reader.GetBoolean(3),
                                    GiaTour = reader.GetDecimal(4),
                                    HinhAnh1 = reader.GetString(5)
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
            var viewModel = new TourIndexViewModel
            {
                ToursTrongNuoc = toursTrongNuoc,
            };

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult DangNhap()
        {
            ViewData["Title"] = "Đăng nhập";
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Mở kết nối
                conn.Open();

                // Viết truy vấn SQL
                string sql = "SELECT * FROM NguoiDung WHERE TenDangNhap = @username AND MatKhau = @password AND LoaiTaiKhoan = 0 AND TrangThai = 1";

                // Tạo SqlCommand
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Gắn tham số để tránh SQL Injection
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    // Thực thi truy vấn
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // Nếu có kết quả
                        {
                            // Lấy thông tin người dùng từ kết quả truy vấn
                            string tenNguoiDung = reader["TenNguoiDung"].ToString();
                            int maNguoiDung = Convert.ToInt32(reader["MaNguoiDung"]);
                            string soDienThoai = reader["SoDienthoai"].ToString();
                            string diaChi = reader["DiaChi"].ToString();

                            // Lưu thông tin vào session
                            HttpContext.Session.SetString("UserName", tenNguoiDung);
                            HttpContext.Session.SetString("UserPhone",soDienThoai);
                            HttpContext.Session.SetString("UserAd", diaChi);
                            HttpContext.Session.SetInt32("UserId", maNguoiDung);

                            // Chuyển hướng đến trang chính
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Thông báo lỗi nếu thông tin đăng nhập không chính xác
                            TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng!";
                            return RedirectToAction("DangNhap", "Home");
                        }
                    }
                }
            }
        }
        public IActionResult DangXuat()
        {
            // Xóa tất cả thông tin trong session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");  // Quay lại trang chủ
        }
        public IActionResult ThongTinNguoiDung()
        {
            ViewData["Title"] = "Thông tin người dùng";
            // Lấy ID từ session
            var maNguoiDung = HttpContext.Session.GetInt32("UserId");
            if (maNguoiDung == null)
            {
                return RedirectToAction("DangNhap", "Home"); // Chuyển hướng nếu chưa đăng nhập
            }

            // Kết nối database và lấy thông tin người dùng
            ThongTinNguoiDung nguoiDung = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT MaNguoiDung, TenNguoiDung, DiaChi, SoDienThoai " +
                               "FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    nguoiDung = new ThongTinNguoiDung
                    {
                        MaNguoiDung = Convert.ToInt32(reader["MaNguoiDung"]),
                        TenNguoiDung = reader["TenNguoiDung"].ToString(),
                        DiaChi = reader["DiaChi"].ToString(),
                        SoDienThoai = reader["SoDienThoai"].ToString(),
                    };
                }
                connection.Close();
            }

            if (nguoiDung == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            return View(nguoiDung); // Truyền dữ liệu tới view
        }

        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ThongTinNguoiDung(string TenNguoiDung, string DiaChi, string SoDienThoai)
        {
            // Lấy maNguoiDung từ session
            var maNguoiDung = HttpContext.Session.GetInt32("UserId");

            // Kiểm tra xem maNguoiDung có hợp lệ không
            if (maNguoiDung == null)
            {
                TempData["ErrorMessage"] = "Bạn cần phải đăng nhập để cập nhật thông tin!";
                return RedirectToAction("DangNhap", "Home");  // Chuyển hướng đến trang login nếu không có maNguoiDung
            }

            try
            {
                // Cập nhật thông tin người dùng vào cơ sở dữ liệu
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE NguoiDung SET TenNguoiDung = @TenNguoiDung, DiaChi = @DiaChi, SoDienThoai = @SoDienThoai WHERE MaNguoiDung = @MaNguoiDung";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);  // Sử dụng maNguoiDung lấy từ session
                    command.Parameters.AddWithValue("@TenNguoiDung", TenNguoiDung);  // Lấy giá trị từ view
                    command.Parameters.AddWithValue("@DiaChi", DiaChi);  // Lấy giá trị từ view
                    command.Parameters.AddWithValue("@SoDienThoai", SoDienThoai);  // Lấy giá trị từ view

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                // Thông báo thành công và chuyển hướng
                TempData["Message"] = "Cập nhật thông tin thành công! Sau 5s hệ thống sẽ tự đăng xuất tài khoản, bạn cần đăng nhập lại để xem thông tin cập nhật!";
                return RedirectToAction("ThongTinNguoiDung");
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra, ghi lại lỗi và hiển thị thông báo lỗi
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return View();  // Nếu có lỗi, quay lại trang cập nhật
            }
        }


        // Hiển thị form đăng ký
        public IActionResult DangKy()
        {
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        public IActionResult DangKy(string username, string password, string HoTen, string DiaChi, string SoDienthoai)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Kiểm tra nếu tên đăng nhập đã tồn tại
                    string checkUserQuery = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @TenDangNhap";
                    using (var checkCommand = new SqlCommand(checkUserQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@TenDangNhap", username);
                        int userCount = (int)checkCommand.ExecuteScalar();

                        if (userCount > 0)
                        {
                            TempData["ErrorMessage"] = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                            return RedirectToAction("DangKy");
                        }
                    }

                    // Thêm người dùng mới vào cơ sở dữ liệu
                    string insertQuery = @"
                    INSERT INTO NguoiDung (TenDangNhap, MatKhau, TenNguoiDung, DiaChi, SoDienThoai, TrangThai, LoaiTaiKhoan)
                    VALUES (@TenDangNhap, @MatKhau, @TenNguoiDung, @DiaChi, @SoDienThoai, @TrangThai, @LoaiTaiKhoan)";
                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@TenDangNhap", username);
                        insertCommand.Parameters.AddWithValue("@MatKhau", password); // Nên mã hóa mật khẩu trước khi lưu
                        insertCommand.Parameters.AddWithValue("@TenNguoiDung", HoTen);
                        insertCommand.Parameters.AddWithValue("@DiaChi", DiaChi);
                        insertCommand.Parameters.AddWithValue("@SoDienThoai", SoDienthoai);
                        insertCommand.Parameters.AddWithValue("@TrangThai", true); // Mặc định kích hoạt
                        insertCommand.Parameters.AddWithValue("@LoaiTaiKhoan", false); // Mặc định tài khoản thường

                        insertCommand.ExecuteNonQuery();
                    }
                }

                // Đặt thông báo thành công và tự động chuyển hướng sau 3 giây
                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn sẽ được chuyển về trang chủ sau 5 giây.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("DangKy");
        }
        public IActionResult TimKiemTour(string keyword, int page = 1, int pageSize = 6)
        {
            var tours = new List<TourViewModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Truy vấn dữ liệu Tour dựa vào từ khóa
                    string query = @"
                SELECT t.MaTour, t.TenTour, t.MaLoaiTour, t.TrangThai, t.GiaTour, t.HinhAnh1, l.TenLoaiTour
                FROM Tour t
                INNER JOIN LoaiTour l ON t.MaLoaiTour = l.MaLoaiTour
                WHERE (TenTour LIKE @Keyword)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tours.Add(new TourViewModel
                                {
                                    MaTour = reader.GetInt32(0),
                                    TenTour = reader.GetString(1),
                                    MaLoaiTour = reader.GetInt32(2),
                                    TrangThai = reader.GetBoolean(3),
                                    GiaTour = reader.GetDecimal(4),
                                    HinhAnh1 = reader.GetString(5),
                                    TenLoaiTour = reader.GetString(6)
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
            var pagedTours = tours.ToPagedList(page, pageSize);

            // Gửi từ khóa và danh sách Tour tìm được về View
            ViewBag.Keyword = keyword;
            return View(pagedTours);
        }
        public IActionResult ThongTinTourDat()
        {
            int maNguoiDung = (int)HttpContext.Session.GetInt32("UserId"); // Lấy mã người dùng từ session

            if (maNguoiDung == 0)
            {
                return RedirectToAction("DangNhap", "Home"); // Nếu chưa đăng nhập, chuyển tới trang đăng nhập
            }

            List<TourDatViewModel> danhSachTourDat = new List<TourDatViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT 
                t.TenTour,
                l.TenLoaiTour,
                p.NgayDatTour,
                p.SoLuong,
                (p.SoLuong * t.GiaTour) AS ThanhTien,
                h.NgayThanhToan,
                p.NgayDi
            FROM PhieuDatTour p
            INNER JOIN Tour t ON p.MaTour = t.MaTour
            INNER JOIN LoaiTour l ON t.MaLoaiTour = l.MaLoaiTour
            LEFT JOIN HoaDon h ON p.MaPhieu = h.MaPhieu
            WHERE p.MaNguoiDung = @MaNguoiDung
            ORDER BY p.NgayDatTour DESC";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachTourDat.Add(new TourDatViewModel
                            {
                                TenTour = reader.GetString(0),
                                LoaiTour = reader.GetString(1),
                                NgayDat = reader.GetDateTime(2),
                                SoLuong = reader.GetInt32(3),
                                ThanhTien = reader.GetDecimal(4),
                                NgayThanhToan = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                NgayDi = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            return View(danhSachTourDat);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
