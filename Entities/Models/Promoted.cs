using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Promoted
    {

        [Key]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Vehicle")]
        public long vehicleId { get; set; }

        public Vehicle vehicle { get; set; }
        [Required]
        public bool isPromoted { get; set; }

        public DateTime promotedUntil { get; set; }
        
    }
}
