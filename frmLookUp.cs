using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmLookUp : Form
    {
        frmPOS f;
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        SqlDataReader dr;
        string stitle = "Sistema de Vendas(morcanuma)";
        public frmLookUp(frmPOS frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadRecords()
        {
            // Limpa todas as linhas existentes do DataGridView antes de carregar novos dados
            dataGridView1.Rows.Clear();

            // Abre a conexão com o banco de dados
            cn.Open();

            // Define o comando SQL com INNER JOINs entre as tabelas de produto, marca e categoria,
            // buscando os campos relevantes
            cm = new SqlCommand(
                "SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty " +
                "FROM tblProduct AS p " +
                "INNER JOIN tblBrand AS b ON b.id = p.bid " +
                "INNER JOIN tblCategory AS c ON c.id = p.cid " +
                "WHERE p.pdesc LIKE '" + txtSearch.Text + "%'", cn);

            // Executa o comando e obtém os resultados
            dr = cm.ExecuteReader();

            int i = 0; // contador para numerar as linhas

            // Loop para ler cada linha do resultado
            while (dr.Read())
            {
                i++; // incrementa o contador

                // Adiciona uma nova linha ao DataGridView com os dados retornados
                dataGridView1.Rows.Add(
                    i,                      // número da linha
                    dr[0].ToString(),      // código do produto (pcode)
                    dr[1].ToString(),      // descrição do produto (pdesc)
                    dr[2].ToString(),      // marca (brand)
                    dr[3].ToString(),      // categoria (category)
                    dr[4].ToString(),       // preço (price)
                    dr[5].ToString(),
                    dr[6].ToString()
                );
            }

            // Fecha o leitor de dados e a conexão
            dr.Close();
            cn.Close();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Selected")
            {
                frmQty frm = new frmQty(f);
                // Chama o método ProductDetails do frmQty e passa os dados do produto: código, preço e número da transação
                frm.ProductDetails(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(), double.Parse(dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString()), f.lblTransno.Text, int.Parse(dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString()));
                frm.ShowDialog();

            }
        }
    }
}
