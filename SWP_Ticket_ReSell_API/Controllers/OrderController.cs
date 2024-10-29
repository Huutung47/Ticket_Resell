using Castle.Core.Resource;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Swashbuckle.AspNetCore.Annotations;
using SWP_Ticket_ReSell_DAO.DTO.Dashboard;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OrderController : Controller
    {
        private readonly ServiceBase<Order> _orderService;
        private readonly ServiceBase<OrderDetail> _orderDetailService;
        private readonly ServiceBase<Ticket> _ticketService;
        private readonly ServiceBase<Package> _packageService;
        private readonly ServiceBase<Feedback> _feedbackService;

        public OrderController(ServiceBase<Order> orderService, ServiceBase<OrderDetail> orderDetailService,
            ServiceBase<Package> packageService, ServiceBase<Ticket> ticketService, ServiceBase<Feedback> feedbackService)
        {
            _ticketService = ticketService;
            _orderDetailService = orderDetailService;
            _orderService = orderService;
            _packageService = packageService;
            _feedbackService = feedbackService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IList<OrderResponseDTO>>> GetOrder()
        {
            var entities = await _orderService.FindListAsync<OrderResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OrderResponseDTO>> GetOrderDetail(string id)
        {
            var entity = await _orderService.FindByAsync(p => p.ID_Order.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Order id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<OrderResponseDTO>());
        }

        [HttpPut]
        [Authorize]
        [SwaggerOperation(Summary = "Update order")]
        public async Task<IActionResult> PutOrder(OrderResponseDTO orderRequest)
        {
            var entity = await _orderService.FindByAsync(p => p.ID_Order == orderRequest.ID_Order);
            if (entity == null)
            {
                return Problem(detail: $"Order_id {orderRequest.ID_Order} cannot found", statusCode: 404);
            }
            orderRequest.Adapt(entity);
            await _orderService.UpdateAsync(entity);
            return Ok("Update Order successfull.");
        }


        [HttpPost()]
        [Authorize]
        [SwaggerOperation(Summary = "Create order")]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderRequest)
        {

            if (orderRequest.TicketItems.Count() <= 0)
            {
                return Problem(detail: "Ticket not selected.", statusCode: 400);
            }

            var order = new Order();

            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            order.ID_Customer = orderRequest.ID_Customer;
            order.Payment_method = orderRequest.Payment_method;
            order.Status = "PENDING";
            order.Create_At = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
            order.Shipping_time = TimeZoneInfo.ConvertTime(DateTime.Now.AddDays(3), vietnamTimeZone);

            var TicketItems = orderRequest.TicketItems;
            decimal totalPriceOrder = 0;
            foreach (var item in TicketItems)
            {
                var ticket = await _ticketService.FindByAsync(t => t.ID_Ticket == item.ID_Ticket);

                if (item.Quantity < 1)
                {
                    return Problem(detail: "The number of tickets cannot be 0", statusCode: 400);
                }

                if (item.Quantity > ticket.Quantity)
                {
                    return Problem(detail: "Ticket is not enough. " + $"[{ticket.Quantity}] tickets left", statusCode: 400);
                }

                //totalPriceOrder += (decimal)ticket.Price * (decimal)item.Quantity;

                ticket.Quantity = ticket.Quantity - item.Quantity;
                if (ticket.Quantity < 1)
                {
                    ticket.Status = "Unavailable";
                }
                await _ticketService.UpdateAsync(ticket);
                if (ticket.Quantity < 1)
                {
                    ticket.Status = "Unavailable";
                }
                await _ticketService.UpdateAsync(ticket);
            }

            //order.TotalPrice = Convert.ToDecimal(totalPriceOrder);
            order.TotalPrice = orderRequest.TotalPrice;
            order.Update_At = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
            //orderRequest.Adapt(order);
            await _orderService.CreateAsync(order);

            foreach (var item in orderRequest.TicketItems)
            {
                var ticket = await _ticketService.FindByAsync(t => t.ID_Ticket == item.ID_Ticket);

                var orderDetail = new OrderDetail
                {
                    ID_Order = order.ID_Order,
                    ID_Ticket = item.ID_Ticket,
                    Price = ticket.Price,
                    Quantity = item.Quantity,
                    Total_price = ticket.Price * item.Quantity
                };

                order.Adapt(orderDetail);
                await _orderDetailService.CreateAsync(orderDetail);
            }

            return Ok(new { message = $"Order [" + $"{order.ID_Order}" + "] create successfully.", orderId = order.ID_Order });
        }

        [HttpGet("all-order-customerid")]
        //[Authorize]
        public async Task<ActionResult<IList<OrderResponseDTO>>> GetOrdersByCustomerId(int customerid)
        {
            var entities = await _orderService.FindListAsync<OrderResponseDTO>(t => t.ID_CustomerNavigation.ID_Customer == customerid);
            if (entities == null)
            {
                return Problem(detail: $"CustomerID {customerid} doesn't have any order", statusCode: 404);
            }
            return Ok(entities);
        }

        [HttpGet("total-all-order-customerid")]
        //[Authorize]
        public async Task<ActionResult<int>> GetTotal(int customerid)
        {
            int? totalPrice=0;
            var entities = await _orderService.FindListAsync<OrderResponseDTO>(t => t.ID_CustomerNavigation.ID_Customer == customerid);
            if(entities == null)
            {
                return Problem(detail: $"CustomerID {customerid} doesn't have any order", statusCode: 404);
            }
            foreach(var item in entities)
            {
                totalPrice = totalPrice +(int)item.TotalPrice;
            }
            return Ok($"Total: {totalPrice}");
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderService.FindByAsync(p => p.ID_Order == id);
            if (order == null)
            {
                return Problem(detail: $"Order_id {id} cannot found", statusCode: 404);
            }

            await _orderService.DeleteAsync(order);
            return Ok("Delete order successfull.");
        }


        [HttpGet("count-order-successfull")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<DashboardOrder>> GetOrderCompleted()
        {
            var successOrders = await _orderService.FindListAsync<Order>(o => o.Status == "COMPLETED");
            var totalSuccess = successOrders.Count();
            return Ok(totalSuccess);
        }

        [HttpGet("count-order-processing")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<DashboardOrder>> GetOrderProcessing()
        {
            var processingOrders = await _orderService.FindListAsync<Order>(o => o.Status == "PROCESSING");
            var totalProcessing = processingOrders.Count();
            return Ok(totalProcessing);
        }

        [HttpGet("count-order-pending")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<DashboardOrder>> GetOrderPending()
        {
            var pendingOrders = await _orderService.FindListAsync<Order>(o => o.Status == "PENDING");
            var totalPending = pendingOrders.Count();
            return Ok(totalPending);
        }

        [HttpGet("count-all-order")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<int>> GetOrderTotal()
        {
            var totalOrders = await _orderService.FindListAsync<Order>();
            var total = totalOrders.Count();
            return Ok(total);
        }

        [HttpGet("count-order-successfull-by-day-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<int>> GetOrderCompletedByDate(DateTime date)
        {
            var successOrders = await _orderService.FindListAsync<Order>(o => o.Status == "COMPLETED" 
            && o.Create_At.Date == date.Date);
            var totalSuccess = successOrders.Count();
            return Ok(totalSuccess);
        }
        [HttpGet("count-order-processing-by-day-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<int>> GetOrderProcessingByDate(DateTime date)
        {
            var successOrders = await _orderService.FindListAsync<Order>(o => o.Status == "PROCESSING" 
            && o.Create_At.Date == date.Date);
            var totalSuccess = successOrders.Count();
            return Ok(totalSuccess);
        }

        [HttpGet("count-order-pending-by-day-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<int>> GetOrderPendingByDate(DateTime date)
        {
            var successOrders = await _orderService.FindListAsync<Order>(o => o.Status == "PENDING"
            && o.Create_At.Date == date.Date);
            var total = successOrders.Count();
            return Ok(total);
        }

        [HttpGet("count-order-completed-by-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<decimal>> GetOrderCompletedByMonthYear(int month, int year)
        {
            var customers = await _orderService.FindListAsync<Customer>(o => o.Status == "COMPLETED" 
            && o.Create_At.Month == month 
            && o.Create_At.Year == year);
            var customersTotal = customers.Count();
            return Ok(customersTotal);
        }

        [HttpGet("count-order-processing-by-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<decimal>> GetOrderProcessingByMonthYear(int month, int year)
        {
            var customers = await _orderService.FindListAsync<Customer>(o => o.Status == "PROCESSING"
            && o.Create_At.Month == month
            && o.Create_At.Year == year);
            var customersTotal = customers.Count();
            return Ok(customersTotal);
        }

        [HttpGet("count-order-pending-by-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<decimal>> GetOrderPendingByMonthYear(int month, int year)
        {
            var customers = await _orderService.FindListAsync<Customer>(o => o.Status == "PENDING"
            && o.Create_At.Month == month
            && o.Create_At.Year == year);
            var customersTotal = customers.Count();
            return Ok(customersTotal);
        }

        [HttpGet("Total-price-order-completed-by-month-year")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<decimal>> GetRevenueByDate(int month, int year)
        {
            var orders = await _orderService.FindListAsync<Order>(o => o.Status == "COMPLETED" 
            && o.Create_At.Date.Month == month
            && o.Create_At.Date.Year == year);
            if (orders == null || !orders.Any())
            {
                return BadRequest($"Khong co ai order trong thang {month}, nam {year}");
            }
            decimal totalRevenue = orders.Where(o => o.TotalPrice.HasValue).Sum(o => o.TotalPrice.Value);
            return Ok(totalRevenue);
        }
    }
}
