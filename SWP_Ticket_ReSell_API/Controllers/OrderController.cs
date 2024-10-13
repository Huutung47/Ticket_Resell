using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
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

        [HttpGet("/{id}")]
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

        [HttpPost("/create")]
        public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateDTO orderRequest)
        {
            var order = new Order();

            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            order.Status = "PENDING";
            order.Create_At = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
            order.Shipping_time = TimeZoneInfo.ConvertTime(DateTime.Now.AddDays(3), vietnamTimeZone);
            orderRequest.Adapt(order);
            await _service.CreateAsync(order);

            if (orderRequest.ticketIds.Count() <= 0)
            {
                return Problem(detail: "Ticket not selected.", statusCode: 400);
            }


            foreach (var item in orderRequest.ticketIds)
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
