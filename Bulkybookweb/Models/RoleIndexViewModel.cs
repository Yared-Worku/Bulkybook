
namespace Bulkybookweb.Models
{
    public class RoleIndexViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public List<string> UserNames { get; set; } = new List<string>();
    }
}