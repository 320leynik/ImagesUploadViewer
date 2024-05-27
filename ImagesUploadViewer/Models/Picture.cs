using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImagesUploadViewer.Models
{
    [Table("Pictures")]
    public class Picture
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Ім'я файлу")]
        [Column("filename")]
        public string Filename { get; set; }

        [Required]
        [Range(0, long.MaxValue)]
        [Display(Name = "Розмір файлу")]
        [Column("filesize")]
        public long Filesize { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата і час завантаження")]
        [Column("loadedat")]
        public DateTime LoadeDat { get; set; }

        [MaxLength(500)]
        [Display(Name = "Шлях до файлу")]
        [Column("filepath")]
        public string FilePath { get; set; }
    }
}
