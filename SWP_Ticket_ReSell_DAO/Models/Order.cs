﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class Order
{
    public int ID_Order { get; set; }

    public int? ID_Customer { get; set; }

    public string Payment_method { get; set; }

    public string Status { get; set; }

    public DateTime? Shipping_time { get; set; }

    public DateTime? Create_At { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? Update_At { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Customer ID_CustomerNavigation { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}