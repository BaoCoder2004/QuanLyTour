using QuanLyTour.Models.KhachSan;
using QuanLyTour.Models.Tour;
using X.PagedList;
namespace QuanLyTour.Models
{
    public class DetailViewModel
    {
        public TourViewModel TourHienTai { get; set; }
        public IPagedList<TourViewModel> TourTuongTu { get; set; }
        public IPagedList<Review> Reviews { get; set; } // Add this property
        public Dictionary<int, int> RatingStats { get; set; }
    }
}
