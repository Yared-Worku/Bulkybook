using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace Bulkybookweb.Models
{
    public class Users : IdentityUser<Guid>
    {
        [DisplayName("First Name")]
        public string? FName { get; set; }
        [DisplayName("Last Name")]
        public string? LName { get; set; }
        public bool? Is_Active { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
    }
}