using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PessoaDAO
{
    public class PessoaDAO
    {
        private string _cnnStr = Properties.Settings.Default.PessoaDAOConnectionString;

        private IList<Telefone> listarTelefonePorPessoa(SqlConnection cnn, int idPessoa)
        {
            var t = new List<Telefone>();
            var queryString = @"
                SELECT 
                    T.ID,
                    T.NUMERO,
                    T.DDD,
                    TT.ID TIPO_ID,
                    TT.TIPO
                FROM PESSOA_TELEFONE PT
                JOIN TELEFONE T ON T.ID = PT.ID_TELEFONE
                JOIN TELEFONE_TIPO TT ON TT.ID = T.TIPO
                WHERE PT.ID_PESSOA = @ID
            ";
            var cmd = new SqlCommand(queryString, cnn);
            cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = idPessoa;
            using (var row = cmd.ExecuteReader())
            {
                while (row.Read())
                {
                    t.Add(new Telefone
                    {
                        id = row.GetInt32(0),
                        numero = row.GetInt32(1),
                        ddd = row.GetInt32(2),
                        tipo = new TipoTelefone
                        {
                            id = row.GetInt32(3),
                            tipo = row.GetString(4),
                        }
                    });
                }
            }
            return t;
        }

        private Pessoa pegarPessoaPorCpf(SqlConnection cnn, long cpf)
        {
            var queryString = @"
                SELECT 
                    P.ID, 
                    P.NOME, 
                    P.CPF, 
                    P.ENDERECO AS ENDERECO_ID,
                    E.LOGRADOURO AS ENDERECO_LOGRADOURO, 
                    E.NUMERO AS ENDERECO_NUMERO, 
                    E.CEP ENDERECO_CEP, 
                    E.BAIRRO ENDERECO_BAIRRO, 
                    E.CIDADE ENDERECO_CIDADE, 
                    E.ESTADO ENDERECO_ESTADO
                FROM PESSOA P
                LEFT JOIN ENDERECO E ON E.ID = P.ENDERECO
                WHERE P.CPF = @CPF
            ";
            var cmd = new SqlCommand(queryString, cnn);
            cmd.Parameters.Add("@CPF", System.Data.SqlDbType.BigInt).Value = cpf;
            using (var row = cmd.ExecuteReader())
            {
                if (row.Read())
                {
                    var p = new Pessoa
                    {
                        id = row.GetInt32(0),
                        nome = row.GetString(1),
                        cpf = row.GetInt64(2)
                    };
                    if (!row.IsDBNull(3))
                    {
                        p.endereco = new Endereco
                        {
                            id = row.GetInt32(3),
                            logradouro = row.GetString(4),
                            numero = row.GetInt32(5),
                            cep = row.GetInt32(6),
                            bairro = row.GetString(7),
                            cidade = row.GetString(8),
                            estado = row.GetString(9)
                        };
                    }
                    return p;
                }
            }
            return null;
        }

        private int insiraEndereco(SqlConnection cnn, SqlTransaction trans, Endereco endereco)
        {
            var queryString = @"
                INSERT INTO ENDERECO (
                    LOGRADOURO, NUMERO, CEP, BAIRRO, CIDADE, ESTADO
                ) VALUES (
                    @LOGRADOURO, @NUMERO, @CEP, @BAIRRO, @CIDADE, @ESTADO
                )
                SELECT SCOPE_IDENTITY()
            ";
            var cmd = new SqlCommand(queryString, cnn, trans);
            cmd.Parameters.Add("@LOGRADOURO", System.Data.SqlDbType.VarChar, 256).Value = endereco.logradouro;
            cmd.Parameters.Add("@NUMERO", System.Data.SqlDbType.Int).Value = endereco.numero;
            cmd.Parameters.Add("@CEP", System.Data.SqlDbType.Int).Value = endereco.cep;
            cmd.Parameters.Add("@BAIRRO", System.Data.SqlDbType.VarChar, 50).Value = endereco.bairro;
            cmd.Parameters.Add("@CIDADE", System.Data.SqlDbType.VarChar, 30).Value = endereco.cidade;
            cmd.Parameters.Add("@ESTADO", System.Data.SqlDbType.VarChar, 20).Value = endereco.estado;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void insiraTelefone(SqlConnection cnn, SqlTransaction trans, int idPessoa, IList<Telefone> telefones)
        {
            string queryString = null;
            SqlCommand cmd = null;
            int idTelefone = 0, idTipoTelefone = 0;

            foreach (var telefone in telefones)
            {
                idTipoTelefone = 0;
                if (telefone.tipo != null)
                {
                    if (telefone.tipo.id > 0)
                    {
                        idTipoTelefone = telefone.tipo.id;
                    }
                    else
                    {
                        queryString = "INSERT INTO TELEFONE_TIPO (TIPO) VALUES (@TIPO)  SELECT SCOPE_IDENTITY()";
                        cmd = new SqlCommand(queryString, cnn, trans);
                        cmd.Parameters.Add("@TIPO", System.Data.SqlDbType.VarChar, 10).Value = telefone.tipo.tipo;
                        idTipoTelefone = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                queryString = @"
                        INSERT INTO TELEFONE (
                            NUMERO, DDD, TIPO
                        ) VALUES (
                            @NUMERO, @DDD, @TIPO
                        )
                        SELECT SCOPE_IDENTITY()
                    ";
                cmd = new SqlCommand(queryString, cnn, trans);
                cmd.Parameters.Add("@NUMERO", System.Data.SqlDbType.Int).Value = telefone.numero;
                cmd.Parameters.Add("@DDD", System.Data.SqlDbType.Int).Value = telefone.ddd;
                cmd.Parameters.Add("@TIPO", System.Data.SqlDbType.Int).Value = idTipoTelefone > 0 ? (object)idTipoTelefone : DBNull.Value;
                idTelefone = Convert.ToInt32(cmd.ExecuteScalar());

                queryString = @"
                        INSERT INTO PESSOA_TELEFONE (
                            ID_PESSOA, ID_TELEFONE
                        ) VALUES (
                            @ID_PESSOA, @ID_TELEFONE
                        )
                    ";
                cmd = new SqlCommand(queryString, cnn, trans);
                cmd.Parameters.Add("@ID_PESSOA", System.Data.SqlDbType.Int).Value = idPessoa;
                cmd.Parameters.Add("@ID_TELEFONE", System.Data.SqlDbType.Int).Value = idTelefone;
                cmd.ExecuteNonQuery();
            }
        }

        private int insiraPessoa(SqlConnection cnn, SqlTransaction trans, Pessoa p, int idEndereco)
        {
            var queryString = @"
                INSERT INTO PESSOA (
                    NOME, CPF, ENDERECO
                ) VALUES (
                    @NOME, @CPF, @ENDERECO
                )
                SELECT SCOPE_IDENTITY()
            ";
            var cmd = new SqlCommand(queryString, cnn, trans);
            cmd.Parameters.Add("@NOME", System.Data.SqlDbType.VarChar, 256).Value = p.nome;
            cmd.Parameters.Add("@CPF", System.Data.SqlDbType.BigInt).Value = p.cpf;
            cmd.Parameters.Add("@ENDERECO", System.Data.SqlDbType.Int).Value = idEndereco > 0 ? (object)idEndereco : DBNull.Value;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void alteraEndereco(SqlConnection cnn, SqlTransaction trans, Endereco endereco)
        {
            var queryString = @"
                        UPDATE ENDERECO SET
                            LOGRADOURO = @LOGRADOURO,
                            NUMERO = @NUMERO,
                            CEP = @CEP,
                            BAIRRO = @BAIRRO,
                            CIDADE = @CIDADE,
                            ESTADO = @ESTADO
                        WHERE ID = @ID
                    ";
            var cmd = new SqlCommand(queryString, cnn, trans);
            cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = endereco.id;
            cmd.Parameters.Add("@LOGRADOURO", System.Data.SqlDbType.VarChar, 256).Value = endereco.logradouro;
            cmd.Parameters.Add("@NUMERO", System.Data.SqlDbType.Int).Value = endereco.numero;
            cmd.Parameters.Add("@CEP", System.Data.SqlDbType.Int).Value = endereco.cep;
            cmd.Parameters.Add("@BAIRRO", System.Data.SqlDbType.VarChar, 50).Value = endereco.bairro;
            cmd.Parameters.Add("@CIDADE", System.Data.SqlDbType.VarChar, 30).Value = endereco.cidade;
            cmd.Parameters.Add("@ESTADO", System.Data.SqlDbType.VarChar, 20).Value = endereco.estado;
            cmd.ExecuteNonQuery();
        }

        private void alterePessoa(SqlConnection cnn, SqlTransaction trans, Pessoa p)
        {
            var queryString = @"
                UPDATE PESSOA SET
                    NOME = @NOME,
                    CPF = @CPF,
                    ENDERECO = @ENDERECO
                WHERE ID = @ID
            ";
            var cmd = new SqlCommand(queryString, cnn, trans);
            cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = p.id;
            cmd.Parameters.Add("@NOME", System.Data.SqlDbType.VarChar, 256).Value = p.nome;
            cmd.Parameters.Add("@CPF", System.Data.SqlDbType.BigInt).Value = p.cpf;
            cmd.Parameters.Add("@ENDERECO", System.Data.SqlDbType.Int).Value = (p.endereco != null) ? (object)p.endereco.id : DBNull.Value;
            cmd.ExecuteNonQuery();
        }

        public Pessoa consulte(long cpf)
        {
            using (var cnn = new SqlConnection(_cnnStr))
            {
                cnn.Open();
                var p = pegarPessoaPorCpf(cnn, cpf);
                if (p != null)
                {
                    p.telefones = listarTelefonePorPessoa(cnn, p.id);
                }
                return p;
            }
        }
        public bool insira(Pessoa p)
        {
            using (var cnn = new SqlConnection(_cnnStr))
            {
                int idPessoa = 0, idEndereco = 0;
                cnn.Open();
                var trans = cnn.BeginTransaction();
                try
                {
                    if (p.endereco != null)
                    {
                        idEndereco = insiraEndereco(cnn, trans, p.endereco);
                    }

                    idPessoa = insiraPessoa(cnn, trans, p, idEndereco);

                    insiraTelefone(cnn, trans, idPessoa, p.telefones);

                    trans.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public bool exclua(Pessoa p)
        {
            string queryString = null;
            SqlCommand cmd = null;
            using (var cnn = new SqlConnection(_cnnStr))
            {
                cnn.Open();
                var trans = cnn.BeginTransaction();
                try
                {
                    queryString = "DELETE FROM PESSOA_TELEFONE WHERE ID_PESSOA = @ID";
                    cmd = new SqlCommand(queryString, cnn, trans);
                    cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = p.id;
                    cmd.ExecuteNonQuery();

                    queryString = "DELETE FROM PESSOA WHERE ID = @ID";
                    cmd = new SqlCommand(queryString, cnn, trans);
                    cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = p.id;
                    cmd.ExecuteNonQuery();

                    if (p.endereco != null)
                    {
                        queryString = "DELETE FROM ENDERECO WHERE ID = @ID";
                        cmd = new SqlCommand(queryString, cnn, trans);
                        cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = p.endereco.id;
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public bool altere(Pessoa p)
        {
            string queryString = null;
            SqlCommand cmd = null;
            using (var cnn = new SqlConnection(_cnnStr))
            {
                cnn.Open();
                var trans = cnn.BeginTransaction();
                try
                {
                    if (p.endereco != null)
                    {
                        if (p.endereco.id > 0)
                        {
                            alteraEndereco(cnn, trans, p.endereco);
                        }
                        else
                        {
                            p.endereco.id = insiraEndereco(cnn, trans, p.endereco);
                            ;
                        }
                    }

                    alterePessoa(cnn, trans, p);

                    queryString = @"
                        DELETE PESSOA_TELEFONE 
                        FROM PESSOA_TELEFONE
                        WHERE ID_PESSOA = @ID
                    ";
                    cmd = new SqlCommand(queryString, cnn, trans);
                    cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = p.id;
                    cmd.ExecuteNonQuery();

                    insiraTelefone(cnn, trans, p.id, p.telefones);
                    trans.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
    }
}
