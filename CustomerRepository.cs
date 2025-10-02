using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace lojaCanuma
{
    public class CustomerRepository
    {
        // Conexão com o banco
        private readonly string _conn = new DBConnection().MyConnection();

        // 1) Listar todos os clientes
        public List<CustomerModels> GetAll()
        {
            var list = new List<CustomerModels>();

            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(
                "SELECT CustomerId, Name, Phone, Email, Points FROM tblCustomer", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new CustomerModels
                        {
                            CustomerId = dr.GetInt32(0),
                            Name = dr.GetString(1),
                            Phone = dr.IsDBNull(2) ? null : dr.GetString(2),
                            Email = dr.IsDBNull(3) ? null : dr.GetString(3),
                            Points = dr.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }

        // 2) Adicionar cliente (AGORA sem OUTPUT, usando SCOPE_IDENTITY())
        public int Add(CustomerModels c)
        {
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
                INSERT INTO tblCustomer (Name, Phone, Email)
                VALUES (@n, @p, @e);

                -- Pega o ID recém-criado nesta mesma conexão
                SELECT CAST(SCOPE_IDENTITY() AS int);
            ", cn))
            {
                // Preenche os "buracos" do SQL com os valores
                cmd.Parameters.AddWithValue("@n", c.Name);
                cmd.Parameters.AddWithValue("@p", (object)c.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@e", (object)c.Email ?? DBNull.Value);

                cn.Open();

                // Executa e volta com o ID do novo cliente
                return (int)cmd.ExecuteScalar();
            }
        }

        // 3) Somar (ou subtrair) pontos
        public void UpdatePoints(int customerId, int pointsToAdd)
        {
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(
                "UPDATE tblCustomer SET Points = Points + @pts WHERE CustomerId = @id", cn))
            {
                cmd.Parameters.AddWithValue("@pts", pointsToAdd);
                cmd.Parameters.AddWithValue("@id", customerId);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 4) Buscar cliente por ID
        public CustomerModels GetById(int id)
        {
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
                SELECT CustomerId, Name, Phone, Email, Points
                FROM tblCustomer
                WHERE CustomerId = @id", cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new CustomerModels
                        {
                            CustomerId = dr.GetInt32(0),
                            Name = dr.GetString(1),
                            Phone = dr.IsDBNull(2) ? null : dr.GetString(2),
                            Email = dr.IsDBNull(3) ? null : dr.GetString(3),
                            Points = dr.GetInt32(4)
                        };
                    }
                }
            }
            return null;
        }

        // Procura clientes pelo texto digitado.
        // top = quantos registros no máximo (pra não trazer a lista inteira de uma vez)
        public List<CustomerModels> Search(string term, int top = 20)
        {
            var list = new List<CustomerModels>();

            // Se não digitou nada, devolve vazio
            if (string.IsNullOrWhiteSpace(term)) return list;

            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
        SELECT TOP (@top) CustomerId, Name, Phone, Email, Points
        FROM tblCustomer
        WHERE
            Name  LIKE '%' + @t + '%'
         OR Phone LIKE '%' + @t + '%'
         OR Email LIKE '%' + @t + '%'
         OR CAST(CustomerId AS NVARCHAR(20)) LIKE '%' + @t + '%'
        ORDER BY Name;", cn))
            {
                cmd.Parameters.AddWithValue("@top", top);
                cmd.Parameters.AddWithValue("@t", term.Trim());

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new CustomerModels
                        {
                            CustomerId = dr.GetInt32(0),
                            Name = dr.GetString(1),
                            Phone = dr.IsDBNull(2) ? null : dr.GetString(2),
                            Email = dr.IsDBNull(3) ? null : dr.GetString(3),
                            Points = dr.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }


        public bool ExistsByName(string name, int? excludeId = null)
        {
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
                    SELECT COUNT(1)
                      FROM tblCustomer
                     WHERE Name = @n
                       AND (@id IS NULL OR CustomerId <> @id);
                ", cn))
                        {
                // Tipado e sem AddWithValue
                cmd.Parameters.Add("@n", SqlDbType.NVarChar, 100).Value = (name ?? "").Trim();

                var pId = cmd.Parameters.Add("@id", SqlDbType.Int);
                pId.Value = excludeId.HasValue ? (object)excludeId.Value : DBNull.Value;

                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public void UpdateBasic(CustomerModels c)
        {
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
                    UPDATE tblCustomer
                       SET Name  = @n,
                           Phone = @p,
                           Email = @e
                     WHERE CustomerId = @id;
                ", cn))
            {
                // Nome (obrigatório)
                cmd.Parameters.Add("@n", SqlDbType.NVarChar, 100).Value =
                    (c.Name ?? "").Trim();

                // Telefone (pode ser nulo)
                var pPhone = cmd.Parameters.Add("@p", SqlDbType.NVarChar, 50);
                pPhone.Value = string.IsNullOrWhiteSpace(c.Phone) ? (object)DBNull.Value : c.Phone.Trim();

                // Email (pode ser nulo)
                var pEmail = cmd.Parameters.Add("@e", SqlDbType.NVarChar, 150);
                pEmail.Value = string.IsNullOrWhiteSpace(c.Email) ? (object)DBNull.Value : c.Email.Trim();

                // ID (obrigatório)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = c.CustomerId;

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }





    }
}
