using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRS_2.Data
{
    public class v_stafflist
    {
        [Key]
        public string? NoPekerja { get; set; }  // Allow null

        public string? Nama { get; set; } = "No Name";  // Set default value
        public string? Syrt { get; set; } = "No Company";
        public string? Jab { get; set; } = "No Department";
        public string? username { get; set; } = "NoUsername";

        public string? email { get; set; } = "NoEmail";

        [NotMapped]
        public string? Kod_Penyelia { get; set; } = "NoKodPenyelia";
        [NotMapped]
        public string? HPTellNumber { get; set; } = "NoTel";
    }

}
