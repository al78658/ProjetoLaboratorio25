using System.ComponentModel.DataAnnotations;

namespace ProjetoLaboratorio25.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Required]
        public string Categoria { get; set; }

        public string? Codigo { get; set; }  

        public string Titulo { get; set; }

        public string Conteudo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }

}
