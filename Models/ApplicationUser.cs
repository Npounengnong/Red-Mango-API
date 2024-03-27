using Microsoft.AspNetCore.Identity;

namespace Red_Mango_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
