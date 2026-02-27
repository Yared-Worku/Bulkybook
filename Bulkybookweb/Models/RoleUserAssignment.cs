using System;
using System.Collections.Generic;

namespace Bulkybookweb.Models
{
    public class RoleUserAssignmentViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public List<UserSelection> Users { get; set; } = new List<UserSelection>();
    }

    public class UserSelection
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}