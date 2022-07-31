using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Branch
    {

        [Key]

        public long Id { get; set; }

        [Required]
        public string Name { get; set; }


        [Required]
        public string address { get; set; }

        [Required]
        public string phoneNumber { get; set; }

        [Required]
        [ForeignKey("Company")]
        public long CompanyID { get; set; }

        public Company company { get; set; }
    }
}
