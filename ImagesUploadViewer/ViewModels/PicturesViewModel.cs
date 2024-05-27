using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using ImagesUploadViewer.Models;

namespace ImagesUploadViewer.ViewModels
{
    public class PicturesViewModel
    {
        public List<Picture> Pictures { get; set; }
        public string SearchPattern { get; set; }
        public PageViewModel Paginator { get; set; }
    }
}
