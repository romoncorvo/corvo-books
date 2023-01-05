﻿using CorvoBooks.DataAccess.Data;
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
  public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
  {
    private ApplicationDbContext _db;
    public ShoppingCartRepository(ApplicationDbContext db) : base(db)
    {
      _db = db;
    }

    public int DecrementCount(ShoppingCart shoppingCart, int count)
    {
      shoppingCart.Count -= count;
      return shoppingCart.Count;
    }

    public int IncrementCount(ShoppingCart shoppingCart, int count)
    {
      shoppingCart.Count += count;
      return shoppingCart.Count;
    }
  }
}
