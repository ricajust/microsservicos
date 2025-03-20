using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alunos.API.Models
{
    public class Usuario
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(255, MinimumLength = 3)]
        [Column("nome")]
        public string Nome { get; set; }

        [Column("cpf")]
        [MaxLength(255)]
        public string Cpf { get; set; }

        [Column("data_nascimento")]
        public DateTime? DataNascimento { get; set; }

        [EmailAddress]
        [Column("email")]
        [MaxLength(255)]
        public string? Email { get; set; } // Tornando nullable

        [StringLength(20, MinimumLength = 8)]
        [Column("telefone")]
        public string? Telefone { get; set; } // Tornando nullable

        [MaxLength(255)]
        [Column("endereco")]
        public string? Endereco { get; set; } // Tornando nullable

        [MaxLength(100)]
        [Column("bairro")]
        public string? Bairro { get; set; } // Tornando nullable

        [MaxLength(100)]
        [Column("cidade")]
        public string? Cidade { get; set; } // Tornando nullable

        [StringLength(2, MinimumLength = 2)]
        [Column("uf")]
        public string? Uf { get; set; } // Tornando nullable

        [StringLength(9, MinimumLength = 8)]
        [Column("cep")]
        public string? Cep { get; set; } // Tornando nullable

        [StringLength(255, MinimumLength = 3)]
        [Column("senha")]
        public string Senha { get; set; }

        // Navigation property for the Aluno (pode ser nulo se um usuário não for um aluno)
        public Aluno? Aluno { get; set; }
    }
}