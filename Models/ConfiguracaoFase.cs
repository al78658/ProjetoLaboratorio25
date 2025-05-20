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
        [Range(1, int.MaxValue, ErrorMessage = "O número de jogos por fase deve ser pelo menos 1")]
        public int NumJogosPorFase { get; set; }

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

        // Novos campos para Campeonato
        [Range(0, int.MaxValue)]
        public int PontosFaltaComparencia { get; set; }

        [Range(0, int.MaxValue)]
        public int PontosDesclassificacao { get; set; }

        [Range(0, int.MaxValue)]
        public int PontosExtra { get; set; }

        public List<string> CriteriosDesempate { get; set; } = new List<string>();

        // Foreign key
        public int CompeticaoId { get; set; }

        // Navigation property
        [ForeignKey("CompeticaoId")]
        public virtual Competicao Competicao { get; set; }
    }
} 