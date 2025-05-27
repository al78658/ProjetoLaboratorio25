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

        // Indica se a notificação foi lida
        public bool Lida { get; set; } = false;

        // Referência ao emparelhamento relacionado (opcional)
        public int? EmparelhamentoId { get; set; }

        [ForeignKey("EmparelhamentoId")]
        public virtual EmparelhamentoBase? Emparelhamento { get; set; }

        // Referência à competição
        [Required]
        public int CompeticaoId { get; set; }

        [ForeignKey("CompeticaoId")]
        public virtual Competicao? Competicao { get; set; }
    }
}