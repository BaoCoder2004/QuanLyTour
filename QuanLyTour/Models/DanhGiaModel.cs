using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTour.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; } // Tên người đánh giá

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // Số sao

        [Required]
        public string Title { get; set; } // Tiêu đề đánh giá

        [Required]
        public string Content { get; set; } // Nội dung đánh giá

        public DateTime ReviewDate { get; set; } = DateTime.Now; // Ngày đánh giá

        public List<string> ImageUrls { get; set; } // Danh sách ảnh đánh giá

        public int HelpfulCount { get; set; } = 0; // Số lượt thích
    }
}
