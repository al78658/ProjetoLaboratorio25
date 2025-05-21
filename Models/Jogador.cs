using ProjetoLaboratorio25.Models;
public class Jogador{
    public int Id { get; set; }
    public string? Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? Categoria { get; set; } = string.Empty;
    public string? Clube { get; set; } = string.Empty;

    public int CompeticaoId { get; set; }
    public Competicao? Competicao { get; set; }
}
