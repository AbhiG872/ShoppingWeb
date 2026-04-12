using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shopping_DataAccess.Repository.IRepository;
using Shopping_Models;
using Shopping_Models.ViewModels;
using Shopping_Utility;
using System.Diagnostics;

namespace ShoppingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product").ToList()
            };
            return View(orderVM);
        }


        #region APICall
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeader = _unitOfWork.OrderHeader.
                GetAll(includeProperties: "ApplicationUser").ToList();


            switch (status)
            {
                case "pending":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeader = orderHeader.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;

                case "completed":
                    orderHeader = orderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeader = orderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
               
                default:
                    
                    break;
           
        }


            return Json(new { data = orderHeader });
        }
       
        #endregion
    }
}
