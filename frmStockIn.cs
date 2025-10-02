using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmStockIn : Form
    {

        string nomeLogado = SessaoUsuario.Nome;
        

        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        SqlDataReader dr;
        string stitle = "Sistema de Vendas(morcanuma)";
        public frmStockIn()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());

            // Carrega fornecedores na ComboBox
            CarregarFornecedores();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

       private void PrenchertxtBox()
        {
            // Gerar número de referência único baseado na hora atual
            txtRefNo.Text = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Pegar nome do usuário logado e preencher o campo
            txtBy.Text = nomeLogado;
        }

        private void frmStockIn_Load(object sender, EventArgs e)
        {
            cbxFornecedor.SelectedIndexChanged += cbxFornecedor_SelectedIndexChanged;


            PrenchertxtBox();

            // Carregar itens ainda pendentes
            LoadStockIn();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }


        public void LoadStockIn()
        {
                int i = 0;
                dataGridView2.Rows.Clear(); // Limpa a tabela antes de carregar os dados
                cn.Open(); // Abre a conexão com o banco de dados

                cm = new SqlCommand("select * from vwStockin where refno like '" + txtRefNo.Text + "' and status like 'Pending'", cn);// Consulta à view vwStockIn
                dr = cm.ExecuteReader(); // Executa e lê os dados

                while (dr.Read())
                {
                    i++; // Contador de linhas (opcional)

                    // Adiciona uma nova linha à grid com os dados obtidos
                    dataGridView2.Rows.Add(
                        i,                                // Número sequencial
                        dr["id"].ToString(),
                        dr["refno"].ToString(),
                        dr["pcode"].ToString(),
                        dr["pdesc"].ToString(),
                        dr["qty"].ToString(),
                        dr["sdate"].ToString(),
                        dr["stockinby"].ToString(),
                        dr["vendedor"].ToString()
                    );
                }

                dr.Close(); // Fecha o leitor
                cn.Close(); // Fecha a conexão
            }


        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmSearchProduct_Stockin frm = new frmSearchProduct_Stockin(this);
            frm.LoadProduct();
            frm.ShowDialog();
        }

        public void Clear()
        {
            txtBy.Clear();
            txtRefNo.Clear();
            dt1.Value = DateTime.Now;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.Rows.Count > 0)
                {
                    if (MessageBox.Show("Tens certeza que queres salvar este registro?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        for (int i = 0; i < dataGridView2.Rows.Count; i++)
                        {
                            // Atualiza a quantidade no produto
                            cn.Open();
                            cm = new SqlCommand("UPDATE tblProduct SET qty = qty + @addQty WHERE pcode = @pcode", cn);
                            cm.Parameters.AddWithValue("@addQty", int.Parse(dataGridView2.Rows[i].Cells[5].Value.ToString()));
                            cm.Parameters.AddWithValue("@pcode", dataGridView2.Rows[i].Cells[3].Value.ToString());
                            cm.ExecuteNonQuery();
                            cn.Close();

                            // Atualiza o registro no tblStockIn, incluindo o fornecedor
                            cn.Open();
                            cm = new SqlCommand("UPDATE tblStockIn SET qty = qty + @qty, status = 'Done', vendorid = @vendorid WHERE id = @id", cn);
                            cm.Parameters.AddWithValue("@qty", int.Parse(dataGridView2.Rows[i].Cells[5].Value.ToString()));
                            cm.Parameters.AddWithValue("@id", dataGridView2.Rows[i].Cells[1].Value.ToString());
                            cm.Parameters.AddWithValue("@vendorid", Convert.ToInt32(lblFornecedorId.Text));
                            cm.ExecuteNonQuery();
                            cn.Close();
                        }

                        // Limpa campos, gera novo número e recarrega dados
                        Clear();
                        ClearFornecedor(); // você pode definir essa função abaixo
                        PrenchertxtBox();
                        LoadStockIn();

                        // Mostra mensagem de sucesso
                        MessageBox.Show("Registro salvo com sucesso!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ClearFornecedor()
        {
            cbxFornecedor.SelectedIndex = -1;
            txtContactPerson.Clear();
            txtEndereco.Clear();
            lblFornecedorId.Text = "";
        }



        private void LoadStockInHistory()
        {
            int i = 0;
            dataGridView1.Rows.Clear(); // Limpa a tabela antes de carregar os dados

            try
            {
                cn.Open(); // Abre a conexão com o banco de dados

                // Consulta segura usando parâmetros para evitar SQL Injection e erros de data
                string query = @"
            SELECT * FROM vwStockin 
            WHERE CAST(sdate AS DATE) BETWEEN @startDate AND @endDate 
            AND status = 'Done'";

                using (SqlCommand cm = new SqlCommand(query, cn))
                {
                    cm.Parameters.AddWithValue("@startDate", date1.Value.Date);
                    cm.Parameters.AddWithValue("@endDate", date2.Value.Date);

                    using (SqlDataReader dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            i++;

                            dataGridView1.Rows.Add(
                                i,                                          // Nº
                                dr["id"].ToString(),                        // ID
                                dr["refno"].ToString(),                     // Nº Ref.
                                dr["pcode"].ToString(),                     // Código
                                dr["pdesc"].ToString(),                     // Descrição
                                dr["qty"].ToString(),                       // Quantidade
                                Convert.ToDateTime(dr["sdate"]).ToString("dd/MM/yyyy"), // Data
                                dr["stockinby"].ToString(),
                                dr["vendedor"].ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar histórico: \n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close(); // Fecha a conexão mesmo se der erro
            }
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadStockInHistory();
        }

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            // Obtém o nome da coluna clicada
            string nomeColuna = dataGridView2.Columns[e.ColumnIndex].Name;

            // Verifica se o nome da coluna é "colDelete" (ícone de lixeira)
            if (nomeColuna == "colDelete")
            {
                // Pergunta ao usuário se deseja remover o item
                if (MessageBox.Show("Remover este item?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open(); // Abre a conexão com o banco de dados

                    // Comando para deletar da tabela tblstockin o item com o ID selecionado
                    cm = new SqlCommand("DELETE FROM tblStockIn WHERE id = '" + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() + "'", cn);
                    cm.ExecuteNonQuery(); // Executa o comando
                    cn.Close(); // Fecha a conexão

                    // Mostra mensagem de sucesso
                    MessageBox.Show("Item removido com sucesso!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadStockIn(); // Recarrega a tabela de estoque
                }

            }
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cbxFornecedor_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CarregarFornecedores()
        {
            try
            {
                cn.Open();
                cm = new SqlCommand("SELECT id, vendedor, contactperson, address FROM tblVendor ORDER BY vendedor", cn);
                dr = cm.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(dr);

                cbxFornecedor.DataSource = dt;
                cbxFornecedor.DisplayMember = "vendedor";
                cbxFornecedor.ValueMember = "id"; // armazenamos o ID internamente

                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao carregar fornecedores: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbxFornecedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cbxFornecedor.SelectedValue.ToString(), out int fornecedorId))
            {
                try
                {
                    cn.Open();
                    cm = new SqlCommand("SELECT contactperson, address FROM tblVendor WHERE id = @id", cn);
                    cm.Parameters.AddWithValue("@id", fornecedorId);
                    dr = cm.ExecuteReader();

                    if (dr.Read())
                    {
                        txtContactPerson.Text = dr["contactperson"].ToString();
                        txtEndereco.Text = dr["address"].ToString();

                        lblFornecedorId.Text = fornecedorId.ToString(); // ✅ Atualiza o ID no Label oculto
                    }

                    dr.Close();
                    cn.Close();
                }
                catch (Exception ex)
                {
                    cn.Close();
                    MessageBox.Show("Erro ao buscar dados do fornecedor: " + ex.Message);
                }
            }
        }
    }
}