using X.PagedList;
namespace QuanLyTour.Models
{
    public class DetailViewModel
    {
        public TourViewModel TourHienTai { get; set; }
        public IPagedList<TourViewModel> TourTuongTu { get; set; }
    }
}
