using CorvoBooks.DataAccess.Repository.IRepository;
using CorvoBooks.Models;
using CorvoBooks.Models.ViewModels;
using CorvoBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace CorvoBooksWeb.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize]
  public class OrderController : Controller
  {
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public OrderVM OrderVM { get; set; }
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
      OrderVM = new OrderVM()
      {
        OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
        OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
      };
      return View(OrderVM);
    }

    [ActionName("Details")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Details_Pay_Now()
    {
      OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
      OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

      // Stripe Settings for Customer
      var domain = "https://localhost:44384/";
      var options = new SessionCreateOptions
      {
        PaymentMethodTypes = new List<string>
                {
                  "card",
                },
        LineItems = new List<SessionLineItemOptions>(),
        Mode = "payment",
        SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
        CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
      };

      foreach (var item in OrderVM.OrderDetail)
      {
        var SessionLineItem = new SessionLineItemOptions
        {
          PriceData = new SessionLineItemPriceDataOptions
          {
            UnitAmount = (long)(item.Price * 100),
            Currency = "usd",
            ProductData = new SessionLineItemPriceDataProductDataOptions
            {
              Name = item.Product.Title,
            },
          },
          Quantity = item.Count,
        };
        options.LineItems.Add(SessionLineItem);
      }

      var service = new SessionService();
      Session session = service.Create(options);
      _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
      _unitOfWork.Save();

      Response.Headers.Add("Location", session.Url);
      return new StatusCodeResult(303);
    }

    public IActionResult PaymentConfirmation(int orderHeaderid)
    {
      OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderHeaderid);

      if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
      {
        var service = new SessionService();
        Session session = service.Get(orderHeader.SessionId);

        //Check stripe status
        if (session.PaymentStatus.ToLower() == "paid")
        {
          _unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
          _unitOfWork.Save();
        }
      }

      return View(orderHeaderid);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateOrderDetail()
    {
      var orderHearderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
      orderHearderFromDb.Name = OrderVM.OrderHeader.Name;
      orderHearderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
      orderHearderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
      orderHearderFromDb.City = OrderVM.OrderHeader.City;
      orderHearderFromDb.State = OrderVM.OrderHeader.State;
      orderHearderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
      if (OrderVM.OrderHeader.Carrier != null)
      {
        orderHearderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
      }
      if (OrderVM.OrderHeader.TrackingNumber != null)
      {
        orderHearderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
      }
      _unitOfWork.OrderHeader.Update(orderHearderFromDb);
      _unitOfWork.Save();
      TempData["Success"] = "Order Details Updated Successfully.";
      return RedirectToAction("Details", "Order", new { orderId = orderHearderFromDb.Id });
    }



    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult StartProcessing()
    {
      _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
      _unitOfWork.Save();
      TempData["Success"] = "Order Status Updated Successfully.";
      return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult ShipOrder()
    {
      var orderHearderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
      orderHearderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
      orderHearderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
      orderHearderFromDb.OrderStatus = SD.StatusShipped;
      orderHearderFromDb.ShippingDate = DateTime.Now;
      if (orderHearderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
      {
        orderHearderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
      }

      _unitOfWork.OrderHeader.Update(orderHearderFromDb);
      _unitOfWork.Save();
      TempData["Success"] = "Order Shipped Successfully.";
      return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult CancelOrder()
    {
      var orderHearderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
      if (orderHearderFromDb.PaymentStatus == SD.PaymentStatusApproved)
      {
        var options = new RefundCreateOptions
        {
          Reason = RefundReasons.RequestedByCustomer,
          PaymentIntent = orderHearderFromDb.PaymentIntentStatus,
        };

        var service = new RefundService();
        Refund refund = service.Create(options);

        _unitOfWork.OrderHeader.UpdateStatus(orderHearderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded); ;
      }
      else
      {
        _unitOfWork.OrderHeader.UpdateStatus(orderHearderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled); ;

      }
      
      _unitOfWork.Save();
      TempData["Success"] = "Order Cancelled Successfully.";
      return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll(string status)
    {
      IEnumerable<OrderHeader> orderHeaders;
      if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
      {
        orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
      }
      else
      {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
      }


      switch (status)
      {
        case "pending":
          orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
          break;
        case "inprocess":
          orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
          break;
        case "completed":
          orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
          break;
        case "approved":
          orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
          break;
        default:
          break;
      }

      return Json(new { data = orderHeaders });
    }
    #endregion
  }
}
