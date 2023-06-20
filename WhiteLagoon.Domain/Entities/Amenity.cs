using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon.Domain.Entities
{
    public class Amenity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Villa")]
        public int VillaId { get; set; }
        //this should not be validated
        public Villa? Villa { get; set; }

        public required string Name { get; set; }
        public string? Description { get; set; }

    }
}
