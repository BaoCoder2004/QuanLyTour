using System.ComponentModel.DataAnnotations;

namespace QuanLyTour.Models
{
	public class HoTro
	{
		public int Id { get; set; }

		public int MaNguoiDung { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập nội dung hỗ trợ.")]
		public string ? Message { get; set; }

		[DataType(DataType.Date)]
		public DateTime NgayTao { get; set; } = DateTime.Now;
	}
}
