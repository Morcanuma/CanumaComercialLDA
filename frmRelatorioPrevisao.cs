using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmRelatorioPrevisao : Form
    {
        private List<(DateTime dia, double total)> historico;
        private List<(DateTime dia, double previsao)> previsao;

        public frmRelatorioPrevisao(
            List<(DateTime dia, double total)> hist,
            List<(DateTime dia, double previsao)> prev)
        {
            InitializeComponent();
            historico = hist;
            previsao = prev;
        }

        private void frmRelatorioPrevisao_Load(object sender, EventArgs e)
        {
            // 🔄 Limpa
            this.reportViewer1.RefreshReport();

            // 🧾 Preenche os DataTables
            var ds = new dsPrevisaoVendas();
            var dtHist = ds.dtHistorico;
            var dtPrev = ds.dtPrevisao;

            foreach (var (d, v) in historico)
                dtHist.Rows.Add(d.ToString("dd/MM"), v);

            foreach (var (d, v) in previsao)
                dtPrev.Rows.Add(d.ToString("dd/MM"), v);

            // 📁 Caminho do relatório
            // Caminho do relatório
            string reportPath = Path.Combine(Application.StartupPath, "Reports", "rptPrevisaoCompleta.rdlc");
            reportViewer1.LocalReport.ReportPath = reportPath;

            // Modo de exibição como folha A4
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout); // ✅ ESSENCIAL
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;


            // 📊 Dados
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dtHistorico", dtHist.AsEnumerable()));
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dtPrevisao", dtPrev.AsEnumerable()));

            // 🏪 Dados da loja
            var loja = CarregarDadosDaLoja();

            string logoTemp = "";
            if (loja.Logo != null)
            {
                logoTemp = Path.Combine(Path.GetTempPath(), "logo_temp.png");
                File.WriteAllBytes(logoTemp, loja.Logo);
                reportViewer1.LocalReport.EnableExternalImages = true;
            }

            // 🎯 Parâmetros
            var parametros = new List<ReportParameter>
            {
                new ReportParameter("pStore", loja.Nome),
                new ReportParameter("pAddress", loja.Endereco),
                new ReportParameter("pTelefone", loja.Telefone),
                new ReportParameter("pEmail", loja.Email),
                new ReportParameter("pLogo", logoTemp != "" ? new Uri(logoTemp).AbsoluteUri : "")
            };

            reportViewer1.LocalReport.SetParameters(parametros);
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;


            // 🔄 Atualiza visual
            reportViewer1.RefreshReport();
        }

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
            using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
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
                dr.Close();
            }
            return loja;
        }


    }
}
