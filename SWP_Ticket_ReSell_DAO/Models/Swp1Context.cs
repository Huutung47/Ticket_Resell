﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SWP_Ticket_ReSell_DAO.Models;

public partial class swp1Context : DbContext
{
    public swp1Context(DbContextOptions<swp1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Boxchat> Boxchats { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boxchat>(entity =>
        {
            entity.HasKey(e => e.ID_Boxchat).HasName("PK__Boxchat__23A1BC0650E5EBAF");

            entity.ToTable("Boxchat");

            entity.Property(e => e.Chat_content).HasColumnType("text");

            entity.HasOne(d => d.Buyer).WithMany(p => p.BoxchatBuyers)
                .HasForeignKey(d => d.Buyer_ID)
                .HasConstraintName("FK__Boxchat__Buyer_I__4AB81AF0");

            entity.HasOne(d => d.ID_TicketNavigation).WithMany(p => p.Boxchats)
                .HasForeignKey(d => d.ID_Ticket)
                .HasConstraintName("FK__Boxchat__ID_Tick__48CFD27E");

            entity.HasOne(d => d.Seller).WithMany(p => p.BoxchatSellers)
                .HasForeignKey(d => d.Seller_ID)
                .HasConstraintName("FK__Boxchat__Seller___49C3F6B7");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.ID_Customer).HasName("PK__Customer__2D8FDE5F429F1FB2");

            entity.ToTable("Customer");

            entity.Property(e => e.Avatar).IsUnicode(false);
            entity.Property(e => e.Average_feedback).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Contact)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EmailConfirm)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.IsActive)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Method_login)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Package_expiration_date).HasColumnType("datetime");
            entity.Property(e => e.Package_registration_time).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_PackageNavigation).WithMany(p => p.Customers)
                .HasForeignKey(d => d.ID_Package)
                .HasConstraintName("FK__Customer__ID_Pac__3C69FB99");

            entity.HasOne(d => d.ID_RoleNavigation).WithMany(p => p.Customers)
                .HasForeignKey(d => d.ID_Role)
                .HasConstraintName("FK__Customer__ID_Rol__3B75D760");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.ID_Feedback).HasName("PK__Feedback__7CA05C3F8CD3EDC1");

            entity.ToTable("Feedback");

            entity.Property(e => e.Comment).HasColumnType("text");
            entity.Property(e => e.History).HasColumnType("datetime");

            entity.HasOne(d => d.ID_OrderNavigation).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ID_Order)
                .HasConstraintName("FK__Feedback__ID_Ord__5441852A");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.ID_Notification).HasName("PK__Notifica__09D4F1664C729EBB");

            entity.ToTable("Notification");

            entity.Property(e => e.Event)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Organizing_time).HasColumnType("datetime");
            entity.Property(e => e.Time_create).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_OrderNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ID_Order)
                .HasConstraintName("FK_Notification_Order");

            entity.HasOne(d => d.ID_RequestNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ID_Request)
                .HasConstraintName("FK_Notification_Request");

            entity.HasOne(d => d.ID_TicketNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ID_Ticket)
                .HasConstraintName("FK__Notificat__ID_Ti__571DF1D5");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.ID_Order).HasName("PK__Order__EC9FA95560FEA472");

            entity.ToTable("Order");

            entity.Property(e => e.Create_At).HasColumnType("datetime");
            entity.Property(e => e.Payment_method)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Shipping_time).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Update_At).HasColumnType("datetime");

            entity.HasOne(d => d.ID_CustomerNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ID_Customer)
                .HasConstraintName("FK__Order__ID_Custom__4222D4EF");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.ID_OrderDetail).HasName("PK__OrderDet__855D4EF51EB5DA47");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total_price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ID_OrderNavigation).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ID_Order)
                .HasConstraintName("FK_OrderDetail_Order");

            entity.HasOne(d => d.ID_TicketNavigation).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ID_Ticket)
                .HasConstraintName("FK_OrderDetail_Ticket");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.ID_Package).HasName("PK__Package__10A64872D75876B9");

            entity.ToTable("Package");

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Name_Package)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.ID_Payment).HasName("PK__Payment__C2118ADEB4373304");

            entity.ToTable("Payment");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Payment_Provider)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ID_Report).HasName("PK__Report__C624529451F27020");

            entity.ToTable("Report");

            entity.Property(e => e.Comment).HasColumnType("text");
            entity.Property(e => e.History).HasColumnType("datetime");

            entity.HasOne(d => d.ID_OrderNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ID_Order)
                .HasConstraintName("FK_Report_Order");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.ID_Request).HasName("PK__Request__D55098805A3AC5D6");

            entity.ToTable("Request");

            entity.Property(e => e.History).HasColumnType("datetime");
            entity.Property(e => e.Price_want).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_CustomerNavigation).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ID_Customer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Request__ID_Cust__5EBF139D");

            entity.HasOne(d => d.ID_TicketNavigation).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ID_Ticket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Request__ID_Tick__5DCAEF64");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.ID_Role).HasName("PK__Role__43DCD32DCA821B51");

            entity.ToTable("Role");

            entity.Property(e => e.Name_role)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.ID_Ticket).HasName("PK__Ticket__79F5DC08E1A98218");

            entity.ToTable("Ticket");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Event_Date).HasColumnType("datetime");
            entity.Property(e => e.Image).IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Seat)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Show_Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Ticket_History).HasColumnType("datetime");
            entity.Property(e => e.Ticket_category)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.ID_CustomerNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ID_Customer)
                .HasConstraintName("FK__Ticket__ID_Custo__3F466844");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.ID_Transaction).HasName("PK__Transact__3F16C92D3DC11280");

            entity.ToTable("Transaction");

            entity.Property(e => e.Created_At).HasColumnType("datetime");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Transaction_Type).HasMaxLength(50);
            entity.Property(e => e.Updated_At).HasColumnType("datetime");

            entity.HasOne(d => d.ID_CustomerNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ID_Customer)
                .HasConstraintName("FK__Transacti__ID_Cu__5070F446");

            entity.HasOne(d => d.ID_OrderNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ID_Order)
                .HasConstraintName("FK__Transacti__ID_Or__4F7CD00D");

            entity.HasOne(d => d.ID_PaymentNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ID_Payment)
                .HasConstraintName("FK__Transacti__ID_Pa__5165187F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}