using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace lojaCanuma
{
    public partial class frmSearchProduct_Stockin : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        SqlDataReader dr;
        string stitle = "Sistema de Vendas(morcanuma)";
        frmStockIn slist;
        public frmSearchProduct_Stockin(frmStockIn flist)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            slist = flist;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadProduct()
        {
            try
            {
                cn.Open();

                string query = "SELECT pcode, pdesc, qty FROM tblProduct WHERE pdesc LIKE '%" + txtSearch.Text + "%' ORDER BY pdesc";
                cm = new SqlCommand(query, cn);

                SqlDataAdapter da = new SqlDataAdapter(cm);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;

                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Obtém o nome da coluna clicada na DataGridView
            string nomeColuna = dataGridView1.Columns[e.ColumnIndex].Name;

            // Verifica se a coluna clicada foi a "colSelect"
            if (nomeColuna == "colSelect")
            {
                if (string.IsNullOrWhiteSpace(slist.txtEndereco.Text))
                {
                    MessageBox.Show("Por favor, selecione um fornecedor válido para preencher o endereço.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    slist.cbxFornecedor.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(slist.txtContactPerson.Text))
                {
                    MessageBox.Show("Por favor, selecione um fornecedor válido para preencher o contacto.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    slist.txtEndereco.Focus();
                    return;
                }


                // Mostra uma mensagem de confirmação para o usuário
                if (MessageBox.Show("Adicionar este item?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) ;
                {
                    cn.Open(); // Abre a conexão com o banco de dados

                    // Prepara o comando SQL para inserir um novo registro na tabela tblstockin
                    cm = new SqlCommand("INSERT INTO tblstockin (refno, pcode, sdate, stockinby, vendorid) VALUES (@refno, @pcode, @sdate, @stockinby, @vendorid)", cn);

                    // Define os valores dos parâmetros com os dados dos controles na tela
                    cm.Parameters.AddWithValue("@refno", slist.txtRefNo.Text); // Número de referência
                    cm.Parameters.AddWithValue("@pcode", dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString()); // Código do produto
                    cm.Parameters.AddWithValue("@sdate", slist.dt1.Value); // Data do estoque
                    cm.Parameters.AddWithValue("@stockinby", slist.txtBy.Text); // Nome de quem está estocando
                    cm.Parameters.AddWithValue("@vendorid", slist.lblFornecedorId.Text); 


                    cm.ExecuteNonQuery(); // Executa o comando (insere no banco)
                    cn.Close(); // Fecha a conexão

                    // Mostra mensagem de sucesso
                    MessageBox.Show("Adicionado com sucesso!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    slist.LoadStockIn();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProduct();
        }
    }
}