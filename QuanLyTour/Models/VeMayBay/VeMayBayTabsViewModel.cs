using System.Collections.Generic;
using X.PagedList;

namespace QuanLyTour.Models.VeMayBay
{
    public class VeMayBayTabsViewModel
    {
        public IPagedList<VeMayBayViewModel> VeMayBayNoiDia { get; set; }
        public IPagedList<VeMayBayViewModel> VeMayBayQuocTe { get; set; }
    }
}

