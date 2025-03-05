using X.PagedList;
namespace QuanLyTour.Models
{
    public class TourTabsViewModel
    {
        public IPagedList<TourViewModel> TourTrongNuoc { get; set; }
        public IPagedList<TourViewModel> TourNuocNgoai { get; set; }
    }
}
