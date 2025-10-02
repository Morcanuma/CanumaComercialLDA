using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace lojaCanuma
{
    class DBConnection
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        public string MyConnection()
        {
            string con = @"Data Source=192.168.1.177,1433;Initial Catalog=POS_DEMO_DBASE;User ID=CanumaDB;Password=123canuma;TrustServerCertificate=True;Network Library=DBMSSOCN";
            return con;
        }






        //public double GetVaL()
        //{
        //    double vat = 0; // Inicializa a variável 'vat' com zero (armazenará o valor do IVA)

        //    cn.ConnectionString = MyConnection(); // Define a string de conexão do banco usando um método chamado 'MyConnection'

        //    cn.Open(); // Abre a conexão com o banco de dados

        //    // Cria o comando SQL para selecionar todos os registros da tabela 'tblVat'
        //    cm = new SqlCommand("select * from tblVat", cn);

        //    dr = cm.ExecuteReader(); // Executa a consulta e retorna um SqlDataReader

        //    while (dr.Read()) // Enquanto houver linhas no resultado
        //    {
        //        // Converte o valor da coluna 'vat' para double e armazena na variável
        //        vat = Double.Parse(dr["vat"].ToString());
        //    }

        //    dr.Close();  // Fecha o DataReader
        //    cn.Close();  // Fecha a conexão com o banco de dados

        //    return vat;  // Retorna o valor do IVA (vat)

        //}


        /// <summary>
        /// Recupera a senha de um usuário com base no nome de usuário fornecido.
        /// </summary>
        /// <param name="user">Nome de usuário (username)</param>
        /// <returns>Senha do usuário, ou string vazia se não encontrado</returns>
        public string GetPassword(string user)
        {
            string password = "";

            // Conexão SQL
            using (SqlConnection cn = new SqlConnection(MyConnection()))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT password FROM tblUser WHERE username = @username", cn))
                {
                    cmd.Parameters.AddWithValue("@username", user);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read()) // Lê apenas se houver linha
                        {
                            password = dr["password"].ToString();
                        }
                    }
                }
            }

            return password;
        }
    }



}
