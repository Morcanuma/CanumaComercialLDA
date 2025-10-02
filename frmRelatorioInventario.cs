using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class frmRelatorioInventario : Form
    {
        private DateTime _dataRef;
        private bool _comDados;

        public frmRelatorioInventario(DateTime dataRef, bool comDados)
        {
            InitializeComponent();
            _dataRef = dataRef;
            _comDados = comDados;
        }
        private async void frmRelatorioInventario_Load(object sender, EventArgs e)
        {
            try
            {
                // 🧾 Carrega dados da loja
                var loja = await Task.Run(() => CarregarDadosDaLoja());
                string caminhoLogo = await ProcessarLogoAsync(loja.Logo);

                // 📄 Prepara o caminho do relatório
                string caminhoRelatorio = Path.Combine(Application.StartupPath, "Reports", "rptInventarioMensal.rdlc");

                if (!System.IO.File.Exists(caminhoRelatorio))
                {
                    MessageBox.Show("Arquivo do relatório não encontrado: " + caminhoRelatorio);
                    return;
                }

                // 🧱 Configura o ReportViewer
                reportViewer1.Reset();
                reportViewer1.LocalReport.ReleaseSandboxAppDomain();
                reportViewer1.LocalReport.ReportPath = caminhoRelatorio;
                reportViewer1.LocalReport.EnableExternalImages = true;

                // 📦 Query com ou sem dados
                string query = @"
            SELECT 
                p.pcode,
                p.pdesc,
                inv.estoqueInicial,
                inv.vendidos,
                inv.esperado,
                inv.estoqueAtual,";

                if (_comDados)
                {
                    query += @"
                inv.contado,
                inv.diferenca,
                inv.status";
                }
                else
                {
                    query += @"
                NULL AS contado,
                NULL AS diferenca,
                NULL AS status";
                }

                query += @"
            FROM tblInventarioMensal inv
            INNER JOIN tblProduct p ON p.pcode = inv.pcode
            WHERE inv.dataRef = @ref";

                // 🔄 Carrega os dados para o DataSet
                var ds = new DataSet();
                using (var cn = new SqlConnection(new DBConnection().MyConnection()))
                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@ref", _dataRef);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(ds, "InventarioMensal");

                    // ✅ Trata nulos manualmente (evita erro com Int32)
                    foreach (DataRow row in ds.Tables["InventarioMensal"].Rows)
                    {
                        if (row.IsNull("contado")) row["contado"] = DBNull.Value;
                        if (row.IsNull("diferenca")) row["diferenca"] = DBNull.Value;
                        if (row.IsNull("status")) row["status"] = DBNull.Value;
                    }
                }

                // 🗂️ Carrega os dados no relatório
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(
                new ReportDataSource("InvenMensal", ds.Tables["InventarioMensal"])
            );


                // 🎯 Define parâmetros visuais
                var parametros = new List<ReportParameter>
        {
            new ReportParameter("pStore", loja.Nome ?? ""),
            new ReportParameter("pAddress", loja.Endereco ?? ""),
            new ReportParameter("pTelefone", loja.Telefone ?? ""),
            new ReportParameter("pEmail", loja.Email ?? ""),
            new ReportParameter("pLogo", caminhoLogo ?? ""),
            new ReportParameter("pData", "Referência: " + _dataRef.ToString("MMMM yyyy")),
            new ReportParameter("pTipoImpressao", _comDados ? "Relatório Completo" : "Modelo em Branco")
        };

                reportViewer1.LocalReport.SetParameters(parametros);

                // 🖨️ Exibe o relatório formatado
                reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.Percent;
                reportViewer1.ZoomPercent = 100;
                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar relatório:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


        public class StoreData
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
                loja.Nome = "Loja Padrão";
                loja.Endereco = "-";
                loja.Telefone = "-";
                loja.Email = "-";
                loja.Logo = Array.Empty<byte>();
            }

            return loja;
        }


    }
}
