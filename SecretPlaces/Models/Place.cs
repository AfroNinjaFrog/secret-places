using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SecretPlaces.Models
{
    public class Place
    {
        [Required] public int ID { get; set; }

        [Required] public string Name { get; set; }

        [Required] public double lon { get; set; }

        [Required] public double lat { get; set; }

        public virtual ICollection<Review> Posts { get; set; }
    }
}