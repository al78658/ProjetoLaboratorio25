using System.ComponentModel.DataAnnotations;

namespace ProjetoLaboratorio25.Models
{
    public class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
        [StringLength(100)]
        public string UtilizadorNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
        public string Senha { get; set; } = string.Empty;
    }
} 