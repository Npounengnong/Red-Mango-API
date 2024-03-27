﻿using System.ComponentModel.DataAnnotations;

namespace Red_Mango_API.Models
{
    public class MenuItem
    {
        [Key] //properties
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpecialTag { get; set; }
        public string Category { get; set; }

        [Range(1, int.MaxValue)]
        public double Price { get; set; }

        [Required]
        public string Image { get; set; }

    }
}
