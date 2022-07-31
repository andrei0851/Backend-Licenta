using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Entities.Models
{
    public class Company
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string companyName { get; set; }

        [Required]
        public string address { get; set; }

        [Required]
        public User owner { get; set; }

        public List<Branch> branches { get; set; }
    }
}
