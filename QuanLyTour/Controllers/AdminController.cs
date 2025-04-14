using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using X.PagedList;
using QuanLyTour.Models;
using X.PagedList.Extensions;
using QuanLyTour.Models.KhachSan;
using QuanLyTour.Models.Tour;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyTour.Models.VeMayBay;


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

		#region PhieuHD
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
		#endregion

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
			List<KhachSanViewModel> khachsan = new List<KhachSanViewModel>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				// Lấy danh sách loại khách sạn
				string sqlLoai = "SELECT MaLoaiKhachSan, TenLoaiKhachSan FROM LoaiKhachSan";
				using (var command = new SqlCommand(sqlLoai, connection))
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

				// Lấy thông tin khách sạn bằng Stored Procedure
				using (var command = new SqlCommand("sp_GetKhachSan", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
					command.Parameters.AddWithValue("@maLoai", maLoai);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							khachsan.Add(new KhachSanViewModel
							{
								MaKhachSan = reader.GetInt32(0),
								TenKhachSan = reader.GetString(1),
								MaLoaiKhachSan = reader.GetInt32(2),
								TrangThai = reader.GetBoolean(3),
								SoNgay = reader.GetString(4),
								DiaDiem = reader.GetString(5),
								GiaKhachSan = reader.GetDecimal(6),
								HinhAnh1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
								HinhAnh2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
								HinhAnh3 = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
								TenLoaiKhachSan = reader.GetString(10),
								MoTa = reader.GetString(11),
								LichTrinh = reader.GetString(12),
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
		public IActionResult SuaKhachSanView(int maKhachSan, string tenKhachSan, string soNgay, string diaDiem, decimal giaKhachSan,int maLoai, IFormFile hinhAnh1, IFormFile hinhAnh2, IFormFile hinhAnh3,int trangThai, string moTa, string lichTrinh)
		{
			string hinhAnh1Path = SaveFile(hinhAnh1);
			string hinhAnh2Path = SaveFile(hinhAnh2);
			string hinhAnh3Path = SaveFile(hinhAnh3);

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("sp_UpdateKhachSan", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
					command.Parameters.AddWithValue("@tenKhachSan", tenKhachSan);
					command.Parameters.AddWithValue("@soNgay", soNgay);
					command.Parameters.AddWithValue("@diaDiem", diaDiem);
					command.Parameters.AddWithValue("@giaKhachSan", giaKhachSan);
					command.Parameters.AddWithValue("@maLoai", maLoai);
					command.Parameters.AddWithValue("@trangThai", trangThai);
					command.Parameters.AddWithValue("@moTa", moTa);
					command.Parameters.AddWithValue("@lichTrinh", lichTrinh);

					// Xử lý hình ảnh nếu null thì gửi NULL
					command.Parameters.AddWithValue("@hinhAnh1", string.IsNullOrEmpty(hinhAnh1Path) ? (object)DBNull.Value : hinhAnh1Path);
					command.Parameters.AddWithValue("@hinhAnh2", string.IsNullOrEmpty(hinhAnh2Path) ? (object)DBNull.Value : hinhAnh2Path);
					command.Parameters.AddWithValue("@hinhAnh3", string.IsNullOrEmpty(hinhAnh3Path) ? (object)DBNull.Value : hinhAnh3Path);

					command.ExecuteNonQuery();
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

        #region VeMayBay

        // Lấy danh sách vé (join ChuyenBay & HangHangKhong) + phân trang
        public IActionResult QuanLyVeMayBayView(int? page)
        {
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            List<VeMayBayViewModel> dsVe = new List<VeMayBayViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT v.MaVe,
                   v.IdChuyenBay, 
                   v.HangVe, 
                   v.GiaNet, 
                   v.ThuePhi,
                   v.TongTien,
                   v.HanhLyXachTayKg, 
                   v.HanhLyKyGuiKg, 
                   v.TrangThai,

                   c.MaChuyenBay, 
                   c.DiemDi, 
                   c.DiemDen, 
                   c.GioKhoiHanh, 
                   c.GioHaCanh,
                   c.CoTheDoiVe,
                   c.CoTheHoanVe,
                   c.PhiDoiVe,
                   c.PhiHoanVe,
                   c.IdHangHangKhong,

                   h.TenHang,
                   h.LogoUrl,
                   h.HanhLyXachTayKg AS DefaultHanhLyXachTayKg,
                   h.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
            FROM VeMayBay v
            INNER JOIN ChuyenBay c ON v.IdChuyenBay = c.IdChuyenBay
            INNER JOIN HangHangKhong h ON c.IdHangHangKhong = h.Id
            ORDER BY v.MaVe DESC
        ";

                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var vm = new VeMayBayViewModel
                            {
                                // Vé
                                MaVe = reader.GetInt32(reader.GetOrdinal("MaVe")),
                                IdChuyenBay = reader.GetInt32(reader.GetOrdinal("IdChuyenBay")),
                                HangVe = reader.GetString(reader.GetOrdinal("HangVe")),
                                GiaNet = reader.GetDecimal(reader.GetOrdinal("GiaNet")),
                                ThuePhi = reader.GetDecimal(reader.GetOrdinal("ThuePhi")),
                                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                                HanhLyXachTayKg = reader.IsDBNull(reader.GetOrdinal("HanhLyXachTayKg"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("HanhLyXachTayKg")),
                                HanhLyKyGuiKg = reader.IsDBNull(reader.GetOrdinal("HanhLyKyGuiKg"))
                                                ? (int?)null
                                                : reader.GetInt32(reader.GetOrdinal("HanhLyKyGuiKg")),
                                TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai")),

                                // Chuyến bay
                                MaChuyenBay = reader.GetString(reader.GetOrdinal("MaChuyenBay")),
                                DiemDi = reader.GetString(reader.GetOrdinal("DiemDi")),
                                DiemDen = reader.GetString(reader.GetOrdinal("DiemDen")),
                                GioKhoiHanh = reader.GetDateTime(reader.GetOrdinal("GioKhoiHanh")),
                                GioHaCanh = reader.GetDateTime(reader.GetOrdinal("GioHaCanh")),
                                CoTheDoiVe = reader.GetBoolean(reader.GetOrdinal("CoTheDoiVe")),
                                CoTheHoanVe = reader.GetBoolean(reader.GetOrdinal("CoTheHoanVe")),
                                PhiDoiVe = reader.IsDBNull(reader.GetOrdinal("PhiDoiVe"))
                                           ? (decimal?)null
                                           : reader.GetDecimal(reader.GetOrdinal("PhiDoiVe")),
                                PhiHoanVe = reader.IsDBNull(reader.GetOrdinal("PhiHoanVe"))
                                            ? (decimal?)null
                                            : reader.GetDecimal(reader.GetOrdinal("PhiHoanVe")),

                                // Hãng hàng không
                                TenHang = reader.GetString(reader.GetOrdinal("TenHang")),
                                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl"))
                                          ? string.Empty
                                          : reader.GetString(reader.GetOrdinal("LogoUrl")),
                                DefaultHanhLyXachTayKg = reader.GetInt32(reader.GetOrdinal("DefaultHanhLyXachTayKg")),
                                DefaultHanhLyKyGuiKg = reader.GetInt32(reader.GetOrdinal("DefaultHanhLyKyGuiKg"))
                            };

                            dsVe.Add(vm);
                        }
                    }
                }
            }

            var pagedVe = dsVe.ToPagedList(pageNumber, pageSize);
            return View(pagedVe);
        }

        // Xoá vé
        public IActionResult XoaVeMayBayView(int maVe)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Kiểm tra xem vé có tồn tại không
                string checkSql = "SELECT COUNT(*) FROM VeMayBay WHERE MaVe = @maVe";
                using (var checkCommand = new SqlCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@maVe", maVe);
                    int exists = (int)checkCommand.ExecuteScalar();

                    if (exists == 0)
                    {
                        TempData["ErrorMessage"] = "Vé không tồn tại.";
                        return RedirectToAction("QuanLyVeMayBayView");
                    }
                }

                // Nếu tồn tại, tiến hành xóa
                string deleteSql = "DELETE FROM VeMayBay WHERE MaVe = @maVe";
                using (var deleteCommand = new SqlCommand(deleteSql, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@maVe", maVe);
                    int rowsAffected = deleteCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["Success"] = "Xoá vé thành công.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể xoá vé do lỗi hệ thống.";
                    }
                }
            }

            return RedirectToAction("QuanLyVeMayBayView");
        }

        [HttpGet]
        public IActionResult SuaVeMayBayView(int maVe)
        {
            VeMayBayViewModel ve = null;
            var dsChuyenBay = new List<ChuyenBay>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // 1) Lấy danh sách chuyến bay (để hiển thị dropdown)
                string sqlChuyenBay = "SELECT IdChuyenBay, MaChuyenBay FROM ChuyenBay";
                using (var cmdCB = new SqlCommand(sqlChuyenBay, connection))
                {
                    using (var reader = cmdCB.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dsChuyenBay.Add(new ChuyenBay
                            {
                                IdChuyenBay = reader.GetInt32(0),
                                MaChuyenBay = reader.GetString(1)
                            });
                        }
                    }
                }

                // 2) Lấy thông tin vé cần sửa (join 3 bảng để lấy đầy đủ các trường)
                string sql = @"
    SELECT v.MaVe,
           v.IdChuyenBay, 
           v.HangVe, 
           v.GiaNet, 
           v.ThuePhi,
           v.TongTien,
           v.HanhLyXachTayKg, 
           v.HanhLyKyGuiKg, 
           v.TrangThai,
           
           c.MaChuyenBay,
           c.DiemDi,
           c.DiemDen,
           c.GioKhoiHanh,
           c.GioHaCanh,
           c.CoTheDoiVe,
           c.CoTheHoanVe,
           c.PhiDoiVe,
           c.PhiHoanVe,
           
           h.TenHang,
           h.LogoUrl,
           h.HanhLyXachTayKg AS DefaultHanhLyXachTayKg,
           h.HanhLyKyGuiKg AS DefaultHanhLyKyGuiKg
    FROM VeMayBay v
    INNER JOIN ChuyenBay c ON v.IdChuyenBay = c.IdChuyenBay
    INNER JOIN HangHangKhong h ON c.IdHangHangKhong = h.Id
    WHERE v.MaVe = @maVe
";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@maVe", maVe);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ve = new VeMayBayViewModel
                            {
                                MaVe = reader.GetInt32(reader.GetOrdinal("MaVe")),
                                IdChuyenBay = reader.GetInt32(reader.GetOrdinal("IdChuyenBay")),
                                HangVe = reader.GetString(reader.GetOrdinal("HangVe")),
                                GiaNet = reader.GetDecimal(reader.GetOrdinal("GiaNet")),
                                ThuePhi = reader.GetDecimal(reader.GetOrdinal("ThuePhi")),
                                TongTien = reader.GetDecimal(reader.GetOrdinal("TongTien")),
                                HanhLyXachTayKg = reader.IsDBNull(reader.GetOrdinal("HanhLyXachTayKg"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("HanhLyXachTayKg")),
                                HanhLyKyGuiKg = reader.IsDBNull(reader.GetOrdinal("HanhLyKyGuiKg"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("HanhLyKyGuiKg")),
                                TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai")),
                                // Thông tin chuyến bay
                                MaChuyenBay = reader.GetString(reader.GetOrdinal("MaChuyenBay")),
                                DiemDi = reader.GetString(reader.GetOrdinal("DiemDi")),
                                DiemDen = reader.GetString(reader.GetOrdinal("DiemDen")),
                                GioKhoiHanh = reader.GetDateTime(reader.GetOrdinal("GioKhoiHanh")),
                                GioHaCanh = reader.GetDateTime(reader.GetOrdinal("GioHaCanh")),
                                CoTheDoiVe = reader.GetBoolean(reader.GetOrdinal("CoTheDoiVe")),
                                CoTheHoanVe = reader.GetBoolean(reader.GetOrdinal("CoTheHoanVe")),
                                PhiDoiVe = reader.IsDBNull(reader.GetOrdinal("PhiDoiVe"))
                                            ? (decimal?)null
                                            : reader.GetDecimal(reader.GetOrdinal("PhiDoiVe")),
                                PhiHoanVe = reader.IsDBNull(reader.GetOrdinal("PhiHoanVe"))
                                            ? (decimal?)null
                                            : reader.GetDecimal(reader.GetOrdinal("PhiHoanVe")),
                                // Thông tin hãng hàng không
                                TenHang = reader.GetString(reader.GetOrdinal("TenHang")),
                                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl"))
                                          ? string.Empty
                                          : reader.GetString(reader.GetOrdinal("LogoUrl")),
                                DefaultHanhLyXachTayKg = reader.GetInt32(reader.GetOrdinal("DefaultHanhLyXachTayKg")),
                                DefaultHanhLyKyGuiKg = reader.GetInt32(reader.GetOrdinal("DefaultHanhLyKyGuiKg"))
                            };
                        }
                    }
                }
            }

            if (ve == null)
            {
                return NotFound("Không tìm thấy vé máy bay.");
            }

            // Gán danh sách chuyến bay cho ViewBag (để dùng cho dropdown)
            ViewBag.ChuyenBayList = dsChuyenBay.Select(cb => new SelectListItem
            {
                Value = cb.IdChuyenBay.ToString(),
                Text = cb.MaChuyenBay
            }).ToList();

            return View(ve);
        }

        [HttpPost]
        public IActionResult SuaVeMayBayView(VeMayBayViewModel model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
    UPDATE VeMayBay
    SET IdChuyenBay = @idChuyenBay, 
        GiaNet = @giaNet, 
        ThuePhi = @thuePhi,
        HangVe = @hangVe, 
        HanhLyXachTayKg = @hanhLyXachTayKg,
        HanhLyKyGuiKg = @hanhLyKyGuiKg,
        TrangThai = @trangThai
    WHERE MaVe = @maVe";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@maVe", model.MaVe);
                    command.Parameters.AddWithValue("@idChuyenBay", model.IdChuyenBay);
                    command.Parameters.AddWithValue("@giaNet", model.GiaNet);
                    command.Parameters.AddWithValue("@thuePhi", model.ThuePhi);
                    command.Parameters.AddWithValue("@hangVe", model.HangVe);
                    command.Parameters.AddWithValue("@hanhLyXachTayKg", (object?)model.HanhLyXachTayKg ?? DBNull.Value);
                    command.Parameters.AddWithValue("@hanhLyKyGuiKg", (object?)model.HanhLyKyGuiKg ?? DBNull.Value);
                    command.Parameters.AddWithValue("@trangThai", model.TrangThai);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("QuanLyVeMayBayView");
        }

        // Hiển thị form thêm vé
        [HttpGet]
        public IActionResult ThemVeMayBayView()
        {
            // Lấy ds chuyến bay
            var dsChuyenBay = new List<ChuyenBay>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using (var cmd = new SqlCommand("SELECT IdChuyenBay, MaChuyenBay FROM ChuyenBay", connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    dsChuyenBay.Add(new ChuyenBay
                    {
                        IdChuyenBay = reader.GetInt32(0),
                        MaChuyenBay = reader.GetString(1)
                    });

            ViewBag.ChuyenBayList = dsChuyenBay
                .Select(cb => new SelectListItem
                {
                    Value = cb.IdChuyenBay.ToString(),
                    Text = cb.MaChuyenBay
                }).ToList();

            // Trả về model rỗng (mặc định SoLuongVe=0)
            return View(new VeMayBayViewModel());
        }

        [HttpPost]
        public IActionResult ThemVeMayBayView(VeMayBayViewModel model)
        {
            // Tự động tính trạng thái dựa trên SoLuongVe
            bool trangThai = model.SoLuongVe > 0;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Nếu cột TongTien là computed thì không cần tính, DB tự tính
            string sql = @"
				INSERT INTO VeMayBay
					(IdChuyenBay, HangVe, GiaNet, ThuePhi, HanhLyXachTayKg, HanhLyKyGuiKg, SoLuongVe, TrangThai)
				VALUES
					(@idChuyenBay, @hangVe, @giaNet, @thuePhi, @xachtay, @kygui, @soLuongVe, @trangThai)";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@idChuyenBay", model.IdChuyenBay);
            cmd.Parameters.AddWithValue("@hangVe", model.HangVe);
            cmd.Parameters.AddWithValue("@giaNet", model.GiaNet);
            cmd.Parameters.AddWithValue("@thuePhi", model.ThuePhi);
            cmd.Parameters.AddWithValue("@xachtay", (object?)model.HanhLyXachTayKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@kygui", (object?)model.HanhLyKyGuiKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@soLuongVe", model.SoLuongVe);
            cmd.Parameters.AddWithValue("@trangThai", trangThai);
            cmd.ExecuteNonQuery();

            return RedirectToAction("QuanLyVeMayBayView");
        }


        #endregion

        #region QuanLyND

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
								MaNguoiDung = reader.GetInt32(0),
								TenNguoiDung = reader.GetString(1),
								DiaChi = reader.GetString(2),
								SoDienThoai = reader.GetString(3),
								TrangThai = reader.GetBoolean(4),
								TenDangNhap = reader.GetString(5),
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
		#endregion

		public IActionResult HoTroView(int? page)
		{
			List<HoTro> danhSachHoTro = new List<HoTro>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				string query = @"
                SELECT h.Id, h.MaNguoiDung, h.Message, h.NgayTao, nd.TenNguoiDung 
                FROM HoTro h 
                JOIN NguoiDung nd ON h.MaNguoiDung = nd.MaNguoiDung
                ORDER BY h.NgayTao DESC";

				using (SqlCommand cmd = new SqlCommand(query, connection))
				{
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							danhSachHoTro.Add(new HoTro
							{
								Id = reader.GetInt32(0),
								MaNguoiDung = reader.GetInt32(1),
								Message = reader.GetString(2),
								NgayTao = reader.GetDateTime(3),
							});
						}
					}
				}
			}

			int pageSize = 10;
			int pageNumber = (page ?? 1);
			return View(danhSachHoTro.ToPagedList(pageNumber, pageSize));
		}
	}

}

