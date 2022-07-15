using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend.Payloads
{
    public class SearchPayload
    {
        
        public float? priceMin { get; set; }

        public float? priceMax { get; set; }

        public int? kmMin { get; set; }

        public int? kmMax { get; set; }

        public string condition { get; set; }
        
        public int? manufactureYearMin { get; set; }

        public int? manufactureYearMax { get; set; }

        public long? vehicleType { get; set; }

        public long? fuel { get; set; }

        public long? color { get; set; }

        public long? countryID { get; set; }

        public long? modelID { get; set; }

        public long? makeID { get; set; }
    }
}
