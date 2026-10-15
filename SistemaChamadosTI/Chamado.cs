
using System;

namespace SistemaChamadosTI
{
    public class Chamado
    {
        public int Id { get; set; }
        public string Solicitante { get; set; } = "";
        public string Setor { get; set; } = "";
        public string Equipamento { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string Prioridade { get; set; } = "Média";
        public string Responsavel { get; set; } = "";
        public DateTime DataAbertura { get; set; }
        public string Status { get; set; } = "Aberto";
    }
}