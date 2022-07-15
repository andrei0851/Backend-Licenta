using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Model
    {
        [Key]
        public long Id { get; set; }

        public string name { get; set; }

        [ForeignKey("Make")]
        public long makeID { get; set; }

        public Make make { get; set; }

    }
}
