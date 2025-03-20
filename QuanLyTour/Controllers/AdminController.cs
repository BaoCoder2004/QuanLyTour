using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using X.PagedList;
using QuanLyTour.Models;
using X.PagedList.Extensions;
using QuanLyTour.Models.Tour;
using QuanLyTour.Models.KhachSan;

namespace QuanLyTour.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Mở kết nối
                conn.Open();

                // Viết truy vấn SQL
                string sql = "SELECT * FROM NguoiDung WHERE TenDangNhap = @username AND MatKhau = @password AND LoaiTaiKhoan = 1 AND TrangThai = 1";

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
                            string tenAdmin = reader["TenNguoiDung"].ToString();
                            int maAdmin = Convert.ToInt32(reader["MaNguoiDung"]);

                            HttpContext.Session.SetString("UserName", tenAdmin);
                            HttpContext.Session.SetInt32("UserId", maAdmin);

                            // Chuyển hướng đến trang chính
                            return RedirectToAction("QuanLyPhieuView", "Admin");
                        }
                        else
                        {
                            // Thông báo lỗi nếu thông tin đăng nhập không chính xác
                            TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng!";
                            return RedirectToAction("Index", "Admin");
                        }
                    }
                }
            }
        }
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Admin");  // Quay lại trang chủ
        }
        public IActionResult QuanLyPhieuView(int? page)
        {
            int pageSize = 10; // Số lượng mục trên mỗi trang
            int pageNumber = (page ?? 1);

            List<PhieuDatTour> phieuTour = new List<PhieuDatTour>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT KH.TenNguoiDung, KH.SoDienThoai, KH.DiaChi, P.TenTour, PDP.NgayDatTour, P.GiaTour, PDP.SoLuong, PDP.MaPhieu, KH.MaNguoiDung, P.MaTour, PDP.NgayDi
                           FROM PhieuDatTour PDP
                           INNER JOIN Tour P ON PDP.MaTour = P.MaTour
                           INNER JOIN NguoiDung KH ON PDP.MaNguoiDung = KH.MaNguoiDung";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            phieuTour.Add(new PhieuDatTour
                            {
                                TenNguoiDung = reader.GetString(0),
                                SoDienThoai = reader.GetString(1),
                                DiaChi = reader.GetString(2),
                                TenTour = reader.GetString(3),
                                NgayDatTour = reader.GetDateTime(4),
                                GiaTour = reader.GetDecimal(5),
                                SoLuong = reader.GetInt32(6),                               
                                MaPhieu = reader.GetInt32(7),
                                MaNguoiDung = reader.GetInt32(8),
                                MaTour = reader.GetInt32(9),
                                NgayDi = reader.GetDateTime(10),
                            });
                        }
                    }
                }
            }

            var pagedTour = phieuTour.ToPagedList(pageNumber, pageSize);
            return View(pagedTour);
        }
        public IActionResult QuanLyHoaDonView(int? page)
        {
            int pageSize = 10; // Số lượng mục trên mỗi trang
            int pageNumber = (page ?? 1);

            List<HoaDon> phieuTour = new List<HoaDon>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT KH.TenNguoiDung, P.TenTour, PDP.NgayDatTour, HD.TongTien, HD.NgayThanhToan, HD.MaHoaDon
                           FROM HoaDon HD
                           INNER JOIN PhieuDatTour PDP ON PDP.MaPhieu = HD.maPhieu
                           INNER JOIN Tour P ON PDP.MaTour = P.MaTour
                           INNER JOIN NguoiDung KH ON PDP.MaNguoiDung = KH.MaNguoiDung";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            phieuTour.Add(new HoaDon
                            {
                                TenNguoiDung = reader.GetString(0),
                                TenTour = reader.GetString(1),
                                NgayDatTour = reader.GetDateTime(2),
                                TongTien = reader.GetDecimal(3),
                                NgayThanhToan = reader.GetDateTime(4),
                                MaHoaDon = reader.GetInt32(5),

                            });
                        }
                    }
                }
            }

            var pagedRooms = phieuTour.ToPagedList(pageNumber, pageSize);
            return View(pagedRooms);
        }

		#region Tour
		public IActionResult QuanLyTourView(int? page)
		{
			int pageSize = 6; // Số lượng mục trên mỗi trang
			int pageNumber = (page ?? 1);

			List<TourViewModel> tour = new List<TourViewModel>();

			using (var connection = new SqlConnection(_connectionString))
			{
				string sql = "SELECT p.MaTour, p.TenTour, l.TenLoaiTour,p.SoNgay, p.DiaDiem, p.GiaTour, p.HinhAnh1, p.HinhAnh2, p.HinhAnh3, p.MaLoaiTour, p.MoTa, p.LichTrinh " +
							 "FROM Tour p " +
							 "INNER JOIN LoaiTour l ON p.MaLoaiTour = l.MaLoaiTour";

				using (var command = new SqlCommand(sql, connection))
				{
					connection.Open();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							tour.Add(new TourViewModel
							{
								MaTour = reader.GetInt32(reader.GetOrdinal("MaTour")),
								TenTour = !reader.IsDBNull(reader.GetOrdinal("TenTour")) ? reader.GetString(reader.GetOrdinal("TenTour")) : string.Empty,
								TenLoaiTour = !reader.IsDBNull(reader.GetOrdinal("TenLoaiTour")) ? reader.GetString(reader.GetOrdinal("TenLoaiTour")) : string.Empty,
								GiaTour = !reader.IsDBNull(reader.GetOrdinal("GiaTour")) ? reader.GetDecimal(reader.GetOrdinal("GiaTour")) : 0,
								SoNgay = !reader.IsDBNull(reader.GetOrdinal("SoNgay")) ? reader.GetString(reader.GetOrdinal("SoNgay")) : string.Empty,
								DiaDiem = !reader.IsDBNull(reader.GetOrdinal("DiaDiem")) ? reader.GetString(reader.GetOrdinal("DiaDiem")) : string.Empty,
								HinhAnh1 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh1")) ? reader.GetString(reader.GetOrdinal("HinhAnh1")) : string.Empty,
								HinhAnh2 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh2")) ? reader.GetString(reader.GetOrdinal("HinhAnh2")) : string.Empty,
								HinhAnh3 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh3")) ? reader.GetString(reader.GetOrdinal("HinhAnh3")) : string.Empty,
								MaLoaiTour = !reader.IsDBNull(reader.GetOrdinal("MaLoaiTour")) ? reader.GetInt32(reader.GetOrdinal("MaLoaiTour")) : 0,
								MoTa = !reader.IsDBNull(reader.GetOrdinal("MoTa")) ? reader.GetString(reader.GetOrdinal("MoTa")) : string.Empty,
								LichTrinh = !reader.IsDBNull(reader.GetOrdinal("LichTrinh")) ? reader.GetString(reader.GetOrdinal("LichTrinh")) : string.Empty,
							});
						}
					}
				}
			}

			var pagedTours = tour.ToPagedList(pageNumber, pageSize);
			return View(pagedTours);
		}

		public IActionResult XoaTourView(int maTour)
		{
			bool phongDaSuDung = false;
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "SELECT COUNT(*) FROM PhieuDatTour WHERE MaTour = @maTour";
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@maTour", maTour);
					int count = (int)command.ExecuteScalar();
					phongDaSuDung = (count > 0);
				}
			}

			// Nếu phòng không tồn tại trong bảng PhieuDatPhong, thực hiện xóa phòng
			if (!phongDaSuDung)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "DELETE FROM Tour WHERE MaTour = @maTour";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maTour", maTour);
						command.ExecuteNonQuery();
					}
				}
				TempData["Sucess"] = "Xóa thành công";
				return RedirectToAction("QuanLyTourView"); // hoặc trang khác tùy ý
			}
			else
			{
				TempData["ErrorMessage"] = "Tour du lịch đã sử dụng, không thể xóa";
				return RedirectToAction("QuanLyTourView");
			}
		}
		[HttpGet]
		public IActionResult SuaTourView(int maTour, int maLoai)
		{
			List<LoaiTour> tourTypes = new List<LoaiTour>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "SELECT MaLoaiTour, TenLoaiTour FROM LoaiTour";
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							tourTypes.Add(new LoaiTour
							{
								MaLoaiTour = reader.GetInt32(0),
								TenLoaiTour = reader.GetString(1)
							});
						}
					}
				}
			}

			List<TourViewModel> tour = new List<TourViewModel>();
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string rooms = "SELECT p.MaTour, p.TenTour, p.MaLoaiTour, p.TrangThai,p.SoNgay, p.DiaDiem, p.GiaTour, p.HinhAnh1, p.HinhAnh2, p.HinhAnh3, l.TenLoaiTour, p.MoTa, p.LichTrinh " +
				   "FROM Tour p " +
				   "INNER JOIN LoaiTour l ON p.MaLoaiTour = l.MaLoaiTour " +
				   "WHERE p.MaTour = @maTour And p.MaLoaiTour = @maLoai";
				using (var command = new SqlCommand(rooms, connection))
				{
					command.Parameters.AddWithValue("@maTour", maTour);
					command.Parameters.AddWithValue("@maLoai", maLoai);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							tour.Add(new TourViewModel
							{
								MaTour = reader.GetInt32(reader.GetOrdinal("MaTour")),
								TenTour = !reader.IsDBNull(reader.GetOrdinal("TenTour")) ? reader.GetString(reader.GetOrdinal("TenTour")) : string.Empty,
								TenLoaiTour = !reader.IsDBNull(reader.GetOrdinal("TenLoaiTour")) ? reader.GetString(reader.GetOrdinal("TenLoaiTour")) : string.Empty,
								SoNgay = !reader.IsDBNull(reader.GetOrdinal("SoNgay")) ? reader.GetString(reader.GetOrdinal("SoNgay")) : string.Empty,
								DiaDiem = !reader.IsDBNull(reader.GetOrdinal("DiaDiem")) ? reader.GetString(reader.GetOrdinal("DiaDiem")) : string.Empty,
								GiaTour = !reader.IsDBNull(reader.GetOrdinal("GiaTour")) ? reader.GetDecimal(reader.GetOrdinal("GiaTour")) : 0,
								HinhAnh1 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh1")) ? reader.GetString(reader.GetOrdinal("HinhAnh1")) : string.Empty,
								HinhAnh2 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh2")) ? reader.GetString(reader.GetOrdinal("HinhAnh2")) : string.Empty,
								HinhAnh3 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh3")) ? reader.GetString(reader.GetOrdinal("HinhAnh3")) : string.Empty,
								MaLoaiTour = !reader.IsDBNull(reader.GetOrdinal("MaLoaiTour")) ? reader.GetInt32(reader.GetOrdinal("MaLoaiTour")) : 0,
								TrangThai = !reader.IsDBNull(reader.GetOrdinal("TrangThai")) && reader.GetBoolean(reader.GetOrdinal("TrangThai")),
								MoTa = !reader.IsDBNull(reader.GetOrdinal("MoTa")) ? reader.GetString(reader.GetOrdinal("MoTa")) : string.Empty,
								LichTrinh = !reader.IsDBNull(reader.GetOrdinal("LichTrinh")) ? reader.GetString(reader.GetOrdinal("LichTrinh")) : string.Empty,
							});
						}
					}
				}

			}
			ViewModel viewModel = new ViewModel
			{
				LoaiTourView = tourTypes,
				TourView = tour
			};
			return View(viewModel);
		}
		[HttpPost]
		public IActionResult SuaTourView(int maTour, string tenTour, string soNgay, string diaDiem, decimal giaTour, int maLoai, IFormFile hinhAnh1, IFormFile hinhAnh2, IFormFile hinhAnh3, int trangThai, string moTa, string lichTrinh)
		{
			string hinhAnh1Path = SaveFile(hinhAnh1);
			string hinhAnh2Path = SaveFile(hinhAnh2);
			string hinhAnh3Path = SaveFile(hinhAnh3);
			if (hinhAnh1Path == null && hinhAnh2Path == null && hinhAnh3Path == null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE Tour SET MaLoaiTour = @maLoai, TenTour = @tenTour,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaTour = @giaTour, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								 "WHERE MaTour = @maTour";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maTour", maTour);
						command.Parameters.AddWithValue("@tenTour", tenTour);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaTour", giaTour);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else if (hinhAnh1Path != null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE Tour SET MaLoaiTour = @maLoai, TenTour = @tenTour,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaTour = @giaTour, HinhAnh1 = @hinhAnh1Path, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								"WHERE MaTour = @maTour";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maTour", maTour);
						command.Parameters.AddWithValue("@tenTour", tenTour);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaTour", giaTour);
						command.Parameters.AddWithValue("@hinhAnh1Path", hinhAnh1Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else if (hinhAnh2Path != null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE Tour SET MaLoaiTour = @maLoai, TenTour = @tenTour,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaTour = @giaTour, HinhAnh2 = @hinhAnh2Path, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
							   "WHERE MaTour = @maTour";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maTour", maTour);
						command.Parameters.AddWithValue("@tenTour", tenTour);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaTour", giaTour);
						command.Parameters.AddWithValue("@hinhAnh2Path", hinhAnh2Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else if (hinhAnh3Path != null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE Tour SET MaLoaiTour = @maLoai, TenTour = @tenTour,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaTour = @giaTour, HinhAnh3 = @hinhAnh3Path, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								"WHERE MaTour = @maTour";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maTour", maTour);
						command.Parameters.AddWithValue("@tenTour", tenTour);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaTour", giaTour);
						command.Parameters.AddWithValue("@hinhAnh3Path", hinhAnh3Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE Tour SET MaLoaiTour = @maLoai, TenTour = @tenTour,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaTour = @giaTour, HinhAnh1 = @hinhAnh1Path,HinhAnh2 = @hinhAnh2Path,HinhAnh3 = @hinhAnh3Path,TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								"WHERE MaTour = @maTour";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maTour", maTour);
						command.Parameters.AddWithValue("@tenTour", tenTour);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaTour", giaTour);
						command.Parameters.AddWithValue("@hinhAnh1Path", hinhAnh1Path);
						command.Parameters.AddWithValue("@hinhAnh2Path", hinhAnh2Path);
						command.Parameters.AddWithValue("@hinhAnh3Path", hinhAnh3Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			return RedirectToAction("QuanLyTourView");
		}
		[HttpGet]
		public IActionResult ThemTourView()
		{
			List<LoaiTour> tourTypes = new List<LoaiTour>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "SELECT MaLoaiTour, TenLoaiTour FROM LoaiTour";
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							tourTypes.Add(new LoaiTour
							{
								MaLoaiTour = reader.GetInt32(0),
								TenLoaiTour = reader.GetString(1)
							});
						}
					}
				}
			}
			return View(tourTypes);
		}
		[HttpPost]
		public IActionResult ThemTourView(string tenTour, int trangThai, string soNgay, string diaDiem, decimal giaTour, int maLoai, IFormFile hinhAnh1, IFormFile hinhAnh2, IFormFile hinhAnh3, string moTa, string lichTrinh)
		{

			string hinhAnh1Path = SaveFile(hinhAnh1);
			string hinhAnh2Path = SaveFile(hinhAnh2);
			string hinhAnh3Path = SaveFile(hinhAnh3);

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "INSERT INTO Tour (TenTour, MaLoaiTour,SoNgay,DiaDiem, GiaTour, HinhAnh1, HinhAnh2, HinhAnh3, TrangThai, MoTa, LichTrinh) " +
							 "VALUES (@tenTour, @maLoai,@soNgay,@diaDiem, @giaTour, @hinhAnh1Path, @hinhAnh2Path, @hinhAnh3Path, @trangThai, @moTa, @lichTrinh)";
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@tenTour", tenTour);
					command.Parameters.AddWithValue("@maLoai", maLoai);
					command.Parameters.AddWithValue("@soNgay", soNgay);
					command.Parameters.AddWithValue("@diaDiem", diaDiem);
					command.Parameters.AddWithValue("@giaTour", giaTour);
					command.Parameters.AddWithValue("@hinhAnh1Path", hinhAnh1Path);
					command.Parameters.AddWithValue("@hinhAnh2Path", hinhAnh2Path);
					command.Parameters.AddWithValue("@hinhAnh3Path", hinhAnh3Path);
					command.Parameters.AddWithValue("@trangThai", trangThai);
					command.Parameters.AddWithValue("@moTa", moTa);
					command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

					command.ExecuteNonQuery();
				}
			}
			return RedirectToAction("QuanLyTourView");
		}

		#endregion

		#region KhachSan
		public IActionResult QuanLyKhachSanView(int? page)
		{
			int pageSize = 6; // Số lượng mục trên mỗi trang
			int pageNumber = (page ?? 1);

			List<KhachSanViewModel> khachsan = new List<KhachSanViewModel>();

			using (var connection = new SqlConnection(_connectionString))
			{
				string sql = "SELECT p.MaKhachSan, p.TenKhachSan, l.TenLoaikhachSan,p.SoNgay, p.DiaDiem, p.GiaKhachSan, p.HinhAnh1, p.HinhAnh2, p.HinhAnh3, p.MaLoaiKhachSan, p.MoTa, p.LichTrinh " +
							 "FROM KhachSan p " +
							 "INNER JOIN LoaiKhachSan l ON p.MaLoaiKhachSan = l.MaLoaiKhachSan";

				using (var command = new SqlCommand(sql, connection))
				{
					connection.Open();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							khachsan.Add(new KhachSanViewModel
							{
								MaKhachSan = reader.GetInt32(reader.GetOrdinal("MaKhachSan")),
								TenKhachSan = !reader.IsDBNull(reader.GetOrdinal("TenKhachSan")) ? reader.GetString(reader.GetOrdinal("TenKhachSan")) : string.Empty,
								TenLoaiKhachSan = !reader.IsDBNull(reader.GetOrdinal("TenLoaikhachSan")) ? reader.GetString(reader.GetOrdinal("TenLoaiKhachSan")) : string.Empty,
								GiaKhachSan = !reader.IsDBNull(reader.GetOrdinal("GiaKhachSan")) ? reader.GetDecimal(reader.GetOrdinal("GiaKhachSan")) : 0,
								SoNgay = !reader.IsDBNull(reader.GetOrdinal("SoNgay")) ? reader.GetString(reader.GetOrdinal("SoNgay")) : string.Empty,
								DiaDiem = !reader.IsDBNull(reader.GetOrdinal("DiaDiem")) ? reader.GetString(reader.GetOrdinal("DiaDiem")) : string.Empty,
								HinhAnh1 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh1")) ? reader.GetString(reader.GetOrdinal("HinhAnh1")) : string.Empty,
								HinhAnh2 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh2")) ? reader.GetString(reader.GetOrdinal("HinhAnh2")) : string.Empty,
								HinhAnh3 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh3")) ? reader.GetString(reader.GetOrdinal("HinhAnh3")) : string.Empty,
								MaLoaiKhachSan = !reader.IsDBNull(reader.GetOrdinal("MaLoaiKhachSan")) ? reader.GetInt32(reader.GetOrdinal("MaLoaiKhachSan")) : 0,
								MoTa = !reader.IsDBNull(reader.GetOrdinal("MoTa")) ? reader.GetString(reader.GetOrdinal("MoTa")) : string.Empty,
								LichTrinh = !reader.IsDBNull(reader.GetOrdinal("LichTrinh")) ? reader.GetString(reader.GetOrdinal("LichTrinh")) : string.Empty,
							});
						}
					}
				}
			}

			var pagedKhachSan = khachsan.ToPagedList(pageNumber, pageSize);
			return View(pagedKhachSan);
		}
		public IActionResult XoaKhachSanView(int maKhachSan)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				// Kiểm tra xem khách sạn có tồn tại không
				string checkSql = "SELECT COUNT(*) FROM KhachSan WHERE MaKhachSan = @maKhachSan";
				using (var checkCommand = new SqlCommand(checkSql, connection))
				{
					checkCommand.Parameters.AddWithValue("@maKhachSan", maKhachSan);
					int exists = (int)checkCommand.ExecuteScalar();

					if (exists == 0)
					{
						TempData["ErrorMessage"] = "Khách sạn không tồn tại";
						return RedirectToAction("QuanLyKhachSanView");
					}
				}

				// Nếu tồn tại, tiến hành xóa
				string deleteSql = "DELETE FROM KhachSan WHERE MaKhachSan = @maKhachSan";
				using (var deleteCommand = new SqlCommand(deleteSql, connection))
				{
					deleteCommand.Parameters.AddWithValue("@maKhachSan", maKhachSan);
					int rowsAffected = deleteCommand.ExecuteNonQuery();

					if (rowsAffected > 0)
					{
						TempData["Success"] = "Xóa khách sạn thành công";
					}
					else
					{
						TempData["ErrorMessage"] = "Không thể xóa khách sạn do lỗi hệ thống";
					}
				}
			}

			return RedirectToAction("QuanLyKhachSanView");
		}
		[HttpGet]
		public IActionResult SuaKhachSanView(int maKhachSan, int maLoai)
		{
			List<LoaiKhachSan> khachsanTypes = new List<LoaiKhachSan>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "SELECT MaLoaiKhachSan, TenLoaiKhachSan FROM LoaiKhachSan";
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							khachsanTypes.Add(new LoaiKhachSan
							{
								MaLoaiKhachSan = reader.GetInt32(0),
								TenLoaiKhachSan = reader.GetString(1)
							});
						}
					}
				}
			}

			List<KhachSanViewModel> khachsan = new List<KhachSanViewModel>();
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string rooms = "SELECT p.MaKhachSan, p.TenKhachSan, p.MaLoaiKhachSan, p.TrangThai,p.SoNgay, p.DiaDiem, p.GiaKhachSan, p.HinhAnh1, p.HinhAnh2, p.HinhAnh3, l.TenLoaiKhachSan, p.MoTa, p.LichTrinh " +
				   "FROM KhachSan p " +
				   "INNER JOIN LoaiKhachSan l ON p.MaLoaiKhachSan = l.MaLoaiKhachSan " +
				   "WHERE p.MaKhachSan =1 And p.MaLoaiKhachSan = @maLoai";
				using (var command = new SqlCommand(rooms, connection))
				{
					command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
					command.Parameters.AddWithValue("@maLoai", maLoai);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							khachsan.Add(new KhachSanViewModel
							{
								MaKhachSan = reader.GetInt32(reader.GetOrdinal("MaKhachSan")),
								TenKhachSan = !reader.IsDBNull(reader.GetOrdinal("TenKhachSan")) ? reader.GetString(reader.GetOrdinal("TenKhachSan")) : string.Empty,
								TenLoaiKhachSan = !reader.IsDBNull(reader.GetOrdinal("TenLoaiKhachSan")) ? reader.GetString(reader.GetOrdinal("TenLoaiKhachSan")) : string.Empty,
								SoNgay = !reader.IsDBNull(reader.GetOrdinal("SoNgay")) ? reader.GetString(reader.GetOrdinal("SoNgay")) : string.Empty,
								DiaDiem = !reader.IsDBNull(reader.GetOrdinal("DiaDiem")) ? reader.GetString(reader.GetOrdinal("DiaDiem")) : string.Empty,
								GiaKhachSan = !reader.IsDBNull(reader.GetOrdinal("GiaKhachSan")) ? reader.GetDecimal(reader.GetOrdinal("GiaKhachSan")) : 0,
								HinhAnh1 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh1")) ? reader.GetString(reader.GetOrdinal("HinhAnh1")) : string.Empty,
								HinhAnh2 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh2")) ? reader.GetString(reader.GetOrdinal("HinhAnh2")) : string.Empty,
								HinhAnh3 = !reader.IsDBNull(reader.GetOrdinal("HinhAnh3")) ? reader.GetString(reader.GetOrdinal("HinhAnh3")) : string.Empty,
								MaLoaiKhachSan = !reader.IsDBNull(reader.GetOrdinal("MaLoaiKhachSan")) ? reader.GetInt32(reader.GetOrdinal("MaLoaiKhachSan")) : 0,
								TrangThai = !reader.IsDBNull(reader.GetOrdinal("TrangThai")) && reader.GetBoolean(reader.GetOrdinal("TrangThai")),
								MoTa = !reader.IsDBNull(reader.GetOrdinal("MoTa")) ? reader.GetString(reader.GetOrdinal("MoTa")) : string.Empty,
								LichTrinh = !reader.IsDBNull(reader.GetOrdinal("LichTrinh")) ? reader.GetString(reader.GetOrdinal("LichTrinh")) : string.Empty,
							});
						}
					}
				}

			}
			TViewModel viewModel = new TViewModel
			{
				LoaiKhachSanView = khachsanTypes,
				KhachSanView = khachsan
			};
			return View(viewModel);
		}
		[HttpPost]
		public IActionResult SuaKhachSanView(int maKhachSan, string tenKhachSan, string soNgay, string diaDiem, decimal giaKhachSan, int maLoai, IFormFile hinhAnh1, IFormFile hinhAnh2, IFormFile hinhAnh3, int trangThai, string moTa, string lichTrinh)
		{
			string hinhAnh1Path = SaveFile(hinhAnh1);
			string hinhAnh2Path = SaveFile(hinhAnh2);
			string hinhAnh3Path = SaveFile(hinhAnh3);
			if (hinhAnh1Path == null && hinhAnh2Path == null && hinhAnh3Path == null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE KhachSan SET MaLoaiKhachSan = @maLoai, TenKhachSan = @tenKhachSan,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaKhachSan = @giaKhachSan, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								 "WHERE MaKhachSan = @maKhachSan";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
						command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else if (hinhAnh1Path != null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE KhachSan SET MaLoaiKhachSan = @maLoai, TenKhachSan = @tenKhachSan,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaKhachSan = @giaKhachSan, HinhAnh1 = @hinhAnh1Path, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								"WHERE MaKhachSan = @maKhachSan";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
						command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
						command.Parameters.AddWithValue("@hinhAnh1Path", hinhAnh1Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else if (hinhAnh2Path != null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE KhachSan SET MaLoaiKhachSan = @maLoai, TenKhachSan = @tenKhachSan,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaKhachSan = @giaKhachSan, HinhAnh2 = @hinhAnh2Path, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
							   "WHERE MaKhachSan = @maKhachSan";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
						command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
						command.Parameters.AddWithValue("@hinhAnh2Path", hinhAnh2Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else if (hinhAnh3Path != null)
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE KhachSan SET MaLoaiKhachSan = @maLoai, TenKhachSan = @tenKhachSan,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaKhachSan = @giaKhachSan, HinhAnh3 = @hinhAnh3Path, TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								"WHERE MaKhachSan = @maKhachSan";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
						command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
						command.Parameters.AddWithValue("@hinhAnh3Path", hinhAnh3Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			else
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					string sql = "UPDATE KhachSan SET MaLoaiKhachSan = @maLoai, TenKhachSan = @tenKhachSan,SoNgay = @soNgay,DiaDiem = @diaDiem, GiaKhachSan = @giaKhachSan, HinhAnh1 = @hinhAnh1Path,HinhAnh2 = @hinhAnh2Path,HinhAnh3 = @hinhAnh3Path,TrangThai = @trangThai, MoTa = @moTa, LichTrinh = @lichTrinh " +
								"WHERE MaKhachSan = @maKhachSan";
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
						command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
						command.Parameters.AddWithValue("@soNgay", soNgay);
						command.Parameters.AddWithValue("@diaDiem", diaDiem);
						command.Parameters.AddWithValue("@maLoai", maLoai);
						command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
						command.Parameters.AddWithValue("@hinhAnh1Path", hinhAnh1Path);
						command.Parameters.AddWithValue("@hinhAnh2Path", hinhAnh2Path);
						command.Parameters.AddWithValue("@hinhAnh3Path", hinhAnh3Path);
						command.Parameters.AddWithValue("@trangThai", trangThai);
						command.Parameters.AddWithValue("@moTa", moTa);
						command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

						command.ExecuteNonQuery();
					}
				}
			}
			return RedirectToAction("QuanLyKhachSanView");
		}
		[HttpGet]
		public IActionResult ThemKhachSanView()
		{
			List<LoaiKhachSan> khachsanTypes = new List<LoaiKhachSan>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "SELECT MaLoaiKhachSan, TenLoaiKhachSan FROM LoaiKhachSan";
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							khachsanTypes.Add(new LoaiKhachSan
							{
								MaLoaiKhachSan = reader.GetInt32(0),
								TenLoaiKhachSan = reader.GetString(1)
							});
						}
					}
				}
			}
			return View(khachsanTypes);
		}
		[HttpPost]
		public IActionResult ThemKhachSanView(string tenKhachSan, int trangThai, string soNgay, string diaDiem, decimal giaKhachSan, int maLoai, IFormFile hinhAnh1, IFormFile hinhAnh2, IFormFile hinhAnh3, string moTa, string lichTrinh)
		{

			string hinhAnh1Path = SaveFile(hinhAnh1);
			string hinhAnh2Path = SaveFile(hinhAnh2);
			string hinhAnh3Path = SaveFile(hinhAnh3);

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string sql = "INSERT INTO KhachSan (TenKhachSan, MaLoaiKhachSan,SoNgay,DiaDiem, GiaKhachSan, HinhAnh1, HinhAnh2, HinhAnh3, TrangThai, MoTa, LichTrinh) " +
							 "VALUES (@tenKhachSan, @maLoai,@soNgay,@diaDiem, @giaKhachSan, @hinhAnh1Path, @hinhAnh2Path, @hinhAnh3Path, @trangThai, @moTa, @lichTrinh)";
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
					command.Parameters.AddWithValue("@maLoai", maLoai);
					command.Parameters.AddWithValue("@soNgay", soNgay);
					command.Parameters.AddWithValue("@diaDiem", diaDiem);
					command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
					command.Parameters.AddWithValue("@hinhAnh1Path", hinhAnh1Path);
					command.Parameters.AddWithValue("@hinhAnh2Path", hinhAnh2Path);
					command.Parameters.AddWithValue("@hinhAnh3Path", hinhAnh3Path);
					command.Parameters.AddWithValue("@trangThai", trangThai);
					command.Parameters.AddWithValue("@moTa", moTa);
					command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

					command.ExecuteNonQuery();
				}
			}
			return RedirectToAction("QuanLyKhachSanView");
		}



		#endregion

		public ActionResult QuanLyNguoiDung(int? page)
        {
            int pageSize = 10; // Số lượng mục trên mỗi trang
            int pageNumber = (page ?? 1);

            List<ThongTinNguoiDung> user = new List<ThongTinNguoiDung>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT MaNguoiDung,TenNguoiDung,DiaChi,SoDienThoai,TrangThai,TenDangNhap,MatKhau FROM NguoiDung WHERE LoaiTaiKhoan = 0";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user.Add(new ThongTinNguoiDung
                            {
                                MaNguoiDung =  reader.GetInt32(0),
                                TenNguoiDung = reader.GetString(1),
                                DiaChi = reader.GetString(2),
                                SoDienThoai = reader.GetString(3),
                                TrangThai = reader.GetBoolean(4),
                                TenDangNhap =   reader.GetString(5),
                                MatKhau = reader.GetString(6),
                            });
                        }
                    }
                }
            }

            var pagedUser = user.ToPagedList(pageNumber, pageSize);
            return View(pagedUser);

        }
        [HttpPost]
        public IActionResult KichHoat(int maUser1)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE NguoiDung SET TrangThai = 1 WHERE MaNguoiDung = @MaNguoiDung";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MaNguoiDung", maUser1);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                TempData["SuccessMessage"] = "Người dùng đã được kích hoạt thành công!";
                return RedirectToAction("QuanLyNguoiDung"); // Điều chỉnh theo trang danh sách người dùng
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("QuanLyNguoiDung");
            }
        }
        [HttpPost]
        public IActionResult HuyKickHoat(int maUser2)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE NguoiDung SET TrangThai = 0 WHERE MaNguoiDung = @MaNguoiDung";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MaNguoiDung", maUser2);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                TempData["SuccessMessage"] = "Người dùng đã được vô hiệu hóa thành công!";
                return RedirectToAction("QuanLyNguoiDung"); // Điều chỉnh theo trang danh sách người dùng
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("QuanLyNguoiDung");
            }
        }
        private string SaveFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/hinhanh", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return fileName;
            }
            return null;
        }
    }
}
