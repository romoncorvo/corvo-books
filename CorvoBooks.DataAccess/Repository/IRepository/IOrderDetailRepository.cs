using CorvoBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvoBooks.DataAccess.Repository.IRepository
{
  public interface IOrderDetailRepository : IRepository<OrderDetail>
  {
    void Update(OrderDetail obj);
  }
}
