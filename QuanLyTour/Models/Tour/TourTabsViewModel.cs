using X.PagedList;
namespace QuanLyTour.Models.Tour
{
    public class TourTabsViewModel
    {
        public IPagedList<TourViewModel> TourTrongNuoc { get; set; }
        public IPagedList<TourViewModel> TourNuocNgoai { get; set; }
    }
}