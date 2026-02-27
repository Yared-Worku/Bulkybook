using Microsoft.AspNetCore.Identity;

namespace Bulkybookweb.Models
{
    public class Roles : IdentityRole<Guid>
    {
        //public string? Name { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
    }
}