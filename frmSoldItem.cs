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
    public partial class frmSoldItem : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        string nomeLogado = SessaoUsuario.Nome;
        frmPOS f;
        public frmSoldItem(frmPOS frm)
        {
            InitializeComponent();
          

            cn = new SqlConnection(dbcon.MyConnection());
            dt1.Value = DateTime.Now;
            dt2.Value = DateTime.Now;
            f = frm;

            LoadFuncionario();
            LoadRecord();
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadRecord()
        {
            int i = 0;
            double totalGeral = 0;
            dataGridView1.Rows.Clear();

            try
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
                cn.Open();

                string nomeLogado = SessaoUsuario.Nome;
                string cargoLogado = SessaoUsuario.Cargo;

                string query = @"
        SELECT 
            c.id, c.transno, c.pcode, p.pdesc, 
            c.price, c.qty, c.disc AS discount, 
            c.total, c.sdate 
        FROM tblCar AS c 
        INNER JOIN tblProduct AS p ON c.pcode = p.pcode 
        WHERE c.status = 'Sold' 
          AND c.sdate BETWEEN @date1 AND @date2";

                cm = new SqlCommand(query, cn);
                cm.Parameters.AddWithValue("@date1", dt1.Value.Date);
                cm.Parameters.AddWithValue("@date2", dt2.Value.Date.AddDays(1).AddTicks(-1));

                // ⚠️ Aqui vem o controle de filtro por funcionário
                if (!cargoLogado.Equals("Administrador do Sistema", StringComparison.OrdinalIgnoreCase))
                {
                    // Operador: sempre filtra pelo próprio nome
                    query += " AND c.funcionario = @funcionario";
                    cm.CommandText = query;
                    cm.Parameters.AddWithValue("@funcionario", nomeLogado);
                }
                else if (cboFuncionario.Text != "Todos")
                {
                    // Admin: filtra se não for "Todos"
                    query += " AND c.funcionario = @funcionario";
                    cm.CommandText = query;
                    cm.Parameters.AddWithValue("@funcionario", cboFuncionario.Text.Trim());
                }

                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    i++;
                    double.TryParse(dr["total"].ToString(), out double totalItem);
                    totalGeral += totalItem;

                    dataGridView1.Rows.Add(
                        i,
                        dr["id"].ToString(),
                        dr["transno"].ToString(),
                        dr["pcode"].ToString(),
                        dr["pdesc"].ToString(),
                        dr["price"].ToString(),
                        dr["qty"].ToString(),
                        dr["discount"].ToString(),
                        dr["total"].ToString(),
                        Convert.ToDateTime(dr["sdate"]).ToString("dd/MM/yyyy HH:mm:ss")
                    );
                }

                dr.Close();
                lblTotal.Text = totalGeral.ToString("N2") + " Kzs";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar registros: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }







        private void dt1_ValueChanged(object sender, EventArgs e)
        {
            LoadRecord();
        }

        private void dt2_ValueChanged(object sender, EventArgs e)
        {
            LoadRecord();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            frmReportSold frm = new frmReportSold(this);
            frm.LoadReport();
            frm.ShowDialog();
        }

        private void cboFuncionario_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Define que o evento foi tratado manualmente e o sistema não deve aplicar o comportamento padrão.
            // Por exemplo: ao pressionar uma tecla inválida, essa linha impede que o caractere apareça no controle.
            e.Handled = true;

        }

        public void LoadFuncionario()
        {
            cboFuncionario.Items.Clear(); // Limpa o ComboBox

            try
            {
                cn.Open();

                // Captura os dados da sessão
                string cargoLogado = SessaoUsuario.Cargo;
                string nomeLogado = SessaoUsuario.Nome;

                // Se for administrador, carrega todos os operadores + opção "Todos"
                if (cargoLogado == "Administrador do Sistema")
                {
                    cboFuncionario.Items.Add("Todos"); // Adiciona a opção especial "Todos"

                    string query = "SELECT name FROM tbluser WHERE role = @role";
                    cm = new SqlCommand(query, cn);
                    cm.Parameters.AddWithValue("@role", "Operador de Caixa");

                    dr = cm.ExecuteReader();

                    while (dr.Read())
                    {
                        cboFuncionario.Items.Add(dr["name"].ToString());
                    }

                    dr.Close();
                    cboFuncionario.Enabled = true;      // Permite selecionar
                    cboFuncionario.SelectedIndex = 0;   // Seleciona "Todos" por padrão
                }
                else
                {
                    // Funcionário comum: só vê o próprio nome e não pode alterar
                    cboFuncionario.Items.Add(nomeLogado);
                    cboFuncionario.SelectedIndex = 0;
                    cboFuncionario.Enabled = false;
                    dt1.Enabled = false;
                    dt2.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar operadores de caixa:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }


        private void cboFuncionario_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecord();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "colCancel")
            {
                frmCancelDetais f = new frmCancelDetais(this);
                f.txtId.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                f.txtTransno.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                f.txtCProd.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                f.txtDesc.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                f.txtPrice.Text = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                f.txtQty.Text = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString();
                f.txtDiscount.Text = dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString();
                f.txtTotal.Text = dataGridView1.Rows[e.RowIndex].Cells[8].Value.ToString();
                f.txtCancel.Text = nomeLogado;

                f.ShowDialog();
            }

        }
    }
}
