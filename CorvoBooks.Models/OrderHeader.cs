﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorvoBooks.Models
{
  public class OrderHeader
  {
    public int Id { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }
    public DateTime ShippingDate { get; set; }
    public double OrderTotal { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime PaymentDueDate { get; set; }

    public string? SessionId { get; set; }
    public string? PaymentIntentStatus { get; set; }

    [Required]
    public string Name { get; set; }
    [Required]
    public string StreetAddress { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string State { get; set; }
    [Required]
    public string PostalCode { get; set; }
    [Required]
    public string PhoneNumber { get; set; }



    public string ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; }
  }
}