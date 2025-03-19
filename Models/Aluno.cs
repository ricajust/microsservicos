using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alunos.API.Models
{
    public class Aluno : Usuario
    {
        // Navigation property for the Usuario
        [ForeignKey("Id")] // Explicitamente define que a FK é a propriedade Id desta classe
        public Usuario Usuario { get; set; }

        // Navigation property for Matricula (will be added later if needed)
        // public List<Matricula> Matriculas { get; set; }
    }
}