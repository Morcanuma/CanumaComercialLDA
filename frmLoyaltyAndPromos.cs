using lojaCanuma.Models;
using lojaCanuma.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmLoyaltyAndPromos : Form
    {
        private readonly string _cn;
        private readonly LoyaltyService _loySvc;
        private readonly PromotionGuardService _guardSvc;
        private LoyaltySettings _cache;
        // ====== Tooltips dinâmicos ======
        private readonly ToolTip tip = new ToolTip();

        public frmLoyaltyAndPromos()
        {
            InitializeComponent();

            _cn = new DBConnection().MyConnection();
            _loySvc = new LoyaltyService(_cn);
            _guardSvc = new PromotionGuardService(_cn);
            ConfigureTooltips();

            this.AcceptButton = btnSalvarConfig;
            this.CancelButton = btnCancelar;
        }

        // ============== EVENTOS BÁSICOS ==============
        private void frmLoyaltyAndPromos_Load(object sender, EventArgs e)
        {
            ConfigureTooltips();
            try
            {
                _cache = _loySvc.GetSettings();
                WriteSettingsToUI(_cache);
                ToggleSections();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar configurações:\n" + ex.Message,
                    "Configurações", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) => this.Close();

        private void btnCancelar_Click_1(object sender, EventArgs e) => this.Close();




        private void chkPromo_CheckedChanged(object sender, EventArgs e) => ToggleSections();


        private void chkEnable_CheckedChanged_1(object sender, EventArgs e) => ToggleSections();



        // ============== SALVAR CONFIGURAÇÕES ==============


        private void btnSalvarConfig_Click(object sender, EventArgs e)
        {
            try
            {
                var s = ReadSettingsFromUI();
                _loySvc.SaveSettings(s);
                _cache = s;
                MessageBox.Show("Configurações salvas com sucesso.",
                    "Configurações", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar configurações:\n" + ex.Message,
                    "Configurações", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        // ============== AUTO-PAUSE (GUARDIÃO) ==============


        private void btnAutoPauseTest_Click_1(object sender, EventArgs e)
        {
            try
            {
                var set = ReadSettingsFromUI();
                var (pausar, motivo, margemMedia) = _guardSvc.EvaluateAutoPause(set, dias: 7);

                lblAutoPauseHint.Text = pausar
                    ? $"⚠ Recomendo PAUSAR. {motivo}"
                    : $"✅ Promoção pode continuar. Margem média: {margemMedia:N1}%";

                if (pausar && set.IsPromoEnabled)
                {
                    var ask = MessageBox.Show(
                        "A promoção está ativa e o guardião recomenda PAUSAR.\n" +
                        "Deseja desativar a promoção agora?",
                        "Auto-pause", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (ask == DialogResult.Yes)
                    {
                        set.IsPromoEnabled = false;
                        _loySvc.SaveSettings(set);
                        WriteSettingsToUI(set);
                        MessageBox.Show("Promoção desativada.", "Auto-pause",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao testar auto-pause:\n" + ex.Message,
                    "Auto-pause", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ============== METAS DO NEGÓCIO (UPSERT) ==============

        private void btnSalvarMetas_Click_1(object sender, EventArgs e)
        {
            try
            {
                var firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                using (var cn = new SqlConnection(_cn))
                using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM dbo.tblBusinessGoals WHERE EffectiveFrom = @d)
                    UPDATE dbo.tblBusinessGoals
                       SET MonthlyRevenueTargetKz = @rev,
                           NetMarginTargetPct      = @netPct,
                           MinGrossMarginPctPerSale= @minSalePct
                     WHERE EffectiveFrom = @d;
                ELSE
                    INSERT INTO dbo.tblBusinessGoals(EffectiveFrom, MonthlyRevenueTargetKz, NetMarginTargetPct, MinGrossMarginPctPerSale)
                    VALUES (@d, @rev, @netPct, @minSalePct);", cn))
                {
                    cmd.Parameters.AddWithValue("@d", firstDay);
                    cmd.Parameters.AddWithValue("@rev", nudMetaFaturamento.Value);
                    cmd.Parameters.AddWithValue("@netPct", nudMetaMargemLiquida.Value);
                    cmd.Parameters.AddWithValue("@minSalePct", nudMargemMinPorVenda.Value);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Metas salvas para " + firstDay.ToString("MM/yyyy"),
                    "Metas do Negócio", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar metas:\n" + ex.Message,
                    "Metas do Negócio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ============== TESTE / SIMULAÇÃO ==============

        private void btnSimularCarrinho_Click_1(object sender, EventArgs e)
        {
            try
            {
                string texto = PromptMultiline(
                @"Cole linhas no formato:
                pcode;preço;kz_custo;qty

                exemplo:
                ABC123;1200;800;2
                DEF999;500;300;1

                Aceita ; , ou TAB como separador.", "Simular carrinho");

                if (string.IsNullOrWhiteSpace(texto))
                    return;

                var linhas = ParseLinesToCart(texto);
                if (linhas.Count == 0)
                {
                    txtResultado.Text = "SEM ITENS";
                    txtMotivo.Text = "Nenhuma linha válida.";
                    return;
                }

                var set = ReadSettingsFromUI();
                var (aplicar, descontoKz, motivo) = _loySvc.ComputeSafeRedeem(set, linhas);

                txtResultado.Text = aplicar ? $"OK (desconto {descontoKz:N2} Kz)" : "BLOQUEADO";
                txtMotivo.Text = aplicar
                    ? "Resgate aprovado pelas regras e pela margem mínima."
                    : motivo;
            }
            catch (Exception ex)
            {
                txtResultado.Text = "ERRO";
                txtMotivo.Text = ex.Message;
            }
        }


        private List<CartLine> ParseLinesToCart(string texto)
        {
            var list = new List<CartLine>();
            var sep = new char[] { ';', ',', '\t' };
            var nl = new[] { "\r\n", "\n" };

            foreach (var raw in texto.Split(nl, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;

                var cl = new CartLine();
                cl.PCode = parts[0].Trim();

                if (!TryParseDecimal(parts[1], out var price)) continue;
                if (!TryParseDecimal(parts[2], out var cost)) continue;
                if (!int.TryParse(parts[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var qty))
                    continue;

                cl.Price = price;
                cl.Cost = cost;
                cl.Qty = qty;

                if (cl.Qty <= 0 || cl.Price < 0 || cl.Cost < 0) continue;

                list.Add(cl);
            }
            return list;
        }

        private bool TryParseDecimal(string s, out decimal value)
        {
            s = (s ?? "").Trim();
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                || decimal.TryParse(s, NumberStyles.Number, new CultureInfo("pt-PT"), out value)
                || decimal.TryParse(s, NumberStyles.Number, new CultureInfo("pt-BR"), out value);
        }

        private string PromptMultiline(string message, string title)
        {
            using (var dlg = new Form
            {
                Width = 620,
                Height = 420,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent
            })
            {
                var lbl = new Label
                {
                    Left = 12,
                    Top = 12,
                    Width = 580,
                    Text = message
                };

                var txt = new TextBox
                {
                    Left = 12,
                    Top = 60,
                    Width = 580,
                    Height = 260,
                    Multiline = true,
                    ScrollBars = ScrollBars.Both
                };

                var ok = new Button
                {
                    Text = "OK",
                    Left = 392,
                    Width = 90,
                    Top = 330,
                    DialogResult = DialogResult.OK
                };

                var cancel = new Button
                {
                    Text = "Cancelar",
                    Left = 502,
                    Width = 90,
                    Top = 330,
                    DialogResult = DialogResult.Cancel
                };

                dlg.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
            }
        }


        // ============== HELPERS DE UI ==============
        private void ToggleSections()
        {
            bool enableProgram = chkEnable.Checked;
            bool enablePromo = chkPromo.Checked && enableProgram;

            grpGanhos.Enabled = enableProgram;
            grpResgate.Enabled = enablePromo;
            grpProtecao.Enabled = true;
            grpMetas.Enabled = true;
            grpTeste.Enabled = enableProgram;
        }

        private LoyaltySettings ReadSettingsFromUI()
        {
            return new LoyaltySettings
            {
                IsEnabled = chkEnable.Checked,
                IsPromoEnabled = chkPromo.Checked,
                KzPerPointEarn = nudKzPerPoint.Value,
                PointsExpireDays = (int)nudExpireDays.Value,
                InactivityMaxDays = (int)nudActivityDays.Value,
                MinPointsToRedeem = (int)nudMinPtsRedeem.Value,
                RedeemUnitPoints = (int)nudPtsPerRedeem.Value,
                RedeemDiscountKz = nudKzPerRedeem.Value,
                MaxDiscountPct = nudMaxPct.Value,
                MinGrossMarginPct = nudMinMargin.Value,
                AutoDisableIfBelowDays = (int)nudAutoPauseDays.Value
            };
        }

        private void WriteSettingsToUI(LoyaltySettings s)
        {
            chkEnable.Checked = s.IsEnabled;
            chkPromo.Checked = s.IsPromoEnabled;
            nudKzPerPoint.Value = Clamp(nudKzPerPoint, s.KzPerPointEarn);
            nudExpireDays.Value = Clamp(nudExpireDays, s.PointsExpireDays);
            nudActivityDays.Value = Clamp(nudActivityDays, s.InactivityMaxDays);
            nudMinPtsRedeem.Value = Clamp(nudMinPtsRedeem, s.MinPointsToRedeem);
            nudPtsPerRedeem.Value = Clamp(nudPtsPerRedeem, s.RedeemUnitPoints);
            nudKzPerRedeem.Value = Clamp(nudKzPerRedeem, s.RedeemDiscountKz);
            nudMaxPct.Value = Clamp(nudMaxPct, s.MaxDiscountPct);
            nudMinMargin.Value = Clamp(nudMinMargin, s.MinGrossMarginPct);
            nudAutoPauseDays.Value = Clamp(nudAutoPauseDays, s.AutoDisableIfBelowDays);
        }

        private decimal Clamp(NumericUpDown nud, decimal value)
        {
            if (value < nud.Minimum) return nud.Minimum;
            if (value > nud.Maximum) return nud.Maximum;
            return value;
        }

        // eventos não usados, só pra não quebrar o designer se já estiverem atribuídos
        private void groupBox1_Enter(object sender, EventArgs e) { /* opcional */ }




        private void ConfigureTooltips()
        {
            tip.AutoPopDelay = 12000;   // fica visível até 12s
            tip.InitialDelay = 250;
            tip.ReshowDelay = 100;
            tip.ShowAlways = true;

            // Mostra tooltip ao passar o mouse (o texto é atualizado na hora)
            grpGanhos.MouseEnter += (_, __) => tip.SetToolTip(grpGanhos, BuildGanhosSummary());
            grpResgate.MouseEnter += (_, __) => tip.SetToolTip(grpResgate, BuildResgateSummary());
            grpProtecao.MouseEnter += (_, __) => tip.SetToolTip(grpProtecao, BuildProtecaoSummary());
            grpMetas.MouseEnter += (_, __) => tip.SetToolTip(grpMetas, BuildMetasSummary());
            grpTeste.MouseEnter += (_, __) => tip.SetToolTip(grpTeste, BuildTesteSummary());

            // Quando qualquer valor mudar, reescreve os tooltips
            void Hook(Control c)
            {
                if (c is NumericUpDown nud) nud.ValueChanged += (_, __) => UpdateTooltips();
                if (c is CheckBox cb)
                {
                    cb.CheckedChanged += (_, __) => UpdateTooltips();
                }
                foreach (Control child in c.Controls) Hook(child);
            }
            Hook(this);

            UpdateTooltips(); // inicial
        }

        private void UpdateTooltips()
        {
            tip.SetToolTip(grpGanhos, BuildGanhosSummary());
            tip.SetToolTip(grpResgate, BuildResgateSummary());
            tip.SetToolTip(grpProtecao, BuildProtecaoSummary());
            tip.SetToolTip(grpMetas, BuildMetasSummary());
            tip.SetToolTip(grpTeste, BuildTesteSummary());
        }

        // -------- textos dinâmicos --------
        private string BuildGanhosSummary()
        {
            // Ex.: “1 ponto a cada 1.000,00 Kz; expira em 60 dias; precisa ter comprado nos últimos 7 dias”
            return
        $@"Programa {(chkEnable.Checked ? "ATIVADO" : "DESATIVADO")}
• Ganha 1 ponto a cada {nudKzPerPoint.Value:N2} Kz de compra.
• Pontos expiram em {nudExpireDays.Value} dia(s).
• Para usar pontos: precisa ter comprado nos últimos {nudActivityDays.Value} dia(s).

Exemplo:
Compra de 5.000 Kz ⇒ 5.000 / {nudKzPerPoint.Value:N2} ≈ {(5000m / (nudKzPerPoint.Value == 0 ? 1 : nudKzPerPoint.Value)):N2} ponto(s) ganho(s).";
        }

        private string BuildResgateSummary()
        {
            // Exemplo com subtotal de 10.000 Kz
            decimal exSubtotal = 10000m;
            decimal tetoPct = Math.Round(exSubtotal * (nudMaxPct.Value / 100m), 2);
            decimal descRegra = nudKzPerRedeem.Value;
            decimal descAplicavel = Math.Min(descRegra, tetoPct);

            return
        $@"Promo {(chkPromo.Checked && chkEnable.Checked ? "ATIVA" : "INATIVA")}
• Mínimo para resgatar: {nudMinPtsRedeem.Value} ponto(s).
• Ao resgatar, consome {nudPtsPerRedeem.Value} ponto(s) e concede {nudKzPerRedeem.Value:N2} Kz.
• Teto de desconto: {nudMaxPct.Value:N2}% do subtotal.

Exemplo (subtotal {exSubtotal:N2} Kz):
• Teto por %: {tetoPct:N2} Kz
• Regra fixa: {descRegra:N2} Kz
→ Desconto aplicável: {descAplicavel:N2} Kz (menor entre os dois).";
        }

        private string BuildProtecaoSummary()
        {
            return
        $@"Proteção de Lucro
• Margem bruta mínima após descontos: {nudMinMargin.Value:N2}%.
• Guardião: recomenda PAUSAR se a margem ficar abaixo da mínima por {nudAutoPauseDays.Value} dia(s) nos últimos 7 dias (teste pelo botão).";
        }

        private string BuildMetasSummary()
        {
            return
        $@"Metas do Negócio (mês atual)
• Faturamento alvo: {nudMetaFaturamento.Value:N2} Kz
• Margem líquida alvo: {nudMetaMargemLiquida.Value:N2}%
• Margem mínima por venda: {nudMargemMinPorVenda.Value:N2}%";
        }

        private string BuildTesteSummary()
        {
            return
        @"Teste / Simulação
• Clique em “Simular carrinho…” e cole linhas no formato:
  pcode;preço;custo;qty
• O sistema calcula se o RESGATE passa no teto % e na margem mínima.";
        }

    }
}