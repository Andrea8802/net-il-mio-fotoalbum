using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net_il_mio_fotoalbum.Models;
using System.Drawing;

namespace net_il_mio_fotoalbum.Controllers
{
    public class PhotoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            using (PhotoContext db = new PhotoContext())
            {
                List<Photo> photos = db.Photos.Where(ph => ph.Visibility == true).ToList();

                List<string> imagesData = new List<string>();

                foreach (Photo photo in photos)
                {
                    imagesData.Add(Convert.ToBase64String(photo.Image));
                }


                PhotoFormModel model = new PhotoFormModel();

                model.ListPhotos = photos;
                model.ListImages = imagesData;

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Details(long id)
        {
            using (PhotoContext db = new PhotoContext())
            {
                Photo photo = db.Photos.Where(ph => ph.Id == id).Include(ph => ph.Categories).FirstOrDefault();

                if (photo == null)
                {
                    return View("Error", "La foto cercata non esiste");
                }

                string imagesData = Convert.ToBase64String(photo.Image);

                PhotoFormModel model = new PhotoFormModel();

                model.Photo = photo;
                model.Image = imagesData;

                return View("Details", model);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            using (PhotoContext db = new PhotoContext())
            {
                PhotoFormModel model = new PhotoFormModel();
                List<Category> categories = db.Categories.ToList();

                List<SelectListItem> listCategories = new List<SelectListItem>();

                foreach (Category category in categories)
                {
                    listCategories.Add(new SelectListItem
                    {
                        Text = category.Name,
                        Value = category.Id.ToString()
                    });
                }

                Photo photo = new Photo();
                photo.Visibility = true;

                model.Categories = listCategories;
                model.Photo = photo;
                return View("Create", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PhotoFormModel model)
        {
            if (!ModelState.IsValid)
            {
                using (PhotoContext db = new PhotoContext())
                {
                    List<Category> categories = db.Categories.ToList();

                    List<SelectListItem> listCategories = new List<SelectListItem>();

                    foreach (Category category in categories)
                    {
                        listCategories.Add(new SelectListItem
                        {
                            Text = category.Name,
                            Value = category.Id.ToString()
                        });
                    }

                    Photo photo = new Photo();
                    photo.Visibility = true;

                    model.Categories = listCategories;
                    model.Photo = photo;
                    return View(model);
                }
            }

            using (PhotoContext db = new PhotoContext())
            {


                if (model.ImageFile == null)
                    return View("Error", "Immagine non selezionata");

                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(model.ImageFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)model.ImageFile.Length);
                }


                if (model.SelectedCategories == null)
                    return View("Error", "Categorie non selezionate");

                List<Category> categories = new List<Category>();
                foreach (var categoryid in model.SelectedCategories)
                {
                    int intcategoryid = int.Parse(categoryid);
                    var category = db.Categories.Where(c => c.Id == intcategoryid).FirstOrDefault();
                    categories.Add(category);
                }

                Photo photo = new Photo()
                {
                    Title = model.Photo.Title,
                    Description = model.Photo.Description,
                    Visibility = model.Photo.Visibility,
                    Image = imageData,
                    Categories = categories
                };

                db.Photos.Add(photo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        public IActionResult Update(long id)
        {
            using (PhotoContext db = new PhotoContext())
            {
                Photo photo = db.Photos.FirstOrDefault(ph => ph.Id == id);

                if (photo == null)
                {
                    return NotFound();
                }

                PhotoFormModel model = new PhotoFormModel();
                List<Category> categories = db.Categories.ToList();

                List<SelectListItem> listCategories = new List<SelectListItem>();

                photo.Categories = new List<Category>();
                foreach (Category category in categories)
                {
                    listCategories.Add(new SelectListItem
                    {
                        Text = category.Name,
                        Value = category.Id.ToString(),
                        Selected = photo.Categories.Any(c => c.Id == category.Id)
                    });
                }

                model.Categories = listCategories;
                model.Photo = photo;
                model.Image = Convert.ToBase64String(photo.Image);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(long id, PhotoFormModel model)
        {
            if (!ModelState.IsValid)
            {
                using (PhotoContext db = new PhotoContext())
                {
                    Photo photo = db.Photos.FirstOrDefault(ph => ph.Id == id);

                    if (photo == null)
                    {
                        return NotFound();
                    }

                    List<Category> categories = db.Categories.ToList();

                    List<SelectListItem> listCategories = new List<SelectListItem>();

                    photo.Categories = new List<Category>();
                    foreach (Category category in categories)
                    {
                        listCategories.Add(new SelectListItem
                        {
                            Text = category.Name,
                            Value = category.Id.ToString(),
                            Selected = photo.Categories.Any(c => c.Id == category.Id)
                        });
                    }

                    model.Categories = listCategories;
                    model.Photo = photo;
                    model.Image = Convert.ToBase64String(photo.Image);

                    return View(model);
                }
            }

            using (PhotoContext db = new PhotoContext())
            {
                Photo photo = db.Photos
                    .Include(ph => ph.Categories)
                    .FirstOrDefault(ph => ph.Id == id);

                if (photo == null)
                {
                    return NotFound();
                }

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    byte[] imageData = null;
                    using (var binaryReader = new BinaryReader(model.ImageFile.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)model.ImageFile.Length);
                    }

                    photo.Image = imageData;
                }

                photo.Title = model.Photo.Title;
                photo.Description = model.Photo.Description;
                photo.Visibility = model.Photo.Visibility;

                if (model.SelectedCategories != null && model.SelectedCategories.Any())
                {
                    photo.Categories.Clear();

                    foreach (var categoryId in model.SelectedCategories)
                    {
                        int intCategoryId = int.Parse(categoryId);
                        Category category = db.Categories.FirstOrDefault(c => c.Id == intCategoryId);
                        if (category != null)
                        {
                            photo.Categories.Add(category);
                        }
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            using (PhotoContext db = new PhotoContext())
            {
                Photo photo = db.Photos.Where(ph => ph.Id == id).FirstOrDefault();

                if (photo == null)
                    return View("Error", "La foto cercata non esiste");

                db.Photos.Remove(photo);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
        }
    }
}
