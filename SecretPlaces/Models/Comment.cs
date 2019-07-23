using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecretPlaces.Models
{
    public class Comment
    {
        public int ID { get; set; }

        [Required]
        public int ReviewID { get; set; }

        [Required]
        public string Content { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Publish Date")]
        public DateTime CreationDate { get; set; }

        public virtual Review Review { get; set; }

        public string UploaderUsername { get; set; }
    }
}
