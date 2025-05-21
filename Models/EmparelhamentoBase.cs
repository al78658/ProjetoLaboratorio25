using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoLaboratorio25.Models
{
    public class EmparelhamentoBase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Clube1 { get; set; } = string.Empty;

        [Required]
        public string Clube2 { get; set; } = string.Empty;

        [Required]
        public DateTime DataJogo { get; set; }

        [Required]
        public TimeSpan HoraJogo { get; set; }

        [Required]
        public int CompeticaoId { get; set; }

        [ForeignKey("CompeticaoId")]
        public virtual Competicao? Competicao { get; set; }

        // ✅ Adiciona esta propriedade opcional se quiseres guardar também o nome da competição
        public string? NomeCompeticao { get; set; }
    }
}
