using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmAjustement : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm;
        SqlDataReader dr;
        DBConnection db = new DBConnection();
        Form1 f;
        int _qty = 0;
        public frmAjustement(Form1 f)
        {
            InitializeComponent();
            cn.ConnectionString = db.MyConnection();
            this.f = f;
        }

        private void frmAjustement_Load(object sender, EventArgs e)
        {
            PrenchertxtBox();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadRecords()
        {
            dataGridView1.Rows.Clear();

            try
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close(); // Fecha antes de abrir de novo

                cn.Open();

                cm = new SqlCommand(
                    "SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty " +
                    "FROM tblProduct AS p " +
                    "INNER JOIN tblBrand AS b ON b.id = p.bid " +
                    "INNER JOIN tblCategory AS c ON c.id = p.cid " +
                    "WHERE p.pdesc LIKE @search", cn);
                cm.Parameters.AddWithValue("@search", txtSearch.Text + "%");

                dr = cm.ExecuteReader();
                int i = 0;

                while (dr.Read())
                {
                    i++;
                    dataGridView1.Rows.Add(
                        i,
                        dr[0].ToString(),
                        dr[1].ToString(),
                        dr[2].ToString(),
                        dr[3].ToString(),
                        dr[4].ToString(),
                        dr[5].ToString(),
                        dr[6].ToString()
                    );
                }

                dr.Close(); // Sempre feche o DataReader
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close(); // Garante que fecha mesmo em erro
            }
        }


        private void PrenchertxtBox()
        {
            // Gerar número de referência único baseado na hora atual
            txtReferencia.Text = DateTime.Now.ToString("yyyyMMddHHmmss");

           
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                MessageBox.Show("Buscando...");
                LoadRecords();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == "Select") // certifique-se que a coluna de botão tem esse nome!
            {
                txtCodigoProduto.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(); // CÓDIGO-P
                txtProduto.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();  // DESCRIÇÃO
                _qty = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString()); // ESTOQUE DISPONÍVEL
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // 🔍 Validação dos campos obrigatórios
                if (string.IsNullOrWhiteSpace(txtQuantidade.Text) ||
                    cbxAcao.SelectedIndex == -1 ||
                    string.IsNullOrWhiteSpace(txtJustificativa.Text))
                {
                    MessageBox.Show("Por favor, preencha a Quantidade, Ação e Justificativa do Ajuste.", "Campos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtQuantidade.Text, out int ajusteQty) || ajusteQty <= 0)
                {
                    MessageBox.Show("Informe uma quantidade válida maior que zero.", "Quantidade Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string acao = cbxAcao.Text.Trim().ToUpper();

                if (acao != "REMOVER DO INVENTÁRIO" && acao != "ADICIONAR AO INVENTÁRIO")
                {
                    MessageBox.Show("Selecione uma ação válida: 'REMOVER DO INVENTÁRIO' ou 'ADICIONAR AO INVENTÁRIO'.", "Ação Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (acao == "REMOVER DO INVENTÁRIO" && ajusteQty > _qty)
                {
                    MessageBox.Show($"A quantidade a remover ({ajusteQty}) é maior que o estoque disponível ({_qty}).", "Estoque Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ❓ Confirmar a ação com o usuário
                DialogResult resposta = MessageBox.Show(
                    $"Tem certeza que deseja {acao.ToLower()} a quantidade de {ajusteQty} para o produto '{txtProduto.Text}'?",
                    "Confirmação de Ajuste",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resposta != DialogResult.Yes)
                    return;

                // 📦 Atualizar estoque
                string sqlUpdate = acao == "REMOVER DO INVENTÁRIO"
                    ? $"UPDATE tblProduct SET qty = qty - {ajusteQty} WHERE pcode = '{txtCodigoProduto.Text}'"
                    : $"UPDATE tblProduct SET qty = qty + {ajusteQty} WHERE pcode = '{txtCodigoProduto.Text}'";

                SqlStatement(sqlUpdate);

                // 🧾 Registrar ajuste
                string sqlInsert = @"INSERT INTO tblStockAdjustement 
                            (referenceno, pcode, qty, action, remarks, sdate, username)
                            VALUES (@referenceno, @pcode, @qty, @action, @remarks, @sdate, @username)";

                cn.Open();
                cm = new SqlCommand(sqlInsert, cn);
                cm.Parameters.AddWithValue("@referenceno", txtReferencia.Text);
                cm.Parameters.AddWithValue("@pcode", txtCodigoProduto.Text);
                cm.Parameters.AddWithValue("@qty", ajusteQty);
                cm.Parameters.AddWithValue("@action", cbxAcao.Text);
                cm.Parameters.AddWithValue("@remarks", txtJustificativa.Text);
                cm.Parameters.AddWithValue("@sdate", DateTime.Now);
                cm.Parameters.AddWithValue("@username", txtUsuario.Text);
                cm.ExecuteNonQuery();
                cn.Close();

                MessageBox.Show("Ajuste de estoque realizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimparCampos();
                LoadRecords();
            }
            catch (Exception ex)
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();

                MessageBox.Show("Erro ao salvar o ajuste:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        public void SqlStatement(string _sql)
        {
            cn.Open();
            cm = new SqlCommand(_sql, cn);
            cm.ExecuteNonQuery();
            cn.Close();
        }

        private void LimparCampos()
        {
            txtReferencia.Text = DateTime.Now.ToString("yyyyMMddHHmmss");
            txtCodigoProduto.Clear();
            txtProduto.Clear();
            txtQuantidade.Clear();
            cbxAcao.SelectedIndex = -1;
            txtJustificativa.Clear();
            _qty = 0;
        }


    }
}
