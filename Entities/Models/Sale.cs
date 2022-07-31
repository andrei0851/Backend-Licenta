using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Sale
    {

        [Key]
        public long Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public long buyerID { get; set; }

        public User buyer { get; set; }

        [Required]
        public float finalprice { get; set; }

        [Required]
        public string proofOfPay { get; set; }

        [Required]
        [ForeignKey("Vehicle")]
        public long vehicleID { get; set; }

        public Vehicle vehicle { get;set; }

    }
}
