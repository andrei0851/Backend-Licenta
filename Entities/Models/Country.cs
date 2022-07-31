using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Country
    {
        [Key]
        public long countryID { get; set; }

        public string name { get; set; }
    }
}
