using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace lojaCanuma
{
    public partial class frmReceipt : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        frmPOS f;
        StoreData lojaData;

        private readonly lojaCanuma.Services.LoyaltyService _loySvc2 =
        new lojaCanuma.Services.LoyaltyService(new DBConnection().MyConnection());
        private lojaCanuma.Models.LoyaltySettings _loySet2;

        public frmReceipt(frmPOS frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
        }

        private void frmReceipt_Load(object sender, EventArgs e)
        {

            reportViewer1.RefreshReport();
        }

       

        private void reportViewer1_Load_1(object sender, EventArgs e)
        {
            
        }

        private (string subject, string html) BuildReceiptEmail(
             string transno,
             string customerName,
             decimal totalKzs,
             DateTime dataHora,
             string lojaNome,
             string lojaEndereco,
             string lojaTelefone,
             string lojaEmail,
             int pontosAntes,
             int pontosUsados,
             int pontosGanhos,
             int pontosRestantes,
             string itemsHtml,
             string loyaltyHtml)
                    {
                        string assunto = $"Recibo #{transno} — {totalKzs:N2} Kzs";

                        string html = $@"
            <!doctype html>
            <html>
            <head>
              <meta charset='utf-8'>
              <style>
                body {{font-family:Segoe UI, Arial, sans-serif; background:#f6f8fb; margin:0; padding:24px; color:#222}}
                .card {{max-width:640px; margin:0 auto; background:#fff; border-radius:12px; padding:24px; box-shadow:0 2px 12px rgba(0,0,0,.06)}}
                .head {{display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:12px}}
                .title {{font-size:18px; font-weight:700}}
                .muted {{color:#6b7280; font-size:12px}}
                .pill {{background:#eef2ff; color:#4338ca; padding:4px 10px; border-radius:999px; font-size:12px; font-weight:600}}
                .row {{display:flex; gap:12px; margin:12px 0}}
                .box {{flex:1; background:#f9fafb; border-radius:10px; padding:12px}}
                .label {{font-size:12px; color:#6b7280}}
                .val {{font-weight:700; margin-top:2px}}
                .total {{font-size:20px; font-weight:800}}
                .hr {{height:1px; background:#eee; margin:16px 0}}
                .foot {{font-size:12px; color:#6b7280; line-height:1.5; margin-top:12px}}
                table {{width:100%; border-collapse:collapse; margin-top:8px}}
                th,td {{padding:8px; border-bottom:1px solid #eee; font-size:13px; text-align:left}}
                th {{color:#6b7280; font-weight:600}}
                td.r {{text-align:right}}
              </style>
            </head>
            <body>
              <div class='card'>
                <div class='head'>
                  <div>
                    <div class='title'>{lojaNome}</div>
                    <div class='muted'>{lojaEndereco}</div>
                    <div class='muted'>Tel: {lojaTelefone} · {lojaEmail}</div>
                  </div>
                  <div class='pill'>Recibo #{transno}</div>
                </div>

                <p>Olá, <strong>{System.Net.WebUtility.HtmlEncode(customerName)}</strong>! Obrigado pela sua compra.</p>

                <div class='row'>
                  <div class='box'>
                    <div class='label'>Data</div>
                    <div class='val'>{dataHora:dd/MM/yyyy HH:mm}</div>
                  </div>
                  <div class='box'>
                    <div class='label'>Cliente</div>
                    <div class='val'>{System.Net.WebUtility.HtmlEncode(customerName)}</div>
                  </div>
                  <div class='box'>
                    <div class='label'>Total</div>
                    <div class='val total'>{totalKzs:N2} Kzs</div>
                  </div>
                </div>

                <div class='hr'></div>

                <div>
                  <div class='label'>Itens da compra</div>
                  {itemsHtml}
                </div>

                <div class='hr'></div>

                <div class='row'>
                  <div class='box'><div class='label'>Pontos antes</div><div class='val'>{pontosAntes}</div></div>
                  <div class='box'><div class='label'>Usados</div><div class='val'>{pontosUsados}</div></div>
                  <div class='box'><div class='label'>Ganhos</div><div class='val'>+{pontosGanhos}</div></div>
                  <div class='box'><div class='label'>Saldo final</div><div class='val'>{pontosRestantes}</div></div>
                </div>

                <div class='hr'></div>
                <div class='label'>Fidelidade Canuma — Como funciona</div>
                {loyaltyHtml}

                <p class='muted'>O recibo completo segue em anexo (PDF).</p>

                <div class='hr'></div>
                <div class='foot'>
                  Precisa de ajuda com esta compra? Responda este e-mail ou contacte-nos por telefone.
                  <br/>© {DateTime.Now:yyyy} {lojaNome}. Todos os direitos reservados.
                </div>
              </div>
            </body>
            </html>";
            return (assunto, html);
        }


        private string BuildItemsTable(DataTable sales)
        {
            if (sales == null || sales.Rows.Count == 0)
                return "<div class='muted'>Sem itens.</div>";

            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Produto</th><th class='r'>Qtd</th><th class='r'>Preço</th><th class='r'>Total</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (DataRow r in sales.Rows)
            {
                string desc = r.Table.Columns.Contains("pdesc") ? Convert.ToString(r["pdesc"]) : "";
                decimal qty = 0, price = 0, total = 0;

                // As colunas vêm da sua query: price, qty, total
                decimal.TryParse(Convert.ToString(r["qty"], CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out qty);
                decimal.TryParse(Convert.ToString(r["price"], CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out price);
                decimal.TryParse(Convert.ToString(r["total"], CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out total);

                sb.AppendLine(
                    $"<tr><td>{System.Net.WebUtility.HtmlEncode(desc)}</td>" +
                    $"<td class='r'>{qty:N0}</td>" +
                    $"<td class='r'>{price:N2} Kzs</td>" +
                    $"<td class='r'>{total:N2} Kzs</td></tr>");
            }

            sb.AppendLine("</tbody></table>");
            return sb.ToString();
        }




        public void LoadReport(string cashReceived, string changeAmount)
        {
            _loySet2 = _loySvc2.GetSettings();

            // Evita ArgumentNullException e garante valor numérico
            if (string.IsNullOrWhiteSpace(cashReceived)) cashReceived = "0";
            if (string.IsNullOrWhiteSpace(changeAmount)) changeAmount = "0";

            try
            {
                // 0) Dados fixos da loja e preparar o viewer
                lojaData = CarregarDadosDaLoja();
                InitializeReportViewer(); // Reset + ReportPath + Clear

                // 1) Carrega dados da venda
                var reportData = LoadReportData(cashReceived);

                // 2) Primeiro o DataSource (o nome deve bater com o RDLC)
                reportViewer1.LocalReport.DataSources.Add(
                    new ReportDataSource("DataSet1", reportData.SalesData));

                // 3) Depois TODOS os parâmetros do RDLC (inclui pBaseTributavel)
                SetReportParameters(reportData, changeAmount);

                // 4) Só agora renderiza na tela (único RefreshReport)
                ConfigureReportViewer();

                // 5) PDF + e-mail (opcional)
                var pdfPath = SalvarReciboPDF(reportData.TransactionNumber);
                var itemsHtml = BuildItemsTable(reportData.SalesData);
                var loyaltyHtml = BuildLoyaltySummaryHtml(reportData);

                var (assunto, corpoHtml) = BuildReceiptEmail(
                    reportData.TransactionNumber,
                    reportData.CustomerName,
                    reportData.Total,
                    DateTime.Now,
                    lojaData.Nome, lojaData.Endereco, lojaData.Telefone, lojaData.Email,
                    reportData.PontosAntesDaCompra,
                    reportData.PontosUsados,
                    reportData.PontosGanhos,
                    reportData.PontosRestantes,
                    itemsHtml,
                    loyaltyHtml
                );

                if (!string.IsNullOrWhiteSpace(reportData.CustomerEmail) &&
                    !string.IsNullOrWhiteSpace(pdfPath))
                {
                    Task.Run(() =>
                        lojaCanuma.Services.EmailService.EnviarComAnexo(
                            reportData.CustomerEmail, assunto, corpoHtml, pdfPath));
                }
            }
            catch (SqlException ex) { LogError(ex, "Database error while loading report"); ShowError("Erro de banco de dados", ex.Message); }
            catch (IOException ex) { LogError(ex, "Report file not found"); ShowError("Arquivo não encontrado", "O relatório não pôde ser localizado"); }
            catch (FormatException ex) { LogError(ex, "Invalid monetary format"); ShowError("Formato inválido", "Valores monetários em formato incorreto"); }
            catch (Exception ex) { LogError(ex, "Unexpected error generating report"); ShowError("Erro inesperado", "Ocorreu um erro ao gerar o relatório"); }
            finally
            {
                EnsureConnectionClosed();
            }
        }


        // Métodos auxiliares privados
        private void InitializeReportViewer()
        {
            string reportPath = Path.Combine(Application.StartupPath, "Reports", "ReportReceipt.rdlc");

            if (!File.Exists(reportPath))
                throw new FileNotFoundException("O arquivo de relatório não foi encontrado", reportPath);

            reportViewer1.Reset(); // <- importante para limpar parâmetros/datasources antigos
            reportViewer1.LocalReport.ReportPath = reportPath;
            reportViewer1.LocalReport.DataSources.Clear();
        }




        private ReportData LoadReportData(string cashReceived)
        {
            var data = new ReportData();


            using (var connection = new SqlConnection(dbcon.MyConnection()))
            using (var command = new SqlCommand(GetSalesQuery(), connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@transno", f.lblTransno.Text.Trim());

                using (var reader = command.ExecuteReader())
                {
                    var salesTable = new DataTable();
                    salesTable.Load(reader);
                    data.SalesData = salesTable;
                    data.PontosUsados = f.pontosUsadosNaCompra;

                }
            }

            data.FormattedCash = FormatCurrencyValue(cashReceived);
            data.TaxBase = GetLabelDecimalValue(f.lblVatable);
            data.Vat = GetLabelDecimalValue(f.lblVat);
            data.Discount = GetLabelDecimalValue(f.lblDiscount);
            data.Total = GetLabelDecimalValue(f.lblTotal);
            data.CashierName = f.lblName?.Text?.Trim() ?? "Não informado";
            data.TransactionNumber = f.lblTransno?.Text?.Trim() ?? "000000";




            // ─── Início da seção Cliente ───────────────────────────────────
            int custId = 0;
            try
            {
                // se frmPOS tem o método acima:
                custId = f.GetSelectedCustomerIdSafe();
            }
            catch { custId = 0; }

            if (custId > 0)
            {
                var repo = new CustomerRepository();
                var cust = repo.GetById(custId); // usa CustomerId

                data.CustomerId = cust?.CustomerId ?? 0;
                data.CustomerName = cust?.Name ?? "Não informado";
                data.CustomerPoints = cust?.Points ?? 0;
                data.CustomerEmail = cust?.Email?.Trim();

                int pontosAtuais = cust?.Points ?? 0;
                int pontosUsados = f.pontosUsadosNaCompra;
                int pontosGanhos = (int)Math.Floor(
                data.Total / (_loySet2?.KzPerPointEarn ?? 1000m));
                data.PontosGanhos = pontosGanhos;
                data.PontosUsados = pontosUsados;
                data.PontosAntesDaCompra = pontosAtuais - pontosGanhos + pontosUsados;
                data.PontosRestantes = pontosAtuais;

                // --- Cálculo do desconto por pontos realmente aplicado + pontos debitados ---
                var s = _loySet2 ?? new lojaCanuma.Models.LoyaltySettings();

                // Valor máximo pela regra (1 resgate por compra)
                decimal regraMaxKz = (s.IsPromoEnabled && f.pontosUsadosNaCompra >= s.RedeemUnitPoints)
                    ? RoundKzTo5(s.RedeemDiscountKz)
                    : 0m;

                // Desconto total da venda em módulo (caso venha negativo do POS)
                decimal descontoTotalAbs = Math.Abs(data.Discount);

                // O que considerar como "desconto por pontos aplicado" nesta venda
                decimal appliedRedeemKz = Math.Min(regraMaxKz, descontoTotalAbs);
                data.RedeemDiscountAppliedKz = appliedRedeemKz;

                // Política para resgate parcial: CEIL (arredonda para cima)
                // Ex.: regra 5 pts → 500 Kz; aplicado 50 Kz → ratio 0,1 → debita 1 ponto
                int pontosDebitados = 0;
                if (appliedRedeemKz > 0m && s.RedeemUnitPoints > 0 && s.RedeemDiscountKz > 0m)
                {
                    decimal unitKz = RoundKzTo5(s.RedeemDiscountKz);         // p.ex. 500
                    decimal ratio = appliedRedeemKz / unitKz;                // p.ex. 50/500 = 0,1
                    pontosDebitados = (int)Math.Ceiling(ratio * s.RedeemUnitPoints);
                    pontosDebitados = Math.Min(pontosDebitados, f.pontosUsadosNaCompra); // nunca mais que o pedido
                }

                // Passe a usar SEMPRE os efetivamente debitados
                data.PontosUsados = pontosDebitados;
                data.PontosAntesDaCompra = (cust?.Points ?? 0) - data.PontosGanhos + data.PontosUsados;
                data.PontosRestantes = (cust?.Points ?? 0);


            }
            else
            {
                data.CustomerId = 0;
                data.CustomerName = "Cliente optou por não informar o nome";
                data.CustomerPoints = 0;
                data.CustomerEmail = null;
            }
            // ─── Fim da seção Cliente ──────────────────────────────────────


            return data;
        }

        private void SetReportParameters(ReportData data, string changeAmount)
        {
            string mensagem = MessageProvider.GetMessageForToday();

            var parameters = new List<ReportParameter>
    {
        new ReportParameter("pBaseTributavel", data.TaxBase.ToString("N2") + " Kzs"),
        new ReportParameter("pIva", data.Vat.ToString("N2") + " Kzs"),
        new ReportParameter("pDiscount", data.Discount.ToString("N2") + " Kzs"),
        new ReportParameter("pTotal", data.Total.ToString("N2") + " Kzs"),
        new ReportParameter("pCash", data.FormattedCash),
        new ReportParameter("pChange", changeAmount.Replace("Kzs", "").Trim() + " Kzs"),
        new ReportParameter("pStore", lojaData.Nome),
        new ReportParameter("pAddress", lojaData.Endereco),
        new ReportParameter("pTransacao", "Fatura #" + data.TransactionNumber),
        new ReportParameter("pFuncionario", data.CashierName),
        new ReportParameter("pTelefone", lojaData.Telefone),
        new ReportParameter("pEmail", lojaData.Email),
        new ReportParameter("pSubtotal", f.lblSub.Text.Replace("Kzs", "").Trim() + " Kzs"),
        new ReportParameter("pMensagem", mensagem)
    };

            if (lojaData.Logo != null)
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), "logo_temp.png");
                File.WriteAllBytes(tempFilePath, lojaData.Logo);
                reportViewer1.LocalReport.EnableExternalImages = true;
                parameters.Add(new ReportParameter("pLogo", new Uri(tempFilePath).AbsoluteUri));
            }
            else
            {
                parameters.Add(new ReportParameter("pLogo", ""));
            }

            // Cliente (separado e junto)
            parameters.Add(new ReportParameter("pCustomerName", data.CustomerName));
            parameters.Add(new ReportParameter("pCustomerId", data.CustomerId.ToString()));
            parameters.Add(new ReportParameter("pCustomerNameId", $"{data.CustomerName} ({data.CustomerId})"));

            // Pontos
            parameters.Add(new ReportParameter("pPontosUsados", data.PontosUsados.ToString()));
            parameters.Add(new ReportParameter("pPontosAntes", data.PontosAntesDaCompra.ToString()));
            parameters.Add(new ReportParameter("pPontosRestantes", data.PontosRestantes.ToString()));
            parameters.Add(new ReportParameter("pPontosGanhos", data.PontosGanhos.ToString()));


            // era: (_loySet2?.RedeemDiscountKz ?? 1000m)
            parameters.Add(new ReportParameter("pDescontoPorPontos",
            data.RedeemDiscountAppliedKz.ToString("N2") + " Kzs"));



            // desconto por pontos desta compra (1 resgate por compra)
            decimal descontoPorPontosKz =
                (data.PontosUsados >= (_loySet2?.RedeemUnitPoints ?? 0))
                    ? RoundKzTo5(_loySet2?.RedeemDiscountKz ?? 0m)
                    : 0m;

            // texto dinâmico completo
            string explicacao = BuildLoyaltyExplanation(data);


            // Texto explicativo dinâmico
            parameters.Add(new ReportParameter("pLoyaltyExplain", explicacao));


            reportViewer1.LocalReport.SetParameters(parameters);
            
        }


        private void ConfigureReportViewer()
        {
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;
            reportViewer1.RefreshReport(); // único lugar com RefreshReport()
        }

        private decimal GetLabelDecimalValue(Label label)
        {
            string rawValue = label?.Text?.Replace("Kzs", "").Trim() ?? "0";
            return decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : 0;
        }

        private string FormatCurrencyValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "0,00 Kzs";

            string numericValue = value.Replace("Kzs", "").Trim();
            if (decimal.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
            {
                return amount.ToString("N2") + " Kzs";
            }
            return "0,00 Kzs";
        }

        private void EnsureConnectionClosed()
        {
            if (cn?.State == ConnectionState.Open)
                cn.Close();
        }

        private void LogError(Exception ex, string message)
        {
            // Implementação real usaria um framework de logging como NLog ou Serilog
            System.Diagnostics.Debug.WriteLine($"[ERROR] {message}: {ex.Message}");
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string GetSalesQuery()
        {
            return @"SELECT c.id, c.transno, c.pcode, c.price, c.qty, c.disc, c.total, c.sdate, c.status, p.pdesc 
             FROM tblcar AS c 
             INNER JOIN tblProduct AS p ON p.pcode = c.pcode 
             WHERE c.transno = @transno";
        }

        // Classe DTO para organizar os dados do relatório
        private class ReportData
        {
            public DataTable SalesData { get; set; }
            public decimal TaxBase { get; set; }
            public decimal Vat { get; set; }
            public decimal Discount { get; set; }
            public decimal Total { get; set; }
            public string FormattedCash { get; set; }
            public string CashierName { get; set; }
            public string TransactionNumber { get; set; }

            // ← Novos campos de cliente:
            public string CustomerName { get; set; }
            public int CustomerPoints { get; set; }

            // ADICIONE:
            public int CustomerId { get; set; }      // 👈 ADICIONAR
            public int PontosUsados { get; set; }  // 👈 obrigatório
            public int PontosRestantes { get; set; }  // 👈 ADICIONE AQUI
            public int PontosAntesDaCompra { get; set; }  // 👈 novo campo
            public int PontosGanhos { get; set; }  // ✅ novo campo

            // 👇 novo:
            public string CustomerEmail { get; set; }

            public decimal RedeemDiscountAppliedKz { get; set; } // desconto por pontos efetivamente aplicado
            public int PointsConverted { get; set; }

        }


        // DTO: Representa os dados principais da loja usados no recibo
        private class StoreData
        {
            public string Nome { get; set; }
            public string Endereco { get; set; }
            public string Telefone { get; set; }
            public string Email { get; set; }
           
            public byte[] Logo { get; set; }
        }

        private StoreData CarregarDadosDaLoja()
        {
            var loja = new StoreData();

            using (SqlConnection cn = new SqlConnection(dbcon.MyConnection()))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM tblStore", cn);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    loja.Nome = dr["store"].ToString();
                    loja.Endereco = dr["address"].ToString();
                    loja.Telefone = dr["telefone"].ToString();
                    loja.Email = dr["email"].ToString();
                    loja.Logo = dr["logo"] != DBNull.Value ? (byte[])dr["logo"] : null;
                }

                if (loja.Logo != null)
                    Debug.WriteLine($"Imagem carregada: {loja.Logo.Length} bytes");
                else
                    Debug.WriteLine("Nenhuma imagem foi carregada do banco");

                dr.Close();
            }

            return loja;
        }

        private string SalvarReciboPDF(string transacaoId)
        {
            try
            {
                string pastaRecibos = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Recibos_Canuma");

                if (!Directory.Exists(pastaRecibos))
                    Directory.CreateDirectory(pastaRecibos);

                string nomeArquivo = $"Recibo_{transacaoId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string caminhoCompleto = Path.Combine(pastaRecibos, nomeArquivo);

                Warning[] warnings;
                string[] streamids;
                string mimeType, encoding, filenameExtension;

                byte[] bytes = reportViewer1.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding,
                    out filenameExtension, out streamids, out warnings
                );

                File.WriteAllBytes(caminhoCompleto, bytes);
                return caminhoCompleto;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar o recibo em PDF:\n" + ex.Message,
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

       
      

        private static decimal RoundKzTo5(decimal v)
        {
            return Math.Round(v / 5m, MidpointRounding.AwayFromZero) * 5m;
        }



        private string BuildLoyaltyExplanation(ReportData data)
        {
            var s = _loySet2 ?? new lojaCanuma.Models.LoyaltySettings();
            var sb = new StringBuilder();

            sb.AppendLine("Fidelidade Canuma — Como funciona:");
            sb.AppendLine($"• A cada {s.KzPerPointEarn:N0} Kz em compras, você acumula 1 ponto.");
            if (s.IsPromoEnabled)
                sb.AppendLine($"• Resgates: a cada {s.RedeemUnitPoints} ponto(s) você ganha {RoundKzTo5(s.RedeemDiscountKz):N0} Kz de desconto (1× por compra).");
            else
                sb.AppendLine("• Resgates: temporariamente desativados.");

            // Use sempre o valor realmente aplicado e os pontos debitados
            decimal aplicado = data.RedeemDiscountAppliedKz;
            int usados = data.PontosUsados;
            decimal pct = data.Total > 0 ? (aplicado / data.Total) * 100m : 0m;

            if (usados > 0 && aplicado > 0)
            {
                sb.AppendLine($"• Nesta compra: usou {usados} ponto(s) → desconto de {aplicado:N0} Kz (≈ {pct:N1}%).");

                // Se aplicou menos do que o máximo da regra, explique o limite
                decimal regraMaxKz = s.IsPromoEnabled ? RoundKzTo5(s.RedeemDiscountKz) : 0m;
                if (aplicado < regraMaxKz)
                    sb.AppendLine($"  (limitado nesta venda; máximo pela regra: {regraMaxKz:N0} Kz por {s.RedeemUnitPoints} ponto(s)).");
            }
            else
            {
                sb.AppendLine("• Nesta compra: nenhum resgate de pontos foi utilizado.");
            }

            sb.AppendLine($"• Pontos ganhos: {data.PontosGanhos} ponto(s).");
            sb.AppendLine($"• Saldo final: {data.PontosRestantes} ponto(s).");

            if (s.PointsExpireDays > 0)
                sb.AppendLine($"• Validade: cada ponto vale por {s.PointsExpireDays} dia(s).");
            if (s.InactivityMaxDays > 0)
                sb.AppendLine($"• Inatividade: após {s.InactivityMaxDays} dia(s) sem compras, seus pontos podem ficar indisponíveis/expirar.");

            return sb.ToString();
        }






        private string BuildLoyaltySummaryHtml(ReportData data)
        {
            var s = _loySet2 ?? new lojaCanuma.Models.LoyaltySettings();
            string li(string x) => $"<li>{System.Net.WebUtility.HtmlEncode(x)}</li>";

            var ul = new StringBuilder("<ul style='margin:6px 0 0 18px; padding:0;'>");
            ul.Append(li($"A cada {s.KzPerPointEarn:N0} Kz em compras, você acumula 1 ponto."));
            ul.Append(li(s.IsPromoEnabled
                ? $"Resgates: a cada {s.RedeemUnitPoints} ponto(s) você ganha {RoundKzTo5(s.RedeemDiscountKz):N0} Kz de desconto (1× por compra)."
                : "Resgates: temporariamente desativados."));

            if (data.PontosUsados > 0 && data.RedeemDiscountAppliedKz > 0)
                ul.Append(li($"Nesta compra: usou {data.PontosUsados} ponto(s) → desconto aplicado: {data.RedeemDiscountAppliedKz:N0} Kz."));
            else
                ul.Append(li("Nesta compra: nenhum resgate de pontos foi utilizado."));

            ul.Append(li($"Pontos ganhos: {data.PontosGanhos}. Saldo final: {data.PontosRestantes}."));

            if (s.PointsExpireDays > 0) ul.Append(li($"Validade: {s.PointsExpireDays} dia(s) por ponto."));
            if (s.InactivityMaxDays > 0) ul.Append(li($"Inatividade: após {s.InactivityMaxDays} dia(s) sem compras, pontos podem expirar."));

            // Observação de limite (quando aplicou menos que o máximo permitido)
            if (s.IsPromoEnabled && data.RedeemDiscountAppliedKz < RoundKzTo5(s.RedeemDiscountKz))
                ul.Append(li($"Limite desta compra: máx. {RoundKzTo5(s.RedeemDiscountKz):N0} Kz por {s.RedeemUnitPoints} ponto(s)."));

            ul.Append("</ul>");
            return ul.ToString();
        }






    }
}
