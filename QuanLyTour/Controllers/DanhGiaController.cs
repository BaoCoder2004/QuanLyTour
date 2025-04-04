using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyTour.Models;
using X.PagedList;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyTour.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<DanhGiaController> _logger;

        public DanhGiaController(ILogger<DanhGiaController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Hiển thị danh sách đánh giá
        public IActionResult Index(string sort, int page = 1, int pageSize = 3)
        {
            try
            {
                var reviews = GetReviewsFromDatabase();

                var sortedReviews = reviews.AsQueryable();

                switch (sort)
                {
                    case "Đánh giá cao nhất":
                        sortedReviews = sortedReviews.OrderByDescending(r => r.Rating);
                        break;
                    case "Đánh giá thấp nhất":
                        sortedReviews = sortedReviews.OrderBy(r => r.Rating);
                        break;
                    default:
                        sortedReviews = sortedReviews.OrderByDescending(r => r.ReviewDate);
                        break;
                }

                var paginatedReviews = sortedReviews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentSort = sort;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)reviews.Count / pageSize);

                return View(paginatedReviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đánh giá");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải đánh giá. Vui lòng thử lại sau.";
                return View(new List<Review>());
            }
        }

        // Hiển thị form đánh giá
        [HttpPost]
        public IActionResult SubmitReview(Review review, IFormFile[] Images, int maTour)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    if (string.IsNullOrEmpty(userName))
                    {
                        TempData["ErrorMessage"] = "Vui lòng đăng nhập để đánh giá";
                        return RedirectToAction("Login", "Account");
                    }

                    review.UserName = userName;
                    review.ReviewDate = DateTime.Now;
                    review.HelpfulCount = 0;

                    // Xử lý ảnh
                    if (Images != null && Images.Length > 0)
                    {
                        review.ImageUrls = new List<string>();
                        foreach (var image in Images.Take(5)) // Giới hạn 5 ảnh
                        {
                            if (image != null && image.Length > 0)
                            {
                                var imagePath = SaveImage(image);
                                review.ImageUrls.Add(imagePath);
                            }
                        }
                    }

                    // Lấy MaPhieu từ bảng PhieuDatTour
                    int maPhieu = GetMaPhieuFromTour(maTour, userName);

                    if (maPhieu == 0)
                    {
                        TempData["ErrorMessage"] = "Bạn chưa đặt tour này nên không thể đánh giá";
                        return RedirectToAction("ChiTietTour", "Tour", new { maTour });
                    }

                    //review.MaPhieu = maPhieu;

                    // Lưu vào database
                    SaveReviewToDatabase(review);

                    TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi thành công!";
                    return RedirectToAction("ChiTietTour", "Tour", new { maTour });
                }

                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi đánh giá");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi đánh giá. Vui lòng thử lại.";
                return RedirectToAction("ChiTietTour", "Tour", new { maTour });
            }
        }

        private int GetMaPhieuFromTour(int maTour, string userName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT TOP 1 p.MaPhieu 
                       FROM PhieuDatTour p
                       INNER JOIN NguoiDung n ON p.MaNguoiDung = n.MaNguoiDung
                       WHERE p.MaTour = @MaTour AND n.TenDangNhap = @UserName
                       ORDER BY p.NgayDatTour DESC";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MaTour", maTour);
                    command.Parameters.AddWithValue("@UserName", userName);

                    var result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        [HttpPost]
        public IActionResult LikeReview(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE Reviews SET HelpfulCount = HelpfulCount + 1 WHERE Id = @Id";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }

                // Lấy số lượt thích mới
                var newCount = GetHelpfulCount(id);
                return Json(new { likes = newCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi like đánh giá ID {id}");
                return Json(new { error = "Có lỗi xảy ra" });
            }
        }

        private List<Review> GetReviewsFromDatabase()
        {
            var reviews = new List<Review>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT Id, UserName, Rating, Title, Content, ReviewDate, HelpfulCount FROM Reviews";

                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new Review
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            Rating = reader.GetInt32(2),
                            Title = reader.GetString(3),
                            Content = reader.GetString(4),
                            ReviewDate = reader.GetDateTime(5),
                            HelpfulCount = reader.GetInt32(6),
                            // ImageUrls cần được lấy từ bảng khác nếu có
                        });
                    }
                }

                // Lấy ảnh cho từng review nếu cần
                foreach (var review in reviews)
                {
                    review.ImageUrls = GetReviewImages(review.Id, connection);
                }
            }

            return reviews;
        }

        private List<string> GetReviewImages(int reviewId, SqlConnection connection)
        {
            var images = new List<string>();
            string sql = "SELECT ImageUrl FROM ReviewImages WHERE ReviewId = @ReviewId";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ReviewId", reviewId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        images.Add(reader.GetString(0));
                    }
                }
            }

            return images;
        }

        private void SaveReviewToDatabase(Review review)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Bắt đầu transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Thêm review chính
                        string sql = @"INSERT INTO Reviews (UserName, Rating, Title, Content, ReviewDate, HelpfulCount)
                                    VALUES (@UserName, @Rating, @Title, @Content, @ReviewDate, @HelpfulCount);
                                    SELECT SCOPE_IDENTITY();";

                        int reviewId;
                        using (var command = new SqlCommand(sql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserName", review.UserName);
                            command.Parameters.AddWithValue("@Rating", review.Rating);
                            command.Parameters.AddWithValue("@Title", review.Title);
                            command.Parameters.AddWithValue("@Content", review.Content);
                            command.Parameters.AddWithValue("@ReviewDate", review.ReviewDate);
                            command.Parameters.AddWithValue("@HelpfulCount", review.HelpfulCount);

                            reviewId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Thêm ảnh nếu có
                        if (review.ImageUrls != null && review.ImageUrls.Any())
                        {
                            foreach (var imageUrl in review.ImageUrls)
                            {
                                string imageSql = "INSERT INTO ReviewImages (ReviewId, ImageUrl) VALUES (@ReviewId, @ImageUrl)";
                                using (var command = new SqlCommand(imageSql, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@ReviewId", reviewId);
                                    command.Parameters.AddWithValue("@ImageUrl", imageUrl);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private string SaveImage(IFormFile image)
        {
            // Triển khai logic lưu ảnh vào thư mục và trả về đường dẫn
            // Ví dụ:
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "Images", "reviews");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            return "/Content/Images/reviews/" + uniqueFileName;
        }

        private int GetHelpfulCount(int reviewId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT HelpfulCount FROM Reviews WHERE Id = @Id";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", reviewId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}