using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SecretPlaces.Models
{
    public class Restaurant
    {
        [Required] public int ID { get; set; }

        [Required] public string Name { get; set; }

        [Required] public RestaurantType RestaurantType { get; set; }

        [Required] public double lon { get; set; }

        [Required] public double lat { get; set; }

        [DisplayName("Is Kosher?")] public bool IsKosher { get; set; }

        public virtual ICollection<Review> Posts { get; set; }
    }

    public enum RestaurantType
    {
        [Display(Name = "מזון מהיר")] FastFood,
        [Display(Name = "בתי קפה")] Caffes,
        [Display(Name = "איטלקי")] Italian,
        [Display(Name = "פיצה")] Pizza,
        [Display(Name = "בשרים")] Meat,
        [Display(Name = "אסייתי")] Asian,
        [Display(Name = "המבורגרים")] Burgers,
        [Display(Name = "קינוחים")] Deserts,
        [Display(Name = "מאפייה")] BakeShops,
    }
}