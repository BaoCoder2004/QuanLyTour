using X.PagedList;

namespace QuanLyTour.Models.VeTau
{
    public class VeTauSearchViewModel
    {
        public IPagedList<VeTauViewModel> VeTaus { get; set; }
        public string Keyword { get; set; }
    }
}
