using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTour.Models;
using QuanLyTour.Models.KhachSan;
using QuanLyTour.Models.Tour;
using X.PagedList;
using X.PagedList.Extensions;
namespace QuanLyTour.Controllers
{
    public class KhachSanController : Controller
    {
		private readonly string _connectionString;
		private readonly ILogger<KhachSanController> _logger;

		public KhachSanController(ILogger<KhachSanController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_connectionString = configuration.GetConnectionString("DefaultConnection");
		}
		public IActionResult Index()
        {
			// Kiểm tra thông tin người dùng từ Session
			var tenNguoiDung = HttpContext.Session.GetString("TenNguoiDung");
			ViewBag.TenNguoiDung = tenNguoiDung;

			var khachsansTrongNuoc = new List<KhachSanViewModel>();

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					string queryTrongNuoc = @"
                SELECT MaKhachSan, TenKhachSan, SoNgay, DiaDiem, MaLoaiKhachSan, TrangThai, GiaKhachSan, HinhAnh1 
                FROM KhachSan 
                WHERE TrangThai = 1";

					using (var command = new SqlCommand(queryTrongNuoc, connection))
					{

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								khachsansTrongNuoc.Add(new KhachSanViewModel
								{
									MaKhachSan = reader.GetInt32(0),
									TenKhachSan = reader.GetString(1),
									SoNgay = reader.GetString(2),
									DiaDiem = reader.GetString(3),
									MaLoaiKhachSan = reader.GetInt32(4),
									TrangThai = reader.GetBoolean(5),
									GiaKhachSan = reader.GetDecimal(6),
									HinhAnh1 = reader.GetString(7)
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
			var viewModel = new KhachSanIndexViewModel
			{
				KhachSansTrongNuoc = khachsansTrongNuoc
			};

			return View(viewModel);
		}
		public IActionResult ChiTietKhachSan(string maKhachSan, string maLoai, int? page)
		{
			int pageSize = 10;
			int pageNumber = page ?? 1;
			KhachSanViewModel khachsan = null;
			List<KhachSanViewModel> khachsantt = new List<KhachSanViewModel>();

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

				// Truy vấn thông tin KhachSan hiện tại
				string sql = "SELECT p.MaKhachSan, p.TenKhachSan, p.MaLoaiKhachSan, p.TrangThai, p.SoNgay, p.DiaDiem, p.GiaKhachSan, p.HinhAnh1, p.HinhAnh2, p.HinhAnh3, l.TenLoaiKhachSan, p.MoTa, p.LichTrinh " +
					"FROM KhachSan p " +
					"INNER JOIN LoaiKhachSan l ON p.MaLoaiKhachSan = l.MaLoaiKhachSan " +
					"WHERE p.MaKhachSan = @maKhachSan";


				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@maKhachSan", maKhachSan);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							khachsan = new KhachSanViewModel
							{
								MaKhachSan = !reader.IsDBNull(0) ? reader.GetInt32(0) : 0,
								TenKhachSan = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
								MaLoaiKhachSan = !reader.IsDBNull(2) ? reader.GetInt32(2) : 0,
								TrangThai = !reader.IsDBNull(3) && reader.GetBoolean(3),
								SoNgay = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
								DiaDiem = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty,
								GiaKhachSan = !reader.IsDBNull(6) ? reader.GetDecimal(6) : 0,
								HinhAnh1 = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty,
								HinhAnh2 = !reader.IsDBNull(8) ? reader.GetString(8) : string.Empty,
								HinhAnh3 = !reader.IsDBNull(9) ? reader.GetString(9) : string.Empty,
								TenLoaiKhachSan = !reader.IsDBNull(10) ? reader.GetString(10) : string.Empty,
								MoTa = !reader.IsDBNull(11) ? reader.GetString(11) : string.Empty,
								LichTrinh = !reader.IsDBNull(12) ? reader.GetString(12) : string.Empty,
							};
						}
					}
				}
				// Truy vấn các KhachSan tương tự
				string sqlKhachSantt = "SELECT p.MaKhachSan, p.TenKhachSan, p.MaLoaiKhachSan, p.TrangThai, p.SoNgay, p.DiaDiem, p.GiaKhachSan, p.HinhAnh1, p.HinhAnh2, p.HinhAnh3, l.TenLoaiKhachSan " +
								   "FROM KhachSan p " +
								   "INNER JOIN LoaiKhachSan l ON p.MaLoaiKhachSan = l.MaLoaiKhachSan " +
								   "WHERE  p.MaKhachSan != @maKhachSan AND p.MaLoaiKhachSan = @maLoaiKhachSan";

				using (var command = new SqlCommand(sqlKhachSantt, connection))
				{
					command.Parameters.AddWithValue("@maKhachSan", maKhachSan);
					command.Parameters.AddWithValue("@maLoaiKhachSan", khachsan?.MaLoaiKhachSan ?? 0); // Sử dụng `MaLoaiKhachSan` từ KhachSan hiện tại

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							khachsantt.Add(new KhachSanViewModel
							{
								MaKhachSan = reader.GetInt32(0),
								TenKhachSan = reader.GetString(1),
								MaLoaiKhachSan = reader.GetInt32(2),
								TrangThai = reader.GetBoolean(3),
								SoNgay = reader.IsDBNull(4) ? "Không xác định" : reader.GetString(4),  // Xử lý NULL
								DiaDiem = reader.IsDBNull(5) ? "Không có địa điểm" : reader.GetString(5),  // Xử lý NULL
								GiaKhachSan = reader.GetDecimal(6),
								HinhAnh1 = reader.GetString(7),
								HinhAnh2 = reader.GetString(8),
								HinhAnh3 = reader.GetString(9),
								TenLoaiKhachSan = reader.GetString(10)
							});
						}
					}
				}
			}

			if (khachsan == null)
			{
				return NotFound();
			}

			// Sử dụng PagedList để phân trang danh sách KhachSan tương tự
			var pagedAvailableKhachSans = khachsantt.ToPagedList(pageNumber, pageSize);

			var viewModel = new DetailViewModel
			{
				KhachSanHienTai = khachsan,
				KhachSanTuongTu = pagedAvailableKhachSans
			};

			return View(viewModel);

		}
		public IActionResult DanhMucKhachSan(int pageTrongNuoc = 1, int pageNuocNgoai = 1, int pageSize = 12)
		{
			var model = new KhachSanTabsViewModel();
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					// Lấy danh sách KhachSan Trong Nước
					string queryTrongNuoc = @"
            SELECT p.MaKhachSan, p.TenKhachSan, p.MaLoaiKhachSan, p.TrangThai, p.SoNgay, p.DiaDiem, p.GiaKhachSan, p.HinhAnh1, l.TenLoaiKhachSan 
            FROM KhachSan p 
            INNER JOIN LoaiKhachSan l ON p.MaLoaiKhachSan = l.MaLoaiKhachSan 
            WHERE p.MaLoaiKhachSan = 1";

					var khachsanTrongNuoc = GetKhachSans(connection, queryTrongNuoc);
					model.KhachSanTrongNuoc = khachsanTrongNuoc.ToPagedList(pageTrongNuoc, pageSize);

					// Lấy danh sách KhachSan Nước Ngoài
					string queryNuocNgoai = @"
            SELECT p.MaKhachSan, p.TenKhachSan, p.MaLoaiKhachSan, p.TrangThai, p.SoNgay, p.DiaDiem, p.GiaKhachSan, p.HinhAnh1, l.TenLoaiKhachSan 
            FROM KhachSan p 
            INNER JOIN LoaiKhachSan l ON p.MaLoaiKhachSan = l.MaLoaiKhachSan 
            WHERE p.MaLoaiKhachSan = 2";

					var khachsanNuocNgoai = GetKhachSans(connection, queryNuocNgoai);
					model.KhachSanNuocNgoai = khachsanNuocNgoai.ToPagedList(pageNuocNgoai, pageSize);
				}
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
			}

			return View(model);
		}
		private List<KhachSanViewModel> GetKhachSans(SqlConnection connection, string query)
		{
			var khachsans = new List<KhachSanViewModel>();

			using (var command = new SqlCommand(query, connection))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						khachsans.Add(new KhachSanViewModel
						{
							MaKhachSan = reader.GetInt32(0),
							TenKhachSan = reader.GetString(1),
							MaLoaiKhachSan = reader.GetInt32(2),
							TrangThai = reader.GetBoolean(3),
							SoNgay = reader.IsDBNull(4) ? "Không xác định" : reader.GetString(4),  // Xử lý NULL
							DiaDiem = reader.IsDBNull(5) ? "Không có địa điểm" : reader.GetString(5),  // Xử lý NULL
							GiaKhachSan = reader.GetDecimal(6),
							HinhAnh1 = reader.IsDBNull(7) ? "" : reader.GetString(7), // Xử lý NULL
							TenLoaiKhachSan = reader.GetString(8)
						});
					}
				}
			}

			return khachsans;
		}
		public IActionResult TimKiemKhachSan(string keyword, int page = 1, int pageSize = 6)
		{
			var khachsans = new List<KhachSanViewModel>();

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					// Truy vấn dữ liệu KhachSan dựa vào từ khóa
					string query = @"
                SELECT t.MaKhachSan, t.TenKhachSan, t.MaLoaiKhachSan,t.SoNgay,t.DiaDiem, t.TrangThai, t.GiaKhachSan, t.HinhAnh1, l.TenLoaiKhachSan
                FROM KhachSan t
                INNER JOIN LoaiKhachSan l ON t.MaLoaiKhachSan = l.MaLoaiKhachSan
                WHERE (TenKhachSan LIKE @Keyword)";

					using (var command = new SqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								khachsans.Add(new KhachSanViewModel
								{
									MaKhachSan = reader.GetInt32(0),
									TenKhachSan = reader.GetString(1),
									MaLoaiKhachSan = reader.GetInt32(2),
									SoNgay = reader.GetString(3),
									DiaDiem = reader.GetString(4),
									TrangThai = reader.GetBoolean(5),
									GiaKhachSan = reader.GetDecimal(6),
									HinhAnh1 = reader.GetString(7),
									TenLoaiKhachSan = reader.GetString(8)
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
			var pagedKhachSans = khachsans.ToPagedList(page, pageSize);

			// Gửi từ khóa và danh sách KhachSan tìm được về View
			ViewBag.Keyword = keyword;
			return View(pagedKhachSans);
		}

		public IActionResult DatKhachSan(int MaNguoiDung, string TenNguoiDung, string SoDienThoai, string DiaChi, string SoThe, string ChuThe, int maKhachSan)
		{
			if (string.IsNullOrWhiteSpace(SoThe) || string.IsNullOrWhiteSpace(ChuThe))
			{
				ModelState.AddModelError("", "Thông tin thanh toán không được để trống.");
				return RedirectToAction("ChiTietKhachSan", new { maKhachSan });
			}

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				

				// Tính tổng tiền
				decimal giaKhachSan = 0;
				string sqlGetGiaKhachSan = "SELECT GiaKhachSan FROM KhachSan WHERE MaKhachSan = @MaKhachSan";
				using (var command = new SqlCommand(sqlGetGiaKhachSan, connection))
				{
					command.Parameters.AddWithValue("@MaKhachSan", maKhachSan);
					giaKhachSan = Convert.ToDecimal(command.ExecuteScalar());
				}

				decimal tongTien = giaKhachSan ;

				// Thêm thông tin thanh toán vào bảng HoaDon
				string sqlInsertHoaDon = "INSERT INTO HoaDonKhachSan (MaNguoiDung, TongTien, NgayThanhToan) " +
										 "VALUES (@MaNguoiDung, @TongTien, @NgayThanhToan)";
				using (var command = new SqlCommand(sqlInsertHoaDon, connection))
				{
					command.Parameters.AddWithValue("@MaNguoiDung", MaNguoiDung);
				
					command.Parameters.AddWithValue("@TongTien", tongTien);
					command.Parameters.AddWithValue("@NgayThanhToan", DateTime.Now);

					command.ExecuteNonQuery();
				}
			}

			TempData["SuccessMessage"] = "Đặt KhachSan và thanh toán thành công!";
			return RedirectToAction("ThanhCongView", "KhachSan");
		}
		public IActionResult ThanhCongView()
		{
			return View();
		}
		public IActionResult ThongTinKhachSanDat()
		{
			int maNguoiDung = HttpContext.Session.GetInt32("UserId") ?? 0; // Lấy mã người dùng từ session

			if (maNguoiDung == 0)
			{
				return RedirectToAction("DangNhap", "Home"); // Nếu chưa đăng nhập, chuyển tới trang đăng nhập
			}

			List<KhachSanDatViewModel> danhSachKhachSanDat = new List<KhachSanDatViewModel>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				string sql = @"
				SELECT 
					MaHoaDonKhachSan,
				    MaNguoiDung,
					TongTien,
					NgayThanhToan
					FROM HoaDonKhachSan
            WHERE MaNguoiDung = @MaNguoiDung";

				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							danhSachKhachSanDat.Add(new KhachSanDatViewModel
							{
								MaHoaDonKhachSan = reader.GetInt32(0),
								MaNguoiDung = reader.GetInt32(1),

								TongTien = reader.GetDecimal(2),
								NgayThanhToan = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
							});
						}
					}
				}
			}
			return View(danhSachKhachSanDat);
		}


	}
}
