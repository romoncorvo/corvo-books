using CorvoBooks.DataAccess.Data;
using CorvoBooks.DataAccess.Repository.IRepository;
using CorvoBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CorvoBooks.DataAccess.Repository
{
  public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
  {
    private ApplicationDbContext _db;
    public OrderHeaderRepository(ApplicationDbContext db) : base(db)
    {
      _db = db;
    }
    public void Update(OrderHeader obj)
    {
      _db.OrderHeaders.Update(obj);
    }

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
      var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
      if (orderFromDb != null)
      {
        orderFromDb.OrderStatus = orderStatus;
        if (paymentStatus != null)
        {
          orderFromDb.PaymentStatus = paymentStatus;
        }
      }
    }

    public void UpdateStripePaymentId(int id, string sessionId, string PaymentIntentStatus)
    {
      var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
      
      orderFromDb.PaymentDate = DateTime.Now;
      orderFromDb.SessionId = sessionId;
      orderFromDb.PaymentIntentStatus = PaymentIntentStatus;
    }
  }
}
