using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        public string Phonenumber { get; set; }

        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }

        public Boolean isConfirmed { get; set; }

        public string clientURI { get; set; }

        public string Role { get; set; }

        public int availableListings { get; set; }
        
        public List<Vehicle> Vehicles { get; set; }
        [ForeignKey("Branch")]
        public long? branchID { get; set; }

        public virtual Branch branch { get; set; }
        [ForeignKey("Company")]
        public long? companyID { get; set; }
        public virtual Company company { get; set; }

        public ProfilePicture profilepicture { get; set; }
    }
}
