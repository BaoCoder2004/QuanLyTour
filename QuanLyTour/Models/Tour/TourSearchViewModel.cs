using X.PagedList;
namespace QuanLyTour.Models.Tour
{
    public class TourSearchViewModel
    {
        public IPagedList<TourViewModel> Tours { get; set; }
        public string Keyword { get; set; }
    }
}
