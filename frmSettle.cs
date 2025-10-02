using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmSettle : Form
    {

        
        frmPOS fpos;
        string stitle = "Sistema de Vendas(morcanuma)";
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public frmSettle(frmPOS fp)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            fpos = fp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose(); 
        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double total = double.Parse(txtSale.Text.Replace("Kzs", "").Trim());
                double cash = double.Parse(txtCash.Text.Replace("Kzs", "").Trim());
                double change = cash - total;
                txtChange.Text = change.ToString("N2") + " Kzs";

            }
            catch (Exception)
            {
                // 4. Se ocorrer erro (ex: texto inválido), define o troco como zero
                txtChange.Text = "0.00";
            }
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn7.Text;
        }

        private void btn8_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn8.Text;
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn9.Text;
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            txtCash.Clear();
            txtCash.Focus();
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn4.Text;
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn5.Text;
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn6.Text;
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn0.Text;
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn1.Text;
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn2.Text;
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn3.Text;
        }

        private void btn00_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn00.Text;
        }


        private void btnEnter_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ Limpa os textos para garantir o formato numérico
                if (!double.TryParse(txtSale.Text.Replace("Kzs", "").Replace(",", "").Trim(), out double totalVenda))
                {
                    MessageBox.Show("Erro ao ler o valor total da venda. Verifique o campo 'Total'.", "Erro de Entrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!double.TryParse(txtCash.Text.Replace("Kzs", "").Replace(",", "").Trim(), out double valorPago))
                {
                    MessageBox.Show("Erro ao ler o valor pago. Verifique o campo 'Pagamento'.", "Erro de Entrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (valorPago < totalVenda)
                {
                    MessageBox.Show("O valor pago é inferior ao total da venda. Corrija para prosseguir.", "Pagamento Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cn.Open();


                // ──── 1) Vincular CustomerId à venda ─────────────────────────────
                int custId = 0;
                if (fpos.cmbCustomer.SelectedValue != null
                    && int.TryParse(fpos.cmbCustomer.SelectedValue.ToString(), out var tmp))
                    custId = tmp;

                if (custId > 0)
                {
                    using (var cmdCust = new SqlCommand(
                        "UPDATE tblCar SET CustomerId = @custId WHERE transno = @transno AND status = 'Pending'",
                        cn))
                    {
                        cmdCust.Parameters.AddWithValue("@custId", custId);
                        cmdCust.Parameters.AddWithValue("@transno", fpos.lblTransno.Text);
                        cmdCust.ExecuteNonQuery();
                    }

                    if (custId > 0)
                    {
                        var repo = new CustomerRepository();
                        var cliente = repo.GetById(custId);
                        fpos.pontosAntesDaCompra = cliente?.Points ?? 0;
                    }

                }
                // ────────────────────────────────────────────────────────────────



                // Verifica estoque produto a produto
                foreach (DataGridViewRow row in fpos.dataGridView1.Rows)
                {
                    string codigo = row.Cells[2].Value?.ToString();
                    int qtdVendida = int.Parse(row.Cells[5].Value.ToString());

                    cm = new SqlCommand("SELECT qty FROM tblProduct WHERE pcode = @codigo", cn);
                    cm.Parameters.AddWithValue("@codigo", codigo);

                    object result = cm.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show($"Produto com código '{codigo}' não encontrado no sistema.", "Produto Inexistente", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int estoqueAtual = Convert.ToInt32(result);
                    if (estoqueAtual < qtdVendida)
                    {
                        MessageBox.Show($"Estoque insuficiente para o produto '{codigo}'.\nDisponível: {estoqueAtual} | Solicitado: {qtdVendida}", "Estoque Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Atualiza estoque
                    cm = new SqlCommand("UPDATE tblProduct SET qty = qty - @qtd WHERE pcode = @codigo", cn);
                    cm.Parameters.AddWithValue("@qtd", qtdVendida);
                    cm.Parameters.AddWithValue("@codigo", codigo);
                    cm.ExecuteNonQuery();
                }





                // Atualiza o status da venda
                cm = new SqlCommand("UPDATE tblCar SET status = 'Sold', sdate = @datahora WHERE transno = @transno", cn);
                cm.Parameters.AddWithValue("@datahora", DateTime.Now);
                cm.Parameters.AddWithValue("@transno", fpos.lblTransno.Text);
                cm.ExecuteNonQuery();



                // 3) Pontos ao cliente (premiar e marcar última compra)
                if (custId > 0)
                {
                    var loy = new lojaCanuma.Services.LoyaltyService(dbcon.MyConnection());
                    var set = loy.GetSettings();

                    if (set.IsEnabled) // ✅ só premia se o programa estiver ligado
                    {
                        decimal totalLiquido = Convert.ToDecimal(totalVenda);
                        loy.AwardPoints(custId, totalLiquido, fpos.lblTransno.Text);
                    }

                    // Se sua SP JÁ atualiza LastPurchaseAt, APAGUE este bloco:
                    using (var cmdLast = new SqlCommand(
                        "UPDATE tblCustomer SET LastPurchaseAt = GETDATE() WHERE CustomerId = @id", cn))
                    {
                        cmdLast.Parameters.AddWithValue("@id", custId);
                        cmdLast.ExecuteNonQuery();
                    }
                }

            

                // ────────────────────────────────────────────────────────────────

                frmReceipt frm = new frmReceipt(fpos);
                frm.LoadReport(txtCash.Text, txtChange.Text);
                frm.ShowDialog();
                fpos.AtualizarPontosCliente();


                MessageBox.Show("Venda concluída com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Atualiza a tela principal
                fpos.LoadCar();
                fpos.GetTransNo();
                // >>> ADICIONE AQUI:
                this.DialogResult = DialogResult.OK;
                this.Close();
                this.Close();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Erro ao acessar o banco de dados. Por favor, tente novamente ou contacte o suporte.\n\nDetalhes técnicos: " + sqlEx.Message, "Erro de Banco de Dados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FormatException formatEx)
            {
                MessageBox.Show("Erro no formato dos valores inseridos. Verifique se os campos numéricos estão corretos.\n\n" + formatEx.Message, "Erro de Formato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro inesperado. Por favor, tente novamente.\n\nDetalhes técnicos: " + ex.Message, "Erro Inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }

        }


    }
}
