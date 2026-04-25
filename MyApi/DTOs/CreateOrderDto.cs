using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        [MinLength(3)]
        public string ProductName { get; set; } = null!;

        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}