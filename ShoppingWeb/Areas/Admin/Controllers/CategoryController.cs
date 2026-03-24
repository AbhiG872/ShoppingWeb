using Microsoft.AspNetCore.Mvc;
using Shopping_DataAccess.Repository.IRepository;
using ShoppingWeb.Models;

namespace ShoppingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var list = _unitOfWork.Category.GetAll().ToList();
            return View(list);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var list = _unitOfWork.Category.Get(c => c.Id == id);
            return View(list);
        }
        [HttpPost]
        public IActionResult Edit(int id, Category obj)
        {
            if (id != obj.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Details(int id)
        {
            var list = _unitOfWork.Category.Get(c => c.Id == id);
            return View(list);
        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categorylist = _unitOfWork.Category.Get(c => c.Id == id);
            if (categorylist == null)
            {
                return NotFound();
            }
            return View(categorylist);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Category? obj = _unitOfWork.Category.Get(c => c.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
