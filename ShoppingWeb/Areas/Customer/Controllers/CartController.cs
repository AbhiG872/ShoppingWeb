using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_DataAccess.Repository.IRepository;
using Shopping_Models;
using Shopping_Models.ViewModels;
using Shopping_Utility;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace ShoppingWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()
        {

          
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [Authorize]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [Authorize]
        [ActionName("Summary")]
       
        public IActionResult SummaryPOST()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get Cart
            ShoppingCartVM.ShoppingCartList = _unitofwork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product"
            );

            // Order Header
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitofwork.ApplicationUser.Get(u => u.Id == userId);

            // Calculate Total
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            // Payment Logic
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            // Save Order Header
            _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofwork.Save();

            // Save Order Details (optimized)
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitofwork.OrderDetail.Add(orderDetail);
            }
            _unitofwork.Save();

            // ================= STRIPE PAYMENT =================
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // ✅ Minimum amount validation (Stripe fix)
                if (ShoppingCartVM.OrderHeader.OrderTotal < 40)
                {
                    TempData["error"] = "Minimum order amount should be ₹40";
                    return RedirectToAction("Summary");
                }

                var domain = $"{Request.Scheme}://{Request.Host}/"; // ✅ dynamic
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // ₹ to paise
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                // Save Stripe IDs
                _unitofwork.OrderHeader.UpdateStripPaymentId(
                    ShoppingCartVM.OrderHeader.Id,
                    session.Id,
                    session.PaymentIntentId
                );
                _unitofwork.Save();

                // Redirect to Stripe
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            // Company Order (No Stripe)
            return RedirectToAction(nameof(OrderConfirmation),
                new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            var order = _unitofwork.OrderHeader.Get(u => u.Id == id);

            if (order.PaymentStatus != SD.PaymentStatusApproved)
            {
                var service = new SessionService();
                var session = service.Get(order.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofwork.OrderHeader.UpdateStatus(
                        id,
                        SD.StatusApproved,
                        SD.PaymentStatusApproved
                    );
                    _unitofwork.Save();
                }
            }

            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cartfromDB = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartfromDB == null)
            {
                return NotFound();
            }

            // 🔹 Increase quantity
            cartfromDB.Count += 1;

            _unitofwork.ShoppingCart.Update(cartfromDB);
            _unitofwork.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartfromDB = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartfromDB == null)
            {
                return NotFound();
            }
            HttpContext.Session.SetInt32(SD.SessionCart, _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartfromDB.ApplicationUserId).Count() - 1);

            if (cartfromDB.Count <= 1)
            {
                _unitofwork.ShoppingCart.Remove(cartfromDB);
            }
            else
            {
                cartfromDB.Count -= 1;
                _unitofwork.ShoppingCart.Update(cartfromDB);
            }

            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartfromDB = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartfromDB == null)
            {
                return NotFound();
            }
            _unitofwork.ShoppingCart.Remove(cartfromDB);
             HttpContext.Session.SetInt32(SD.SessionCart, _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartfromDB.ApplicationUserId).Count() - 1);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceBasedQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

    }
}
