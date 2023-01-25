﻿using CorvoBooks.DataAccess.Data;
using CorvoBooks.DataAccess.Repository;
using CorvoBooks.DataAccess.Repository.IRepository;
using CorvoBooks.Models;
using CorvoBooks.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using CorvoBooks.Utility;
using Microsoft.AspNetCore.Authorization;

namespace CorvoBooksWeb.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize(Roles = SD.Role_Admin)]
  public class CompanyController : Controller
  {
    private readonly IUnitOfWork _unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
      return View();
    }

    // GET
    public IActionResult Upsert(int? id)
    {
      Company company = new();

      if (id == null || id == 0)
      {
        // Create Company
        return View(company);
      }
      else
      {
        // Update Company
        company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
        return View(company);
      }
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(Company obj)
    {
      if (ModelState.IsValid)
      {
        if (obj.Id == 0)
        {
          _unitOfWork.Company.Add(obj);
          TempData["success"] = "Company created successfully.";
        }
        else
        {
          _unitOfWork.Company.Update(obj);
          TempData["success"] = "Company updated successfully.";
        }

        _unitOfWork.Save();

        return RedirectToAction("index");
      }
      return View(obj);
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
      var companyList = _unitOfWork.Company.GetAll();
      return Json(new { data = companyList });
    }

    // POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
      var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
      if (obj == null)
      {
        return Json(new { success = false, message = "Error while deleting" });
      }

      _unitOfWork.Company.Remove(obj);
      _unitOfWork.Save();

      return Json(new { success = true, message = "Delete Successful" });
    }

    #endregion
  }
}
