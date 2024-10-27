using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Notificate;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificateController : ControllerBase
    {
        private readonly ServiceBase<Ticket> _serviceTicket;
        private readonly ServiceBase<Notification> _serviceNotification;
        private readonly GenericRepository<Notification> _notificationRepository;

        public NotificateController(ServiceBase<Ticket> serviceTicket, ServiceBase<Notification> serviceNotification, GenericRepository<Notification> notificationRepository)
        {
            _serviceTicket = serviceTicket;
            _serviceNotification = serviceNotification;
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IList<NotificateResponseDTO>>> GetNotificate()
        {
            var entities = await _serviceNotification.FindListAsync<NotificateResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificateResponseDTO>> GetNotificate(string id)
        {
            var entity = await _serviceNotification.FindByAsync(p => p.ID_Notification.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Notificate id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<NotificateResponseDTO>());
        }

        [HttpPut]
        public async Task<IActionResult> PutNotificate(NotificateResponseDTO notificate)
        {
            var entity = await _serviceNotification.FindByAsync(p => p.ID_Notification == notificate.ID_Notification);
            if (entity == null)
            {
                return Problem(detail: $"Notificate_id {notificate.ID_Notification} cannot found", statusCode: 404);
            }
            notificate.Adapt(entity);
            await _serviceNotification.UpdateAsync(entity);
            return Ok("Update notificate successfull.");
        }

        [HttpPost]
        public async Task<ActionResult<NotificateResponseDTO>> PostNotificate(NotificateDTO notificate)
        {
            var notificates = new Notification()
            {
                ID_Request = notificate.ID_Request,
                Title = notificate.Title,
                ID_Order = notificate.ID_Order,
                Event = notificate.Event,
                ID_Ticket = notificate.ID_Ticket,
                Organizing_time = notificate.Organizing_time,
                Time_create = DateTime.Now
            };
            //notificate.Adapt(notificates);
            await _serviceNotification.CreateAsync(notificates);
            return Ok("Create notificate successfull.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotificate(int id)
        {
            var notificate = await _serviceNotification.FindByAsync(p => p.ID_Notification == id);
            if (notificate == null)
            {
                return Problem(detail: $"Notificate_id {id} cannot found", statusCode: 404);
            }

            await _serviceNotification.DeleteAsync(notificate);
            return Ok("Delete ticket successfull.");
        }
    }
}
