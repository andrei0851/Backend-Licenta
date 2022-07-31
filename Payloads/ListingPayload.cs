

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend.Payloads
{
    public class ListingPayload
    {
        [Required]
        public float price { get; set; }
        [Required]
        public string description { get; set; }

        [Required]
        public int km { get; set; }

        public string VIN { get; set; }

        [Required]
        public string condition { get; set; }
        [Required]
        public int manufactureYear { get; set; }

        [Required]
        public long vehicleType { get; set; }
        [Required]
        public long fuel { get; set; }
        [Required]
        public long color { get; set; }

        [Required]
        public long cc { get; set; }

        [Required]
        public long power { get; set; }

        [Required]
        public long countryID { get; set; }

        [Required]
        public long modelID { get; set; }

        [Required]
        public long makeID { get; set; }

        public string firstPhoto { get; set; }
    }
}
