using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecretPlaces.Models
{
    public class User
    {
        [Required]
        public string ID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Firstname{ get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        [Display(Name = "Is Admin?")]
        public bool IsAdmin { get; set; }
    }
}