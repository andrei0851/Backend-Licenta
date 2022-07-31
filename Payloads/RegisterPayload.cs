using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    public class RegisterPayload
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Phonenumber { get; set; }

        [Required]
        public string Lastname { get; set; }
    }
}