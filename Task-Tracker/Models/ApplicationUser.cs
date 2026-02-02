using Microsoft.AspNetCore.Identity;
using System;

namespace TaskTracker.Models  // <--- Check this namespace carefully
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}