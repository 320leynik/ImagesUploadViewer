using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ImagesUploadViewer.Data;
using ImagesUploadViewer.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using ImagesUploadViewer.ViewModels;
using System.Text.RegularExpressions;
using ImageMagick;

namespace ImagesUploadViewer.Controllers
{
    public class PicturesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private IWebHostEnvironment _env;

        private readonly string[] permittedExtensions = new string[]
        {
            ".jpg",".jpeg",".png",".bmp",".gif",".ico"
        };

        private Random rnd;

        public PicturesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            rnd = new Random();
        }

        [HttpPost]
        public JsonResult GetPictureData(int id)
        {
            var picture = _context.Pictures.FirstOrDefault(p => p.Id == id);
            if (picture != null)
            {
                return Json(new
                {
                    success = true,
                    filename = picture.Filename,
                    fileext = Path.GetExtension(picture.FilePath),
                    filesize = picture.Filesize,
                    loadedat = picture.LoadeDat,
                    filepath = picture.FilePath
                });
            }
            return Json(new { success = false });
        }

        // Create Filepath For Compressed Picture
        private async Task CreateCompressedPicture(string uploadFile)
        {
            string compressedPath = "";
            if (!string.IsNullOrWhiteSpace(uploadFile))
            {
                uploadFile = _env.WebRootPath + uploadFile;
                string ext = Path.GetExtension(uploadFile);
                if (permittedExtensions.Contains(ext))
                {
                    compressedPath = _env.WebRootPath + "/files/compressed/" + Path.GetFileName(uploadFile);
                    Directory.CreateDirectory(Path.GetDirectoryName(compressedPath));
                    if (ext == ".gif")
                    {
                        using var collection = new MagickImageCollection(uploadFile);
                        collection.Coalesce();
                        foreach (var image in collection)
                        {
                            image.Resize(125, 125);
                        }
                        await collection.WriteAsync(compressedPath);
                    }
                    else
                    {
                        using var image = new MagickImage(uploadFile);
                        var size = new MagickGeometry(125, 125);
                        size.IgnoreAspectRatio = true;
                        image.Resize(size);

                        await image.WriteAsync(compressedPath);
                    }
                }
            }
        }

        // Get Compressed Pictures
        private string GetCompressedPicture(string picturePath)
        {
            string name = Path.GetFileName(picturePath);

            string compressed = "/files/compressed/" + name;
            if(System.IO.File.Exists(_env.WebRootPath + compressed))
            {
                return compressed;
            }
            return picturePath;
        }


        // GET: Pictures
        public async Task<IActionResult> Gallery(int pageNumber = 1, string searchPattern = "")
        {
            int pageSize = 4;
            var pictures = await _context.Pictures.ToListAsync();

            // Вибір файлів за паттерном імені
            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                if (!searchPattern.Contains('.'))
                {
                    string regex = "^" + Regex.Escape(searchPattern).Replace("\\*", ".*") + "$";
                    pictures = pictures.Where(p => Regex.IsMatch(p.Filename, regex)).ToList();
                }
                else
                {
                    string ext = Path.GetExtension(searchPattern);
                    string filename = Path.GetFileNameWithoutExtension(searchPattern);

                    string regex = "^" + Regex.Escape(filename).Replace("\\*", ".*") + "$";
                    string extRegex = "^" + Regex.Escape(ext).Replace("\\*", ".*").Replace("\\.", @"\.") + "$";
                    pictures = pictures.Where(p =>
                        Regex.IsMatch(p.Filename, regex) &&
                        (Regex.IsMatch(Path.GetExtension(p.FilePath), extRegex))
                    ).ToList();
                }

            }

            int count = pictures.Count();
            var items = pictures.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Превью файлів показуються у компресованному вигляді задля швидкості завантаження
            foreach (var item in items)
            {
                item.FilePath = GetCompressedPicture(item.FilePath);
            }

            PicturesViewModel viewModel = new()
            {
                Pictures = items,
                Paginator = new PageViewModel(count, pageNumber, pageSize),
                SearchPattern = searchPattern
            };

            // Якщо відбувся виклик для пагінації
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PagePartialView", viewModel);
            }

            ViewBag.Current = "PictureGallery";
            return View(viewModel);
        }

        // GET: Pictures/Upload
        [Route("Upload")]
        public IActionResult Upload()
        {
            ViewBag.Current = "PictureUpload";
            return View();
        }

        // POST: Pictures/Upload
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Upload")]
        public async Task<IActionResult> Upload([Bind("Id,Filename,Filesize,FilePath")] Picture picture, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                // Перевіряємо чи є файл
                if (uploadFile != null)
                {
                    string name = uploadFile.FileName;
                    var ext = Path.GetExtension(name);

                    // Перевіряємо підходяще розширення файлу та назву файлу
                    if (permittedExtensions.Contains(ext) && !string.IsNullOrWhiteSpace(picture.Filename) && picture.Filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                    {
                        DateTime now = DateTime.UtcNow;

                        string filename = picture.Filename;
                        filename = filename.Length > 25 ? filename.Substring(0, 25) : filename;
                        picture.Filename = filename;

                        string fullName = now.Ticks + rnd.Next(10000) + filename + ext;
                        string path = $"/files/original/{fullName}";
                        string serverPath = _env.WebRootPath + path;
                        if (System.IO.File.Exists(serverPath))
                        {
                            return View(picture);
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(serverPath)); // На випадок якщо не існує такої папки (якщо існує, то нічого не відбувається)
                        using (FileStream fs = new FileStream(serverPath, FileMode.Create, FileAccess.Write))
                        {
                            await uploadFile.CopyToAsync(fs);
                        }
                        await CreateCompressedPicture(path); // Додатково створити скомпресовані варіанти графічних файлів
                        // *
                        long fileSize = uploadFile.Length;
                        // * Незважаючи на вже надані дані розміру, ще раз їх перезапишемо задля уникнення можливих помилок
                        picture.Filesize = fileSize;

                        picture.LoadeDat = now;
                        picture.FilePath = path;

                        _context.Add(picture);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Gallery));
                    }
                }
            }

            ViewBag.Current = "PictureUpload";
            return View(picture);
        }
    }
}
