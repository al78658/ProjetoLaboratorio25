using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoLaboratorio25.Models
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Clube1 { get; set; } = string.Empty;

        [Required]
        public string Clube2 { get; set; } = string.Empty;

        // Clube vitorioso (pode ser nulo em caso de empate)
        public string? ClubeVitorioso { get; set; }

        // Motivo da notificação (opcional)
        public string? Motivo { get; set; }

        // Data e hora da notificação
        [Required]
        public DateTime DataNotificacao { get; set; } = DateTime.Now;

        public int Pontuacao1 { get; set; }
        public int Pontuacao2 { get; set; }
    }
}