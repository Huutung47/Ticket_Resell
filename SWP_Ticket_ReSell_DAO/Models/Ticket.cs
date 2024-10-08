﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class Ticket
{
    public int ID_Ticket { get; set; }

    public int? Seller { get; set; }

    public int? Buyer { get; set; }

    public decimal? Price { get; set; }

    public string Ticket_category { get; set; }

    public bool? Ticket_type { get; set; }

    public int? Quantity { get; set; }

    public DateTime? Ticket_History { get; set; }

    public string Status { get; set; }

    public DateTime? Event_Date { get; set; }

    public string Show_Name { get; set; }

    public string Location { get; set; }

    public string Description { get; set; }

    public int? Seat { get; set; }

    public int? Ticketsold { get; set; }

    public string Image { get; set; }

    public virtual ICollection<Boxchat> Boxchats { get; set; } = new List<Boxchat>();

    public virtual Customer BuyerNavigation { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual Customer SellerNavigation { get; set; }
}