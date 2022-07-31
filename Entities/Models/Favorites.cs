using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Favorites
    {
        [Key]
        public long Id {  get; set; }

        [ForeignKey("User")]
        public long UserId { get; set; }

        public User user { get; set; }

        [ForeignKey("Vehicle")]
        public long VehicleId { get; set; }

        public Vehicle vehicle { get; set; }
    }
}
