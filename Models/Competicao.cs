using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoLaboratorio25.Models
{
    public class Competicao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da competição é obrigatório")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O tipo de competição é obrigatório")]
        [StringLength(50)]
        public string? TipoCompeticao { get; set; }

        [Required(ErrorMessage = "O número de jogadores é obrigatório")]
        [Range(2, int.MaxValue, ErrorMessage = "O número de jogadores deve ser pelo menos 2")]
        public int NumJogadores { get; set; }

        [Required(ErrorMessage = "O número de equipas é obrigatório")]
        [Range(2, int.MaxValue, ErrorMessage = "O número de equipas deve ser pelo menos 2")]
        public int NumEquipas { get; set; }

        [Required(ErrorMessage = "Os pontos por vitória são obrigatórios")]
        [Range(1, int.MaxValue, ErrorMessage = "Os pontos por vitória devem ser pelo menos 1")]
        public int PontosVitoria { get; set; }

        [Required(ErrorMessage = "Os pontos por empate são obrigatórios")]
        [Range(0, int.MaxValue, ErrorMessage = "Os pontos por empate não podem ser negativos")]
        public int PontosEmpate { get; set; }

        public int? OrganizadorId { get; set; }

        [ForeignKey("OrganizadorId")]
        public virtual Utilizador? Organizador { get; set; }

        // Navigation properties
        public virtual ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
        // Removido: public virtual ICollection<JogoEmparelhado> JogosEmparelhados { get; set; } = new List<JogoEmparelhado>();
        public virtual ICollection<EmparelhamentoFinal> EmparelhamentosFinal { get; set; } = new List<EmparelhamentoFinal>();
        public virtual ICollection<ConfiguracaoFase> ConfiguracoesFase { get; set; } = new List<ConfiguracaoFase>();
    }
}