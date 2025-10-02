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
    public partial class frmProductListcs : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        SqlDataReader dr;
        private bool isUpdate = false;

        public frmProductListcs()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            frmProduct frm = new frmProduct(this);
            frm.btnSave.Enabled = true;
            frm.btnUpdate.Enabled = false;
            frm.LoadBrand();
            frm.LoadCategory();
            frm.ShowDialog();
            

        }

        public void LoadRecords()
        {
            dataGridView1.Rows.Clear();
            cn.Open();

            // ADICIONE p.cost_price na consulta
            cm = new SqlCommand(
                "SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.reorder, p.cost_price " +
                "FROM tblProduct AS p " +
                "INNER JOIN tblBrand AS b ON b.id = p.bid " +
                "INNER JOIN tblCategory AS c ON c.id = p.cid " +
                "WHERE p.pdesc LIKE '" + txtSearch.Text + "%'", cn);

            dr = cm.ExecuteReader();
            int i = 0;

            while (dr.Read())
            {
                i++;
                dataGridView1.Rows.Add(
                    i,                      // índice 0
                    dr[0].ToString(),       // índice 1 (pcode)
                    dr[1].ToString(),       // índice 2 (barcode)
                    dr[2].ToString(),       // índice 3 (pdesc)
                    dr[3].ToString(),       // índice 4 (brand)
                    dr[4].ToString(),       // índice 5 (category)
                    dr[5].ToString(),       // índice 6 (price)
                    dr[6].ToString(),       // índice 7 (reorder)
                    dr[7].ToString()        // índice 8 (cost_price) - NOVO CAMPO
                );
            }

            dr.Close();
            cn.Close();

            // Configuração das colunas (apenas as que você quer modificar)
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Aumenta apenas a coluna da descrição do produto
            dataGridView1.Columns[3].Width = 200;  // Descrição do produto (aumentado para 300 pixels)

            // Reduz as colunas numéricas
            dataGridView1.Columns[6].Width = 90;  // Preço de venda (reduzido)
            dataGridView1.Columns[7].Width = 90;  // Estoque mínimo (reduzido) 
            dataGridView1.Columns[8].Width = 90;  // Preço de compra (reduzido)

            // Mantém o texto completo visível sem quebra de linha
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == "Edit")
            {
                // Abre o formulário para editar produto
                frmProduct frm = new frmProduct(this);
                frm.isUpdate = true; // ← ESSA LINHA É O QUE FALTAVA
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.LoadBrand();
                frm.LoadCategory();

                // Preenche os campos com os valores da linha selecionada
                frm.txtProdCode.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(); // Código
                frm.txtBarcode.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();  // Código de barras
                frm.txtDescription.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString(); // Descrição
                frm.cbxBrand.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString(); // Marca
                frm.cbxCategory.Text = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString(); // Categoria
                frm.txtPreco.Text = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString(); // Preço
                frm.txtReorder.Text = dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString(); // Reorder level
                frm.txtPrecoCompra.Text = dataGridView1.Rows[e.RowIndex].Cells[8].Value.ToString();





                frm.ShowDialog();
            }
            else if(colName == "Delete")
            {
                // Confirmação antes de excluir
                if (MessageBox.Show("Tens certeza que queres eliminar este registro?",
                                    "Eliminar Produto",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Abre a conexão com o banco de dados
                    cn.Open();

                    // Prepara o comando SQL para excluir com base no código do produto
                    cm = new SqlCommand("DELETE FROM tblProduct WHERE pcode = @pcode", cn);
                    cm.Parameters.AddWithValue("@pcode", dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());

                    cm.ExecuteNonQuery(); // Executa o DELETE
                    cn.Close();           // Fecha a conexão
                   
                    LoadRecords();        // Atualiza a tabela exibida
                }
            }

        }

        private void frmProductListcs_Load(object sender, EventArgs e)
        {
            LoadRecords();


        }
    }
}
