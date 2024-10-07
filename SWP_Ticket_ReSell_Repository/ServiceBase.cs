using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.Models;

namespace Repository;
public class ServiceBase<T> : GenericRepository<T> where T : class
{
    public ServiceBase(swp1Context context) : base(context)
    {
    }

}
