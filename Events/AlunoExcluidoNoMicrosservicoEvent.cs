public class AlunoExcluidoNoMicrosservicoEvent
{
    public Guid Id { get; set; }
public string Origem { get; set; } = "Microsservico";
    public string EventType { get; set; } = "AlunoExcluido";
}