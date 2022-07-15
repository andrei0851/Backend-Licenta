using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class VehicleImage
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string imageURL { get; set; }
        [Required]
        [ForeignKey("Vehicle")]
        public long vehicleID { get; set; }
        public Vehicle vehicle { get; set; }
        [Required]
        public int order { get; set; }
        [Required]
        public string filename { get; set; }
    }
}
