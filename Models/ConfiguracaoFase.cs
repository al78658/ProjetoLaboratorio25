using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoLaboratorio25.Models
{
    public class ConfiguracaoFase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FaseNumero { get; set; }

        [Required]
        [StringLength(50)]
        public string Formato { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PontosVitoria { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PontosEmpate { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PontosDerrota { get; set; }

        public List<string> CriteriosDesempate { get; set; } = new List<string>();

        // Foreign key
        public int CompeticaoId { get; set; }

        // Navigation property
        [ForeignKey("CompeticaoId")]
        public virtual Competicao Competicao { get; set; }
    }
}