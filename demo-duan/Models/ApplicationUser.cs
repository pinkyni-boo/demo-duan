using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace demo_duan.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string Role { get; set; } = "Customer";
        
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}