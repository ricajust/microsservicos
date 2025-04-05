public class AlunoAtualizadoNoMicrosservicoEvent
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string DataNascimento { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public string Endereco { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Uf { get; set; }
    public string Cep { get; set; }
    public string Senha { get; set; }
public string Origem { get; set; } = "Microsservico";
    public string EventType { get; set; } = "AlunoAtualizado";
}