using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class FuelType
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        public string fuel { get; set; }
    }
}
