using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Swashbuckle.AspNetCore.Annotations;
using SWP_Ticket_ReSell_DAO.DTO.Order;
using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OrderController : Controller
    {
        private readonly ServiceBase<Order> _service;
        private readonly ServiceBase<OrderDetail> _orderDetailService;
        private readonly ServiceBase<Ticket> _ticketService;

        public OrderController(ServiceBase<Order> service, ServiceBase<OrderDetail> orderDetailService, ServiceBase<Ticket> ticketService)
        {
            _ticketService = ticketService;
            _orderDetailService = orderDetailService;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IList<OrderResponseDTO>>> GetOrder()
        {
            var entities = await _service.FindListAsync<OrderResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrderDetail(string id)
        {
            var entity = await _service.FindByAsync(p => p.ID_Order.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Order id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<OrderResponseDTO>());
        }

        [HttpPut]
        [SwaggerOperation(Summary = "Update order")]
        public async Task<IActionResult> PutOrder(OrderResponseDTO orderRequest)
        {
            var entity = await _service.FindByAsync(p => p.ID_Order == orderRequest.ID_Order);
            if (entity == null)
            {
                return Problem(detail: $"Order_id {orderRequest.ID_Order} cannot found", statusCode: 404);
            }
            orderRequest.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update Order successfull.");
        }

        [HttpPost()]
        [SwaggerOperation(Summary = "Create order")]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderRequest)
        {

            if (orderRequest.TicketItems.Count() <= 0)
            {
                return Problem(detail: "Ticket not selected.", statusCode: 400);
            }

            var order = new Order();

            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
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

                totalPriceOrder += (decimal)ticket.Price * (decimal)item.Quantity;

                ticket.Quantity -= item.Quantity;
                await _ticketService.UpdateAsync(ticket);
            }
            order.TotalPrice = Convert.ToDecimal(totalPriceOrder);
            orderRequest.Adapt(order);
            await _service.CreateAsync(order);

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

            return Ok("Order [" + $"{order.ID_Order}" + "] create successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _service.FindByAsync(p => p.ID_Order == id);
            if (order == null)
            {
                return Problem(detail: $"Order_id {id} cannot found", statusCode: 404);
            }

            await _service.DeleteAsync(order);
            return Ok("Delete order successfull.");
        }
    }
}
