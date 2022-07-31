using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Entities.Models
{
    public class ProfilePicture
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string imgLink { get; set; }
        [Required]
        [ForeignKey("User")]
        public long UserID { get; set; }
        public User user { get; set; }
    }
}
