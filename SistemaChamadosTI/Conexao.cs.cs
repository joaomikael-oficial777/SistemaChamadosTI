
using MySql.Data.MySqlClient;

namespace SistemaChamadosTI
{
    public class Conexao
    {
        private readonly string stringConexao =
            "Server=localhost;Port=3306;Database=chamados_ti;Uid=root;Pwd=root;";

        public MySqlConnection ObterConexao()
        {
            return new MySqlConnection(stringConexao);
        }
    }
}