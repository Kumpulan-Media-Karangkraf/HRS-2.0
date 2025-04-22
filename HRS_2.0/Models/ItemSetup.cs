using System.ComponentModel.DataAnnotations;

namespace HRS_2.Models
{
    public class ItemSetup
    {
        [Required]
        [Key]
        public string DocNo { get; set; }

        [StringLength(50, ErrorMessage = "Name cannot be longer than 20 characters.")]
        public string Name { get; set; }

        [StringLength(15, ErrorMessage = "Item cannot be longer than 20 characters.")]
        public string Item { get; set; }
    }
}
