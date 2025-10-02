using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace lojaCanuma
{
    public partial class frmInventoryReport : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        //SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public frmInventoryReport()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void frmInventoryReportcs_Load(object sender, EventArgs e)
        {

            this.reportViewer1.RefreshReport();
        }

        public void LoadReport()
        {
            try
            {
                // 🔹 Carrega dados da loja
                var loja = CarregarDadosDaLoja();
                if (loja.Logo == null || loja.Logo.Length == 0)
                    throw new Exception("Logotipo da loja não encontrado.");

                // 🔹 Salva logotipo temporário
                string tempLogoPath = Path.Combine(Path.GetTempPath(), "logo_temp_inventory.png");
                File.WriteAllBytes(tempLogoPath, loja.Logo);

                // 🔹 Caminho do relatório
                reportViewer1.LocalReport.ReportPath = Path.Combine(Application.StartupPath, @"Reports\ReportInventory.rdlc");
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.EnableExternalImages = true;

                // 🔹 Consulta ao banco e carregamento de dados
                using (SqlConnection connection = new SqlConnection(new DBConnection().MyConnection()))
                using (SqlDataAdapter adapter = new SqlDataAdapter(@"
            SELECT 
                p.pcode, 
                p.barcode, 
                p.pdesc, 
                b.brand, 
                c.category, 
                p.price, 
                p.reorder, 
                p.qty
            FROM tblProduct AS p
            INNER JOIN tblBrand AS b ON p.bid = b.id
            INNER JOIN tblCategory AS c ON p.cid = c.id
        ", connection))
                {
                    DataSet1 ds = new DataSet1();
                    adapter.Fill(ds.Tables["dtInventory"]);

                    // 🔹 Fonte de dados do relatório
                    ReportDataSource rptDS = new ReportDataSource("DataSet1", ds.Tables["dtInventory"]);
                    reportViewer1.LocalReport.DataSources.Add(rptDS);

                    // 🔹 Parâmetros do relatório
                    var parametros = new ReportParameter[]
                    {
                new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                new ReportParameter("pLogo", new Uri(tempLogoPath).AbsoluteUri),
                new ReportParameter("pDate", DateTime.Now.ToString("dd/MM/yyyy"))
                    };
                    reportViewer1.LocalReport.SetParameters(parametros);
                }

                // 🔹 Configuração visual
                reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.Percent;
                reportViewer1.ZoomPercent = 100;
                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar o relatório: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private class StoreData
        {
            public string Nome { get; set; }
            public byte[] Logo { get; set; }
        }

        private StoreData CarregarDadosDaLoja()
        {
            var loja = new StoreData();

            try
            {
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT TOP 1 store, logo FROM tblStore", cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        loja.Nome = dr["store"]?.ToString() ?? "Canuma Comercial, LDA";
                        loja.Logo = dr["logo"] as byte[] ?? Array.Empty<byte>();
                    }
                    else
                    {
                        throw new Exception("Nenhuma loja encontrada na base de dados.");
                    }

                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao carregar dados da loja: {ex.Message}");
            }

            return loja;
        }
        public void LoadTopSelling(DateTime from, DateTime to, string ordenacao)
        {
            try
            {
                // 🔹 1. Carrega dados da loja
                var loja = CarregarDadosDaLoja();
                if (loja.Logo == null || loja.Logo.Length == 0)
                    throw new Exception("Logotipo da loja não encontrado.");

                // 🔹 2. Salva o logotipo em arquivo temporário
                string tempLogoPath = Path.Combine(Path.GetTempPath(), "logo_temp_top.png");
                File.WriteAllBytes(tempLogoPath, loja.Logo);
                if (!File.Exists(tempLogoPath))
                    throw new Exception("Erro ao salvar o logotipo temporário.");

                // 🔹 3. Define a ordenação dinâmica
                string orderBy = "qty DESC";
                string ordenacaoTexto = "Quantidade";

                if (ordenacao == "ORD. POR VALOR")
                {
                    orderBy = "total DESC";
                    ordenacaoTexto = "Valor";
                }

                // 🔹 4. SQL com ordenação dinâmica
                string sql = $@"
            SELECT TOP 10 
                p.pcode, 
                p.pdesc, 
                SUM(si.qty) AS qty, 
                SUM(si.total) AS total
            FROM vwSoldItems si
            INNER JOIN tblProduct p ON si.pcode = p.pcode
            WHERE si.sdate BETWEEN @from AND @to 
              AND si.status = 'Sold'
            GROUP BY p.pcode, p.pdesc
            ORDER BY {orderBy}";

                // 🔹 5. Executa a consulta
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(sql, cn);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "DataSet1");

                    // 🔹 6. Configura o ReportViewer
                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.ReportPath = @"Reports\ReportToTemSolItem.rdlc";
                    reportViewer1.LocalReport.EnableExternalImages = true;

                    ReportDataSource source = new ReportDataSource("DataSet1", ds.Tables["DataSet1"]);
                    reportViewer1.LocalReport.DataSources.Add(source);

                    // 🔹 7. Parâmetros do relatório
                    ReportParameter[] parametros = new ReportParameter[]
                    {
                new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                new ReportParameter("pDate", $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}"),
                new ReportParameter("pLogo", new Uri(tempLogoPath).AbsoluteUri),
                new ReportParameter("pOrdenacao", "Ordenado por: " + ordenacaoTexto)
                    };

                    reportViewer1.LocalReport.SetParameters(parametros);
                    reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                    reportViewer1.ZoomMode = ZoomMode.Percent;
                    reportViewer1.ZoomPercent = 100;
                    reportViewer1.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar relatório de mais vendidos:\n" + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        public void LoadSoldItemsReport(DateTime from, DateTime to)
        {
            try
            {
                // 🔹 Carrega dados da loja (nome e logotipo)
                var loja = CarregarDadosDaLoja();
                if (loja.Logo == null || loja.Logo.Length == 0)
                    throw new Exception("Logotipo da loja não encontrado.");

                string tempLogoPath = Path.Combine(Path.GetTempPath(), "logo_temp_sold.png");
                File.WriteAllBytes(tempLogoPath, loja.Logo);

                string sql = @"
                            SELECT 
                                c.pcode, 
                                p.pdesc, 
                                c.price, 
                                SUM(c.qty) AS total_qty, 
                                SUM(c.disc) AS total_dic, 
                                SUM(c.total) AS total
                            FROM tblcar AS c
                            INNER JOIN tblProduct AS p ON c.pcode = p.pcode
                            WHERE c.status = 'Sold' 
                              AND c.sdate BETWEEN @from AND @to
                            GROUP BY c.pcode, p.pdesc, c.price";

                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand(sql, cn);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    da.Fill(ds, "dtSoldItems2");

                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.ReportPath = @"Reports\ReportSoldItems2.rdlc"; // Altere o nome se necessário
                    reportViewer1.LocalReport.EnableExternalImages = true;

                    ReportDataSource source = new ReportDataSource("DataSet1", ds.Tables["dtSoldItems2"]);
                    reportViewer1.LocalReport.DataSources.Add(source);

                    var parametros = new ReportParameter[]
                    {
                new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                new ReportParameter("pDate", $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}"),
                new ReportParameter("pLogo", new Uri(tempLogoPath).AbsoluteUri)
                    };

                    reportViewer1.LocalReport.SetParameters(parametros);
                    reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                    reportViewer1.ZoomMode = ZoomMode.Percent;
                    reportViewer1.ZoomPercent = 100;
                    reportViewer1.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar o relatório de vendas:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        public void LoadCriticalItemsReport()
{
    try
    {
        // 🔹 1. Carrega dados da loja
        var loja = CarregarDadosDaLoja();
        if (loja.Logo == null || loja.Logo.Length == 0)
            throw new Exception("Logotipo da loja não encontrado.");

        string tempLogoPath = Path.Combine(Path.GetTempPath(), "logo_temp_critical.png");
        File.WriteAllBytes(tempLogoPath, loja.Logo);

        // 🔹 2. Consulta SQL
        string sql = "SELECT * FROM vwCriticalItems";

        using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand(sql, cn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds, "dtCriticalItems");

            // 🔹 3. Configuração do ReportViewer
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.ReportPath = @"Reports\ReportCriticalItems.rdlc";
            reportViewer1.LocalReport.EnableExternalImages = true;

            ReportDataSource source = new ReportDataSource("DataSet1", ds.Tables["dtCriticalItems"]);
            reportViewer1.LocalReport.DataSources.Add(source);

            var parametros = new ReportParameter[]
            {
                new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                new ReportParameter("pDate", DateTime.Now.ToString("dd/MM/yyyy")),
                new ReportParameter("pLogo", new Uri(tempLogoPath).AbsoluteUri)
            };

            reportViewer1.LocalReport.SetParameters(parametros);
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;
            reportViewer1.RefreshReport();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Erro ao gerar o relatório de produtos críticos:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}





        public void LoadCancelledOrdersReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 🔹 1. Dados da loja e logo
                var loja = CarregarDadosDaLoja();
                if (loja.Logo == null || loja.Logo.Length == 0)
                    throw new Exception("Logotipo da loja não encontrado.");

                string tempLogoPath = Path.Combine(Path.GetTempPath(), "logo_temp_cancel.png");
                File.WriteAllBytes(tempLogoPath, loja.Logo);

                // 🔹 2. Consulta
                string sql = @"
            SELECT * FROM vwCancelOrders 
            WHERE CONVERT(date, sdate) BETWEEN @startDate AND @endDate";

                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(sql, cn);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    da.Fill(ds, "dtCancelledOrders");

                    // 🔹 3. Configurar ReportViewer
                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.ReportPath = @"Reports\ReportCancelledOrders.rdlc";
                    reportViewer1.LocalReport.EnableExternalImages = true;

                    ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables["dtCancelledOrders"]);
                    reportViewer1.LocalReport.DataSources.Add(rds);

                    var parametros = new ReportParameter[]
                    {
                new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                new ReportParameter("pDate", $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}"),
                new ReportParameter("pLogo", new Uri(tempLogoPath).AbsoluteUri)
                    };

                    reportViewer1.LocalReport.SetParameters(parametros);
                    reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                    reportViewer1.ZoomMode = ZoomMode.Percent;
                    reportViewer1.ZoomPercent = 100;
                    reportViewer1.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar relatório de cancelamentos:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadStockInHistoryReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var loja = CarregarDadosDaLoja();
                if (loja.Logo == null || loja.Logo.Length == 0)
                    throw new Exception("Logotipo da loja não encontrado.");

                string tempLogo = Path.Combine(Path.GetTempPath(), "logo_temp_stockin.png");
                File.WriteAllBytes(tempLogo, loja.Logo);

                string sql = @"
            SELECT * FROM vwStockin 
            WHERE CAST(sdate AS DATE) BETWEEN @startDate AND @endDate 
            AND status = 'Done'";

                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand(sql, cn);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();
                    da.Fill(ds, "dtStockInHistory");

                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.ReportPath = @"Reports\ReportStockInHistory.rdlc";
                    reportViewer1.LocalReport.EnableExternalImages = true;

                    ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables["dtStockInHistory"]);
                    reportViewer1.LocalReport.DataSources.Add(rds);

                    var parametros = new ReportParameter[]
                    {
                new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                new ReportParameter("pDate", $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}"),
                new ReportParameter("pLogo", new Uri(tempLogo).AbsoluteUri)
                    };

                    reportViewer1.LocalReport.SetParameters(parametros);
                    reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                    reportViewer1.ZoomMode = ZoomMode.Percent;
                    reportViewer1.ZoomPercent = 100;
                    reportViewer1.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar relatório de entradas de estoque:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





    }
}
