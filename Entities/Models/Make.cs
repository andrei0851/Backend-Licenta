using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Make
    {
        [Key]
        public long Id { get; set; }

        public string name { get; set; }

        public List<Model> models { get; set; }

        public List<Vehicle> vehicles { get; set; }
    }
}
