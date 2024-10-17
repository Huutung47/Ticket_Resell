using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using Repository;

[ApiController]
[Route("api/[controller]")]
public class FirebaseController : ControllerBase
{
    private readonly FirebaseStorageService _firebaseStorageService;
    private readonly ServiceBase<Ticket> _serviceTicket;

    public FirebaseController(FirebaseStorageService firebaseStorageService, ServiceBase<Ticket> serviceTicket)
    {
        _firebaseStorageService = firebaseStorageService;
        _serviceTicket = serviceTicket;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<TicketResponseDTO>> UploadImage([FromForm]TicketImageDTO ticketRequest)
    {
        var imageTicket = new Ticket()
        {
            //ID_Customer = customerID,
            //Image = null,
        };
        if (ticketRequest.Image != null && ticketRequest.Image.Length > 0)
        {
            // Tải ảnh lên Firebase
            using (var stream = ticketRequest.Image.OpenReadStream())
            {
                var imageUrl = await _firebaseStorageService.UploadFileAsync(stream, ticketRequest.Image.FileName);
                var ticket = new Ticket
                {
                    Image = imageUrl,  
                };
                await _serviceTicket.CreateAsync(ticket);
                return Ok("Create ticket image successfull.\n");
            }
        }
        else
        {
            return BadRequest("Add image ticket pls");
        }
       
    }
}
