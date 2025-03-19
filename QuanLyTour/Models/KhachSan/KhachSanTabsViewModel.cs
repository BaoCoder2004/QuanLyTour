using X.PagedList;

namespace QuanLyTour.Models.KhachSan
{
    public class KhachSanTabsViewModel
    {
		public IPagedList<KhachSanViewModel> KhachSanTrongNuoc { get; set; }
		public IPagedList<KhachSanViewModel> KhachSanNgoai { get; set; }
	}
}
