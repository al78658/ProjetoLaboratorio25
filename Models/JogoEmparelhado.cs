using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoLaboratorio25.Models
{
    // Esta classe foi substituída por EmparelhamentoBase
    // Mantida apenas para referência histórica
    /*
    public class JogoEmparelhado
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int Jogador1Id { get; set; }
        
        [Required]
        public int Jogador2Id { get; set; }
        
        [Required]
        public DateTime DataJogo { get; set; }
        
        [Required]
        public TimeSpan HoraJogo { get; set; }
        
        [Required]
        public int CompeticaoId { get; set; }

        [ForeignKey("CompeticaoId")]
        public virtual Competicao? Competicao { get; set; }
        
        [ForeignKey("Jogador1Id")]
        public virtual Jogador? Jogador1 { get; set; }
        
        [ForeignKey("Jogador2Id")]
        public virtual Jogador? Jogador2 { get; set; }
    }
    */
}