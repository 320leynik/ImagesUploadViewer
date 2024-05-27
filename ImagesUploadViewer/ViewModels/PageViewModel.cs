using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesUploadViewer.ViewModels
{
    public class PageViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public PageViewModel(int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling((double)count / pageSize);
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
