using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Vehicle
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public float price { get; set; }
        [Required]
        public string description { get; set; }

        [Required]
        public int km { get; set; }

        public string VIN { get; set; }
        [Required]
        public string postDate { get; set; }
        [Required]
        public string condition { get; set; }
        [Required]
        public int manufactureYear { get; set; }
        [Required]
        [ForeignKey("User")]
        public long UserID { get; set; }

        public string user { get; set; }

        [ForeignKey("Country")]
        [Required]
        public long countryID { get; set; }

        public string country {get;set;}

        [ForeignKey("Model")]
        [Required]
        public long modelID { get; set; }

        public string model { get; set; }

        [ForeignKey("Make")]
        [Required]
        public long makeID { get; set; }

        public string make { get; set; }

        [ForeignKey("VehicleColor")]
        [Required]
        public long vehicleColorId { get; set; }

        public string color { get; set; }

        [ForeignKey("VehicleType")]
        [Required]
        public long vehicleTypeId { get; set; }

        public string type { get; set; }

        [ForeignKey("FuelType")]
        [Required]
        public long fuelTypeId { get; set; }

        public string fuel { get; set; }

        public bool active { get; set; }

        public string firstImage { get; set; }

        public long cc { get; set; }

        public long power { get; set; }
    }
}
