using X.PagedList;
namespace QuanLyTour.Models
{
    public class TourSearchViewModel
    {
        public IPagedList<TourViewModel> Tours { get; set; }
        public string Keyword { get; set; }
    }
}
