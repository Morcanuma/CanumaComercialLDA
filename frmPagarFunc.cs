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
   
    public partial class frmPagarFunc : Form
    {
        private int editingPagamentoId = 0;
        DBConnection db = new DBConnection();

        public frmLucroEOutros FrmPai { get; set; }
        // valores que voltam para o frmLucroEOutros
        public string FuncionarioSelecionado { get; private set; }
        public decimal ValorPago { get; private set; }
        public DateTime DataPagamento { get; private set; }
        public frmPagarFunc()
        {
            InitializeComponent();
            CarregarFuncionarios();
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CarregarFuncionarios()
        {
            using (var cn = new SqlConnection(new DBConnection().MyConnection()))
            using (var cmd = new SqlCommand("SELECT name FROM tblUser", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        cbFuncionario.Items.Add(dr.GetString(0));
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

        private void frmPagarFunc_Load(object sender, EventArgs e)
        {

            CarregarPagamentos();

            if (FrmPai != null)
            {
                string valor = FrmPai.lblSaldoLucroAtual.Text;
                labelDisponivel.Text = "Valor disponível para despesa: " + valor;
            }
        }

        private void btnAddDespesa_Click_1(object sender, EventArgs e)
        {
            try
            {
                // 1) Validação básica dos campos obrigatórios
                if (cbFuncionario.SelectedIndex < 0
                    || string.IsNullOrWhiteSpace(txtValor.Text)
                    || string.IsNullOrWhiteSpace(txtObservacao.Text))
                {
                    MessageBox.Show(
                        "⚠ Campos obrigatórios faltando!\n\n• Selecione um funcionário\n• Preencha o valor\n• Adicione uma observação",
                        "Dados Incompletos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // 2) Validação do valor numérico
                if (!decimal.TryParse(txtValor.Text, out decimal valor) || valor <= 0)
                {
                    MessageBox.Show(
                        $"Valor inválido: '{txtValor.Text}'\n\nDigite um valor numérico maior que zero (Ex: 25000 ou 12500.50)",
                        "Formato Incorreto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtValor.Focus();
                    txtValor.SelectAll();
                    return;
                }

                // 3) Verificação do saldo disponível (AGORA COM VALIDAÇÃO EFETIVA)
                string textoLabel = FrmPai.lblSaldoLucroAtual.Text.Replace("Kz", "").Trim();
                if (!decimal.TryParse(textoLabel, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal valorDisponivel))
                {
                    MessageBox.Show("⚠ Não foi possível verificar o saldo disponível",
                                    "Erro de Sistema",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                // VALIDAÇÃO CRÍTICA ADICIONADA AQUI
                if (valor > valorDisponivel)
                {
                    decimal deficit = valor - valorDisponivel;
                    MessageBox.Show(
                        $"🚫 Saldo insuficiente!\n\n" +
                        $"Valor do pagamento: {valor:N2} Kz\n" +
                        $"Saldo disponível: {valorDisponivel:N2} Kz\n" +
                        $"Faltam: {deficit:N2} Kz",
                        "Limite Excedido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // 4) Confirmação explícita antes de gravar
                var confirmacao = MessageBox.Show(
                    $"Confirmar pagamento de {valor:N2} Kz para {cbFuncionario.Text}?\n\n" +
                    $"Saldo após pagamento: {(valorDisponivel - valor):N2} Kz\n\n" +
                    $"Observação: {txtObservacao.Text.Trim()}",
                    "Confirmação de Pagamento",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirmacao != DialogResult.Yes) return;

                // 5) Processamento do pagamento
                FuncionarioSelecionado = cbFuncionario.Text;
                ValorPago = valor;
                DataPagamento = DateTime.Now;

                using (var cn = new SqlConnection(new DBConnection().MyConnection()))
                using (var cmd = new SqlCommand(
                    @"INSERT INTO tblPagamentosFuncionarios
              (funcionario, valor, data_pagamento, observacao)
              VALUES (@f, @v, @d, @o)", cn))
                {
                    cmd.Parameters.AddWithValue("@f", FuncionarioSelecionado);
                    cmd.Parameters.AddWithValue("@v", ValorPago);
                    cmd.Parameters.AddWithValue("@d", DataPagamento);
                    cmd.Parameters.AddWithValue("@o", txtObservacao.Text.Trim());

                    cn.Open();
                    cmd.ExecuteNonQuery();
                    editingPagamentoId = 0;            // limpa a flag
                }

                // 6) Atualização do saldo disponível na interface
                decimal novoSaldo = valorDisponivel - valor;
                FrmPai.lblSaldoLucroAtual.Text = novoSaldo.ToString("N2") + " Kz";

                // 7) Feedback de sucesso detalhado
                MessageBox.Show(
                    $"✅ Pagamento registrado com sucesso!\n\n" +
                    $"Funcionário: {FuncionarioSelecionado}\n" +
                    $"Valor: {ValorPago:N2} Kz\n" +
                    $"Novo saldo: {novoSaldo:N2} Kz\n" +
                    $"Data: {DataPagamento:dd/MM/yyyy HH:mm}",
                    "Pagamento Efetuado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                CarregarPagamentos();
               
                this.Close();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    $"⛔ Erro de banco de dados:\n{sqlEx.Message}\n\nCódigo: {sqlEx.Number}",
                    "Falha na Conexão",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"⛔ Erro inesperado:\n{ex.Message}",
                    "Falha no Sistema",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CarregarPagamentos()
        {
            try
            {
                // Limpa a grid antes de carregar novos dados
                dtgVPagamento.Rows.Clear();

                const string sql = @"
            SELECT 
                id,   
                data_pagamento,
                funcionario,
                valor,
                observacao
            FROM tblPagamentosFuncionarios
            ORDER BY data_pagamento DESC"; // mostra os mais recentes no topo

                using (var cn = new SqlConnection(db.MyConnection()))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            dtgVPagamento.Rows.Add(
                                dr.GetInt32(0),
                                dr.GetDateTime(1).ToString("dd/MM/yyyy HH:mm"),
                                dr.GetString(2),
                                dr.GetDecimal(3).ToString("N2") + " Kz",
                                dr.IsDBNull(4) ? string.Empty : dr.GetString(4)
                            );
                        }
                    }
                }

                // Atualiza o total de salários pagos
                using (var cn2 = new SqlConnection(db.MyConnection()))
                {
                    cn2.Open();
                    decimal total = ObterTotal(cn2,
                        "SELECT ISNULL(SUM(valor),0) FROM tblPagamentosFuncionarios");

                    lblTotalSalarioPagos.Text = $"Valor Total em Salários: {total:N2} Kz (Kwanza)";
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Erro ao carregar pagamentos:\n{ex.Message}",
                                "Erro de Banco de Dados",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado:\n{ex.Message}",
                                "Erro",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        // Método auxiliar para obter totais (deve estar em sua classe)
        private decimal ObterTotal(SqlConnection cn, string query, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(query, cn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                object result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        private void dtgVPagamento_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                var grid = (DataGridView)sender;
                string colunaClicada = grid.Columns[e.ColumnIndex].Name;
                var id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Id"].Value);

                // AÇÃO DE EXCLUSÃO
                if (colunaClicada == "Delete")
                {
                    var confirmacao = MessageBox.Show($"Deseja realmente excluir o pagamento ID {id}?",
                                                    "Confirmação de Exclusão",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Warning,
                                                    MessageBoxDefaultButton.Button2);

                    if (confirmacao != DialogResult.Yes) return;

                    using (var cn = new SqlConnection(db.MyConnection()))
                    using (var cmd = new SqlCommand("DELETE FROM tblPagamentosFuncionarios WHERE id = @id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cn.Open();
                        int registrosAfetados = cmd.ExecuteNonQuery();

                        if (registrosAfetados > 0)
                        {
                            MessageBox.Show($"Pagamento ID {id} excluído com sucesso!",
                                          "Operação Concluída",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Information);
                        }
                    }
                    CarregarPagamentos();
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}",
                                "Operação Falhou",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        
    }
}

