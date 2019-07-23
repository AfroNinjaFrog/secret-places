using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecretPlaces.Models
{
    public class Review
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        [Display(Name = "Publish Date")]
        [DataType(DataType.Date)]
        public DateTime PublishDate { get; set; }

        [Display(Name = "Uploader Username")]
        public string UploaderUsername { get; set; }

        [Required]
        public int RestaurantID { get; set; }

        public virtual Restaurant Restaurant { get; set; }

        public virtual List<Comment> Comments { get; set; }

        public virtual bool IsRecommended { get; set; }
    }

    public class GroupByRestaurant
    {
        [Display(Name = "Restaurant Name")]
        public string RestaurantName { get; set; }

        [Display(Name = "Total Reviews")]
        public int TotalReviews { get; set; }
    }
}
