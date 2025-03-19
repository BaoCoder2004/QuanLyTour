
using QuanLyTour.Models.Tour;
using QuanLyTour.Models.KhachSan;
namespace QuanLyTour.Models
{
    public class ViewModel
    {
        public List<LoaiTour> LoaiTourView { get; set; }
        public List<TourViewModel> TourView { get; set; }

		public List<LoaiKhachSan> LoaiKhachSanView { get; set; }
		public List<KhachSanViewModel> KhachSanView { get; set; }
	}
}
