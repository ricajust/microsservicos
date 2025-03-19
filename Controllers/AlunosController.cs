using Microsoft.AspNetCore.Mvc;
using Alunos.API.Services;
using Alunos.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("[controller]")]
public class AlunosController : ControllerBase
{
    private readonly IAlunoService _alunoService;

    public AlunosController(IAlunoService alunoService)
    {
        _alunoService = alunoService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlunoResponseDTO>> CreateAsync([FromBody] AlunoDTO alunoDTO)
    {
        if (alunoDTO == null || string.IsNullOrWhiteSpace(alunoDTO.Nome) || string.IsNullOrWhiteSpace(alunoDTO.Cpf) || string.IsNullOrWhiteSpace(alunoDTO.Senha))
        {
            return BadRequest("Os dados do aluno são inválidos.");
        }

        var alunoCriado = await _alunoService.CreateAsync(alunoDTO);
        var responseDTO = new AlunoResponseDTO
        {
            Id = alunoCriado.Id,
            Nome = alunoCriado.Nome,
            Cpf = alunoCriado.Cpf,
            DataNascimento = alunoCriado.DataNascimento,
            Email = alunoCriado.Email,
            Telefone = alunoCriado.Telefone,
            Endereco = alunoCriado.Endereco,
            Bairro = alunoCriado.Bairro,
            Cidade = alunoCriado.Cidade,
            Uf = alunoCriado.Uf,
            Cep = alunoCriado.Cep
        };

        return CreatedAtAction(nameof(GetByIdAsync), new { id = alunoCriado.Id }, responseDTO);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlunoResponseDTO>> GetByIdAsync(Guid id)
    {
        var alunoDTO = await _alunoService.GetByIdAsync(id);
        if (alunoDTO == null)
        {
            return NotFound();
        }

        var responseDTO = new AlunoResponseDTO
        {
            Id = alunoDTO.Id,
            Nome = alunoDTO.Nome,
            Cpf = alunoDTO.Cpf,
            DataNascimento = alunoDTO.DataNascimento,
            Email = alunoDTO.Email,
            Telefone = alunoDTO.Telefone,
            Endereco = alunoDTO.Endereco,
            Bairro = alunoDTO.Bairro,
            Cidade = alunoDTO.Cidade,
            Uf = alunoDTO.Uf,
            Cep = alunoDTO.Cep
        };

        return Ok(responseDTO);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlunoResponseDTO>>> GetAllAsync()
    {
        var alunosDTO = await _alunoService.GetAllAsync();
        var responseDTOs = new List<AlunoResponseDTO>();
        foreach (var alunoDTO in alunosDTO)
        {
            responseDTOs.Add(new AlunoResponseDTO
            {
                Id = alunoDTO.Id,
                Nome = alunoDTO.Nome,
                Cpf = alunoDTO.Cpf,
                DataNascimento = alunoDTO.DataNascimento,
                Email = alunoDTO.Email,
                Telefone = alunoDTO.Telefone,
                Endereco = alunoDTO.Endereco,
                Bairro = alunoDTO.Bairro,
                Cidade = alunoDTO.Cidade,
                Uf = alunoDTO.Uf,
                Cep = alunoDTO.Cep
            });
        }
        return Ok(responseDTOs);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlunoResponseDTO>> UpdateAsync(Guid id, [FromBody] AlunoDTO alunoDTO)
    {
        if (alunoDTO == null || string.IsNullOrWhiteSpace(alunoDTO.Nome) || string.IsNullOrWhiteSpace(alunoDTO.Cpf) || string.IsNullOrWhiteSpace(alunoDTO.Senha) || id == Guid.Empty)
        {
            return BadRequest("Os dados do aluno são inválidos.");
        }

        var alunoAtualizado = await _alunoService.UpdateAsync(id, alunoDTO);
        if (alunoAtualizado == null)
        {
            return NotFound();
        }

        var responseDTO = new AlunoResponseDTO
        {
            Id = alunoAtualizado.Id,
            Nome = alunoAtualizado.Nome,
            Cpf = alunoAtualizado.Cpf,
            DataNascimento = alunoAtualizado.DataNascimento,
            Email = alunoAtualizado.Email,
            Telefone = alunoAtualizado.Telefone,
            Endereco = alunoAtualizado.Endereco,
            Bairro = alunoAtualizado.Bairro,
            Cidade = alunoAtualizado.Cidade,
            Uf = alunoAtualizado.Uf,
            Cep = alunoAtualizado.Cep
        };

        return Ok(responseDTO);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var alunoExcluido = await _alunoService.DeleteAsync(id);
        if (alunoExcluido == null)
        {
            return NotFound();
        }
        return NoContent();
    }
}