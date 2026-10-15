
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace SistemaChamadosTI
{
    public class ChamadoDAO
    {
        private readonly Conexao conexao = new Conexao();

        // VALIDAR O STATUS RECEBIDO
        private string ValidarStatus(string status)
        {
            status = (status ?? string.Empty).Trim();

            string[] statusPermitidos =
            {
                "Aberto",
                "Em andamento",
                "Aguardando",
                "Resolvido",
                "Cancelado"
            };

            foreach (string opcao in statusPermitidos)
            {
                if (string.Equals(
                    status,
                    opcao,
                    StringComparison.Ordinal))
                {
                    return status;
                }
            }

            throw new Exception(
                "O status recebido não é válido.\n" +
                "Valor recebido: [" + status + "]\n" +
                "Tamanho do texto: " + status.Length
            );
        }

        // CADASTRAR CHAMADO
        public void Cadastrar(Chamado chamado)
        {
            string status = ValidarStatus(chamado.Status);

            using (MySqlConnection conn = conexao.ObterConexao())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO chamados
                    (
                        solicitante,
                        setor,
                        equipamento,
                        categoria,
                        descricao,
                        prioridade,
                        responsavel,
                        status
                    )
                    VALUES
                    (
                        @solicitante,
                        @setor,
                        @equipamento,
                        @categoria,
                        @descricao,
                        @prioridade,
                        @responsavel,
                        @status
                    )";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@solicitante", chamado.Solicitante);

                    cmd.Parameters.AddWithValue(
                        "@setor", chamado.Setor);

                    cmd.Parameters.AddWithValue(
                        "@equipamento", chamado.Equipamento);

                    cmd.Parameters.AddWithValue(
                        "@categoria", chamado.Categoria);

                    cmd.Parameters.AddWithValue(
                        "@descricao", chamado.Descricao);

                    cmd.Parameters.AddWithValue(
                        "@prioridade", chamado.Prioridade);

                    cmd.Parameters.AddWithValue(
                        "@responsavel", chamado.Responsavel);

                    cmd.Parameters.Add(
                        "@status", MySqlDbType.VarChar, 20
                    ).Value = status;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // CONSULTAR TODOS OS CHAMADOS
        public List<Chamado> Listar()
        {
            List<Chamado> lista = new List<Chamado>();

            using (MySqlConnection conn = conexao.ObterConexao())
            {
                conn.Open();

                string sql = @"
                    SELECT
                        id,
                        solicitante,
                        setor,
                        equipamento,
                        categoria,
                        descricao,
                        prioridade,
                        responsavel,
                        data_abertura,
                        status
                    FROM chamados
                    ORDER BY id DESC";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Chamado chamado = new Chamado();

                        chamado.Id =
                            Convert.ToInt32(reader["id"]);

                        chamado.Solicitante =
                            Convert.ToString(reader["solicitante"]) ?? "";

                        chamado.Setor =
                            Convert.ToString(reader["setor"]) ?? "";

                        chamado.Equipamento =
                            Convert.ToString(reader["equipamento"]) ?? "";

                        chamado.Categoria =
                            Convert.ToString(reader["categoria"]) ?? "";

                        chamado.Descricao =
                            Convert.ToString(reader["descricao"]) ?? "";

                        chamado.Prioridade =
                            Convert.ToString(reader["prioridade"]) ?? "";

                        chamado.Responsavel =
                            Convert.ToString(reader["responsavel"]) ?? "";

                        chamado.DataAbertura =
                            Convert.ToDateTime(reader["data_abertura"]);

                        chamado.Status =
                            Convert.ToString(reader["status"]) ?? "";

                        lista.Add(chamado);
                    }
                }
            }

            return lista;
        }

        // ALTERAR STATUS DO CHAMADO
        public void AlterarStatus(int id, string status)
        {
            status = ValidarStatus(status);

            using (MySqlConnection conn = conexao.ObterConexao())
            {
                conn.Open();

                string banco = "";
                string tipoColuna = "";

                try
                {
                    // Identificar o banco usado pelo programa
                    using (MySqlCommand cmdBanco =
                        new MySqlCommand("SELECT DATABASE()", conn))
                    {
                        banco = Convert.ToString(
                            cmdBanco.ExecuteScalar()) ?? "(não identificado)";
                    }

                    // Consultar a definição real da coluna status
                    string sqlTipo = @"
                        SELECT COLUMN_TYPE
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = DATABASE()
                          AND TABLE_NAME = 'chamados'
                          AND COLUMN_NAME = 'status'";

                    using (MySqlCommand cmdTipo =
                        new MySqlCommand(sqlTipo, conn))
                    {
                        tipoColuna = Convert.ToString(
                            cmdTipo.ExecuteScalar()) ?? "(não encontrada)";
                    }

                    if (tipoColuna == "(não encontrada)")
                    {
                        throw new Exception(
                            "A coluna status não foi encontrada " +
                            "na tabela chamados do banco " + banco + "."
                        );
                    }

                    string sql = @"
                        UPDATE chamados
                        SET status = @status
                        WHERE id = @id";

                    using (MySqlCommand cmd =
                        new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add(
                            "@id", MySqlDbType.Int32
                        ).Value = id;

                        cmd.Parameters.Add(
                            "@status", MySqlDbType.VarChar, 20
                        ).Value = status;

                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        // Se nenhuma linha mudou, pode ser que o status
                        // já fosse igual. Conferimos se o ID existe.
                        if (linhasAfetadas == 0)
                        {
                            string sqlExiste = @"
                                SELECT COUNT(*)
                                FROM chamados
                                WHERE id = @id";

                            using (MySqlCommand cmdExiste =
                                new MySqlCommand(sqlExiste, conn))
                            {
                                cmdExiste.Parameters.Add(
                                    "@id", MySqlDbType.Int32
                                ).Value = id;

                                int existe = Convert.ToInt32(
                                    cmdExiste.ExecuteScalar());

                                if (existe == 0)
                                {
                                    throw new Exception(
                                        "O chamado de ID " + id +
                                        " não foi encontrado."
                                    );
                                }
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception(
                        "O MySQL não conseguiu alterar o status.\n\n" +
                        "Banco conectado: " + banco + "\n" +
                        "Tipo da coluna status: " + tipoColuna + "\n" +
                        "ID do chamado: " + id + "\n" +
                        "Status enviado: [" + status + "]\n" +
                        "Tamanho do texto: " + status.Length + "\n\n" +
                        "Erro original: " + ex.Message,
                        ex
                    );
                }
            }
        }

        // EXCLUIR CHAMADO
        public void Excluir(int id)
        {
            using (MySqlConnection conn = conexao.ObterConexao())
            {
                conn.Open();

                string sql = @"
                    DELETE FROM chamados
                    WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(
                        "@id", MySqlDbType.Int32
                    ).Value = id;

                    int linhasAfetadas = cmd.ExecuteNonQuery();

                    if (linhasAfetadas == 0)
                    {
                        throw new Exception(
                            "Nenhum chamado foi encontrado para excluir."
                        );
                    }
                }
            }
        }
    }
}