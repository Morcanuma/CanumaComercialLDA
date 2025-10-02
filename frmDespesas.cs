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
    public partial class frmDespesas : Form
    {
        DBConnection db = new DBConnection();
        public frmLucroEOutros FrmPai { get; set; }

        public frmDespesas()
        {
            InitializeComponent();
            cbTipo.Items.AddRange(new object[]
                {
                    "Aluguel do espaço",
                    "Compra de ingredientes",
                    "Energia elétrica",
                    "Água",
                    "Gás de cozinha",
                    "Salários de funcionários",
                    "Transporte e entrega",
                    "Marketing e divulgação",
                    "Materiais descartáveis",
                    "Manutenção de equipamentos",
                    "Utensílios de cozinha",
                    "Higiene e limpeza",
                    "Taxas e impostos",
                    "Internet e telefone",
                    "Software ou sistema de gestão",
                    "Uniformes e EPIs",
                    "Despesas bancárias",
                    "Emergência / imprevistos"
                });
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void btnAddDespesa_Click(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ VALIDATION PHASE
                // Validate required fields
                if (cbTipo.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtValor.Text) || string.IsNullOrWhiteSpace(txtDescricao.Text))
                {
                    MessageBox.Show("📋 Preencha todos os campos obrigatórios:\n\n• Tipo de despesa\n• Valor\n• Descrição",
                                    "Dados Incompletos",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                // Parse and validate amount
                if (!decimal.TryParse(txtValor.Text, out decimal valorDespesa) || valorDespesa <= 0)
                {
                    MessageBox.Show($"💰 Valor inválido: '{txtValor.Text}'\n\nDigite um valor positivo (Ex: 150000 ou 7500.50)",
                                    "Valor Incorreto",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    txtValor.Focus();
                    txtValor.SelectAll();
                    return;
                }

                // 2️⃣ FUNDS AVAILABILITY CHECK
                // Parse available amount (safer method with culture consideration)
                string textoLabel = FrmPai.lblSaldoLucroAtual.Text.Replace("Kz", "").Trim();
                if (!decimal.TryParse(textoLabel, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal valorDisponivel))
                {
                    MessageBox.Show("⚠ Não foi possível verificar o saldo disponível",
                                    "Erro de Sistema",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                if (valorDespesa > valorDisponivel)
                {
                    decimal deficit = valorDespesa - valorDisponivel;
                    MessageBox.Show($"🚫 Saldo insuficiente!\n\nValor da despesa: {valorDespesa:N2} Kz\n" +
                                   $"Saldo disponível: {valorDisponivel:N2} Kz\n" +
                                   $"Faltam: {deficit:N2} Kz",
                                   "Limite Excedido",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                    return;
                }

                // 3️⃣ USER CONFIRMATION
                var confirmResult = MessageBox.Show($"ℹ Confirmar registro de despesa?\n\n" +
                                                  $"Tipo: {cbTipo.Text}\n" +
                                                  $"Valor: {valorDespesa:N2} Kz\n" +
                                                  $"Descrição: {txtDescricao.Text.Trim()}",
                                                  "Confirmação",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question,
                                                  MessageBoxDefaultButton.Button2);

                if (confirmResult != DialogResult.Yes) return;

                // 4️⃣ DATABASE OPERATION
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tblDespesasFixas 
                (tipo, valor, data_despesa, descricao)
                VALUES (@tipo, @valor, @data, @descricao)", cn))
                    {
                        cmd.Parameters.AddWithValue("@tipo", cbTipo.Text);
                        cmd.Parameters.AddWithValue("@valor", valorDespesa);
                        cmd.Parameters.AddWithValue("@data", DateTime.Now);
                        cmd.Parameters.AddWithValue("@descricao", txtDescricao.Text.Trim());

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception("Nenhum registro foi inserido no banco de dados");
                        }
                    }
                }

                // 5️⃣ UPDATE UI AND SHOW SUCCESS
                decimal novoDisponivel = valorDisponivel - valorDespesa;
                FrmPai.lblSaldoLucroAtual.Text = novoDisponivel.ToString("N2") + " Kz";
                labelDisponivel.Text = $"Valor disponível: {novoDisponivel:N2} Kz";

                MessageBox.Show($"✅ Despesa registrada com sucesso!\n\n" +
                               $"• Tipo: {cbTipo.Text}\n" +
                               $"• Valor: {valorDespesa:N2} Kz\n" +
                               $"• Novo saldo: {novoDisponivel:N2} Kz",
                               "Registro Concluído",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
               
                CarregarDespesasFixas();
                this.Close();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"⛔ Erro no banco de dados:\n{sqlEx.Message}\n\nCódigo: {sqlEx.Number}",
                                "Falha na Operação",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (FormatException)
            {
                MessageBox.Show("Formato numérico inválido. Use apenas números e ponto decimal.",
                                "Erro de Formato",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⛔ Erro inesperado:\n{ex.Message}",
                                "Falha no Sistema",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        private void frmDespesas_Load(object sender, EventArgs e)
        {
            CarregarDespesasFixas();

           


            if (FrmPai != null)
            {
                string valor = FrmPai.lblSaldoLucroAtual.Text;
                labelDisponivel.Text = "Valor disponível para despesa: " + valor;
            }
        }

        private void txtValor_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool teclaDeControle = char.IsControl(e.KeyChar);
            bool numero = char.IsDigit(e.KeyChar);

            if (!teclaDeControle && !numero)
            {
                e.Handled = true;   // bloqueia a tecla
            }
        }

        /// <summary>
        /// Carrega as despesas fixas no grid e atualiza o label de total.
        /// </summary>
        public void CarregarDespesasFixas()
        {
            dtgrdvwDespesas.Rows.Clear();

            const string sql = @"
        SELECT id, tipo, descricao, valor, data_despesa
        FROM tblDespesasFixas
        ORDER BY data_despesa DESC";

            using (var cn = new SqlConnection(db.MyConnection()))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dtgrdvwDespesas.Rows.Add(
                        dr.GetInt32(0),                                // ID
                        dr.GetString(1),                               // Tipo
                        dr.GetString(2),                               // Descrição
                        dr.GetDecimal(3).ToString("N2") + " Kz",       // Valor
                        dr.GetDateTime(4).ToString("dd/MM/yyyy"),      // Data
                        Properties.Resources.icons8_delete_25 // ✅ Imagem na coluna Delete
                    );

                    }
                }
            }

            // Atualiza o total
            using (var cn2 = new SqlConnection(db.MyConnection()))
            {
                cn2.Open();
                using (var cmd2 = new SqlCommand("SELECT ISNULL(SUM(valor), 0) FROM tblDespesasFixas", cn2))
                {
                    decimal total = Convert.ToDecimal(cmd2.ExecuteScalar());
                    lblTotalDespesa.Text = $"Despesas Fixas Totais: {total:N2} Kz (Kwanza)";
                }
            }
        }



        private void dtgrdvwDespesas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1) Se clicou no cabeçalho ou fora de linha, sai
            if (e.RowIndex < 0) return;

            var grid = dtgrdvwDespesas;

            // 2) Detecta se clicou na coluna “Delete”
            if (grid.Columns[e.ColumnIndex].Name != "Delete")
                return;

            // 3) Tenta excluir dentro do try/catch
            try
            {
                // 3.1) Confirmação
                if (MessageBox.Show(
                        "❗ Tem certeza que deseja remover esta despesa?",
                        "Confirmação de Exclusão",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2
                    ) != DialogResult.Yes)
                    return;

                // 3.2) Remove do banco
                int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells[0].Value);
                using (var cn = new SqlConnection(db.MyConnection()))
                using (var cmd = new SqlCommand("DELETE FROM tblDespesasFixas WHERE id = @id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                // 3.3) Atualiza interface
                CarregarDespesasFixas();           // Recarrega a grid de despesas
                  

                // 3.4) Feedback de sucesso
                MessageBox.Show(
                    "✅ Despesa removida com sucesso!",
                    "Exclusão Efetuada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    $"⛔ Erro ao acessar o banco de dados:\n{sqlEx.Message}",
                    "Falha na Exclusão",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"⛔ Ocorreu um erro inesperado ao tentar excluir a despesa:\n{ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmDespesaGrafico frm = new frmDespesaGrafico();
            frm.ShowDialog();
        }

      

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

