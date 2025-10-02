using lojaCanuma.Models;
using lojaCanuma.Services;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmDiscount : Form
    {
        private readonly ItemVenda _item;          // recebido do POS (Id da tblCar + preço unitário original)
        private readonly frmPOS _frmPrincipal;

        // serviços/regras
        private readonly string _cn = new DBConnection().MyConnection();
        private readonly LoyaltyService _loySvc;
        private LoyaltySettings _set;

        // dados da linha
        private int _qty = 0;
        private decimal _unitPrice = 0m;
        private decimal _unitCost = 0m;    // tblProduct.cost_price
        private decimal _subtotal = 0m;    // price * qty

        // limites calculados
        private decimal _maxByPct = 0m;       // teto por % (subtotal * MaxDiscountPct)
        private decimal _maxByMargin = 0m;    // teto por margem mínima
        private decimal _maxAllowed = 0m;     // min(tetos)

        public frmDiscount(frmPOS frm, ItemVenda item)
        {
            InitializeComponent();
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _frmPrincipal = frm ?? throw new ArgumentNullException(nameof(frm));

            _loySvc = new LoyaltyService(_cn);
            _set = _loySvc.GetSettings();

            // carrega dados da linha direto do banco (qty, price confirmado, cost_price)
            LoadLineData();

            // mostra preço unitário atual
            txtPrice.Text = _unitPrice.ToString("N2");
            // limpa campos de desconto
            txtPercent.Text = "";
            txtAmount.Text = "0,00";
        }

        private void LoadLineData()
        {
            using (var cn = new SqlConnection(_cn))
            using (var cmd = new SqlCommand(@"
                SELECT c.pcode, c.price, c.qty, p.cost_price
                FROM tblCar c
                INNER JOIN tblProduct p ON p.pcode = c.pcode
                WHERE c.id = @id;", cn))
            {
                cmd.Parameters.AddWithValue("@id", _item.Id);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        throw new InvalidOperationException("Linha do carrinho não encontrada.");

                    _unitPrice = r["price"] == DBNull.Value ? 0m : Convert.ToDecimal(r["price"], CultureInfo.InvariantCulture);
                    _qty = r["qty"] == DBNull.Value ? 0 : Convert.ToInt32(r["qty"]);
                    _unitCost = r["cost_price"] == DBNull.Value ? 0m : Convert.ToDecimal(r["cost_price"], CultureInfo.InvariantCulture);
                }
            }

            _subtotal = _unitPrice * _qty;

            // teto por % do subtotal
            _maxByPct = Math.Round(_subtotal * (_set.MaxDiscountPct / 100m), 2);

            // teto por margem mínima:
            // queremos: ((subtotal - desconto) - costTotal) / subtotal >= MinGrossMarginPct
            // ⇒ desconto <= subtotal - costTotal - (subtotal * MinGrossMarginPct)
            var costTotal = _unitCost * _qty;
            var margemMinValor = Math.Round(_subtotal * (_set.MinGrossMarginPct / 100m), 2);
            _maxByMargin = _subtotal - costTotal - margemMinValor;
            if (_maxByMargin < 0) _maxByMargin = 0m;

            _maxAllowed = Math.Min(_maxByPct, _maxByMargin);
            if (_maxAllowed < 0) _maxAllowed = 0m;

            // dica visual (se tiver um Label de ajuda, opcional)
            this.Text = $"Desconto no item (Qtd: {_qty}) — Máx: {_maxAllowed:N2} Kz";
        }

        private static bool TryParseDecimal(string s, out decimal v)
        {
            s = (s ?? "").Trim();

            // tenta invariável e culturas comuns pt
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out v)
                || decimal.TryParse(s, NumberStyles.Number, new CultureInfo("pt-PT"), out v)
                || decimal.TryParse(s, NumberStyles.Number, new CultureInfo("pt-BR"), out v);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        // Quando o usuário digita o percentual, calculamos o valor em Kz sobre o SUBTOTAL (price * qty)
        private void txtPercent_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPercent.Text))
            {
                txtAmount.Text = "0,00";
                return;
            }

            if (TryParseDecimal(txtPercent.Text.Replace("%", ""), out var percent))
            {
                var valor = Math.Round(_subtotal * (percent / 100m), 2);
                txtAmount.Text = valor.ToString("N2");
            }
            else
            {
                txtAmount.Text = "0,00";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Usa o valor em Kz; se preferir, pode recalcular de txtPercent
                if (!TryParseDecimal(txtAmount.Text.Replace("Kz", "").Replace("Kzs", ""), out var descontoSolicitado))
                {
                    MessageBox.Show("Valor de desconto inválido.", "Desconto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (descontoSolicitado < 0) descontoSolicitado = 0m;

                // aplica os limites calculados
                if (descontoSolicitado > _maxAllowed)
                {
                    var msg = $"O desconto solicitado ({descontoSolicitado:N2} Kz) excede o máximo permitido ({_maxAllowed:N2} Kz)\n" +
                              $"• Teto por % ({_set.MaxDiscountPct:N2}%): {_maxByPct:N2} Kz\n" +
                              $"• Teto por margem mínima ({_set.MinGrossMarginPct:N2}%): {_maxByMargin:N2} Kz\n\n" +
                              $"Deseja aplicar o MÁXIMO permitido?";
                    var ask = MessageBox.Show(msg, "Proteção de lucro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (ask == DialogResult.Yes)
                        descontoSolicitado = _maxAllowed;
                    else
                        return;
                }

                // não deixa o total da linha ficar negativo
                if (descontoSolicitado > _subtotal) descontoSolicitado = _subtotal;

                using (var cn = new SqlConnection(_cn))
                using (var cm = new SqlCommand(
                    "UPDATE tblCar SET disc = @disc, total = (price * qty) - @disc WHERE id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@disc", descontoSolicitado);
                    cm.Parameters.AddWithValue("@id", _item.Id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }

                _frmPrincipal.LoadCar(); // Atualiza totais no POS
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao aplicar desconto: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
