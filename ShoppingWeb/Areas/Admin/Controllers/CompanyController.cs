using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_DataAccess.Repository.IRepository;
using Shopping_Models;

namespace ShoppingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var list = _unitOfWork.Company.GetAll().ToList();
            return View(list);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Company obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Company Created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var list = _unitOfWork.Company.Get(c => c.Id == id);
            return View(list);
        }
        [HttpPost]
        public IActionResult Edit(int id, Company obj)
        {
            if (id != obj.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Company Updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        public IActionResult Details(int id)
        {
            var list = _unitOfWork.Company.Get(c => c.Id == id);
            return View(list);
        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company? categorylist = _unitOfWork.Company.Get(c => c.Id == id);
            if (categorylist == null)
            {
                return NotFound();
            }
            return View(categorylist);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Company? obj = _unitOfWork.Company.Get(c => c.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Company Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
