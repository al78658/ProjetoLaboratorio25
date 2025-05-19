using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoLaboratorio25.Models
{
    public class Jogador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(10)]
        public string Codigo { get; set; }

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; }

        [Required]
        [StringLength(100)]
        public string Clube { get; set; }
    }
}