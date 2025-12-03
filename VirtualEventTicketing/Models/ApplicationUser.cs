using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketing.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(260)]
        public string? ProfileImagePath { get; set; }
    }
}
