using X.PagedList;
using QuanLyTour.Models.KhachSan;

namespace QuanLyTour.Models.KhachSan
{
    public class KhachSanSearchViewModel
    {
        public IPagedList<KhachSanViewModel> KhachSans { get; set; }
        public string Keyword { get; set; }
    }
}
