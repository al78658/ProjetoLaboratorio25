using System.ComponentModel.DataAnnotations;

namespace ProjetoLaboratorio25.Models
{
    public class ConfiguracaoFase
    {
        [Required]
        public int FaseNumero { get; set; }

        [Required]
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
    }
} 