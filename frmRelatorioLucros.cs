using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace lojaCanuma
{
    public partial class frmRelatorioLucros : Form
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public frmRelatorioLucros()
        {
            InitializeComponent();
        }

        private async void frmRelatorioLucros_Load(object sender, EventArgs e)
        {
            try
            {
                await CarregarRelatorioAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar relatório:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CarregarRelatorioAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                var loja = await Task.Run(() => CarregarDadosDaLoja());
                string caminhoLogo = await ProcessarLogoAsync(loja.Logo);
                var (ds, lucroBruto, despesas, lucroLiquido) = await CarregarDadosRelatorioAsync();

                ConfigurarRelatorio(ds, loja, caminhoLogo, lucroBruto, despesas, lucroLiquido);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao montar relatório:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task<string> ProcessarLogoAsync(byte[] logo)
        {
            if (logo == null || logo.Length == 0)
                return "";

            string caminho = Path.Combine(Path.GetTempPath(), "logo_temp_" + Guid.NewGuid() + ".png");

            using (FileStream fs = new FileStream(caminho, FileMode.Create, FileAccess.Write))
                await fs.WriteAsync(logo, 0, logo.Length);

            return "file:///" + caminho.Replace("\\", "/");
        }

        private async Task<(DataSet2, decimal, decimal, decimal)> CarregarDadosRelatorioAsync()
        {
            return await Task.Run(() =>
            {
                var ds = new DataSet2();
                decimal lucroBruto = 0;
                decimal despesas = 0;

                using (var cn = new SqlConnection(new DBConnection().MyConnection()))
                using (var cmd = new SqlCommand(@"
                    SELECT DataMovimento, TipoMovimento, Categoria, Valor, Saldo
                    FROM vwFluxoCaixaComSaldo
                    WHERE DataMovimento BETWEEN @inicio AND @fim
                    ORDER BY DataMovimento", cn))
                {
                    cmd.Parameters.AddWithValue("@inicio", DataInicio.Date);
                    cmd.Parameters.AddWithValue("@fim", DataFim.Date.AddDays(1).AddTicks(-1));

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(ds, "FluxoCaixaDetalhado");

                    foreach (DataRow row in ds.Tables["FluxoCaixaDetalhado"].Rows)
                    {
                        string tipo = row["TipoMovimento"]?.ToString() ?? "";
                        decimal valor = Convert.ToDecimal(row["Valor"] ?? 0);

                        if (tipo == "Venda") lucroBruto += valor;
                        else if (tipo.Contains("Despesa") || tipo.Contains("Salário") || tipo.Contains("Cancelamento"))
                            despesas += Math.Abs(valor);
                    }
                }

                return (ds, lucroBruto, despesas, lucroBruto - despesas);
            });
        }

        private void ConfigurarRelatorio(DataSet2 ds, StoreData loja, string caminhoLogo, decimal lucroBruto, decimal despesas, decimal lucroLiquido)
        {
            string caminhoRelatorio = Path.Combine(Application.StartupPath, "Reports", "rptFluxoCaixaNovo.rdlc");
            if (!File.Exists(caminhoRelatorio))
            {
                MessageBox.Show("Relatório não encontrado.");
                return;
            }

            reportViewer1.Reset();
            reportViewer1.LocalReport.ReleaseSandboxAppDomain();
            reportViewer1.LocalReport.ReportPath = caminhoRelatorio;
            reportViewer1.LocalReport.DataSources.Clear();

            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", ds.Tables["FluxoCaixaDetalhado"]));
            reportViewer1.LocalReport.EnableExternalImages = true;

            var parametros = new List<ReportParameter>
            {
                new ReportParameter("pStore", loja.Nome ?? ""),
                new ReportParameter("pPeriodo", "De " + DataInicio.ToString("dd/MM/yyyy") + " até " + DataFim.ToString("dd/MM/yyyy")),
                new ReportParameter("pAddress", loja.Endereco ?? ""),
                new ReportParameter("pTelefone", loja.Telefone ?? ""),
                new ReportParameter("pEmail", loja.Email ?? ""),
                new ReportParameter("pLogo", caminhoLogo ?? ""),
                new ReportParameter("pLucroBruto", "Lucro Bruto: " + lucroBruto.ToString("N2") + " Kz"),
                new ReportParameter("pDespesasTotais", "Despesas Totais: " + despesas.ToString("N2") + " Kz"),
                new ReportParameter("pLucroLiquido", "Lucro Líquido: " + lucroLiquido.ToString("N2") + " Kz")

            };

            try
            {
                reportViewer1.LocalReport.SetParameters(parametros);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao definir parâmetros:\n" + ex.Message);
            }

            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;
            reportViewer1.RefreshReport();
        }

        private StoreData CarregarDadosDaLoja()
        {
            var loja = new StoreData();

            try
            {
                using (var cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT TOP 1 store, address, telefone, email, logo FROM tblStore", cn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            loja.Nome = dr["store"]?.ToString();
                            loja.Endereco = dr["address"]?.ToString();
                            loja.Telefone = dr["telefone"]?.ToString();
                            loja.Email = dr["email"]?.ToString();
                            loja.Logo = dr["logo"] != DBNull.Value ? (byte[])dr["logo"] : Array.Empty<byte>();
                        }
                    }
                }
            }
            catch
            {
                loja.Nome = "Canuma Comercial";
                loja.Endereco = "-";
                loja.Telefone = "-";
                loja.Email = "-";
                loja.Logo = Array.Empty<byte>();
            }

            return loja;
        }

        private class StoreData
        {
            public string Nome { get; set; }
            public string Endereco { get; set; }
            public string Telefone { get; set; }
            public string Email { get; set; }
            public byte[] Logo { get; set; }
        }
    }
}