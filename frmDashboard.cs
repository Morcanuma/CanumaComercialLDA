using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lojaCanuma
{
    public partial class frmDashboard : Form
    {
        private readonly string _conn = new DBConnection().MyConnection();

        public frmDashboard()
        {
            InitializeComponent();
            CarregarDadosDashboard();
            LoadChart();
            LoadChartMensal();
        }

        private void frmDashboard_VisibleChanged(object sender, EventArgs e)
        {
            // Verifica se o formulário está visível no momento
            if (this.Visible)
            {
                // Chama novamente o método de carregamento dos dados do dashboard
                // Isso garante que os dados apresentados estejam sempre atualizados
                CarregarDadosDashboard();
            }
        }

        // Cria uma conexão com o banco de dados usando a string definida na classe DBConnection
        SqlConnection cn = new SqlConnection(new DBConnection().MyConnection());

        // Evento que é executado quando o formulário frmDashboard é carregado
        private void frmDashboard_Load(object sender, EventArgs e)
        {
            CarregarDadosDashboard(); // Chama o método que busca e exibe os dados no painel
        }








        // Método responsável por buscar e exibir os dados nos painéis do dashboard
        private void CarregarDadosDashboard()
        {
            try
            {
                cn.Open();

                // 🔸 1. VENDAS DE HOJE (status 'Sold' e data de hoje)
                SqlCommand cmd1 = new SqlCommand(@"
            SELECT ISNULL(SUM(total), 0) 
            FROM tblCar 
            WHERE status = 'Sold' AND 
                  CONVERT(date, sdate) = CONVERT(date, GETDATE())", cn);
                lblVendasHoje.Text = Convert.ToDouble(cmd1.ExecuteScalar()).ToString("N2") + " KZs";

                // 🔸 2. LINHA DE PRODUTOS (quantidade de produtos únicos)
                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM tblProduct", cn);
                lblLinhaProdutos.Text = cmd2.ExecuteScalar().ToString();

                // 🔸 3. ESTOQUE ATUAL (soma do campo qty)
                SqlCommand cmd3 = new SqlCommand("SELECT ISNULL(SUM(qty), 0) FROM tblProduct", cn);
                lblEstoqueAtual.Text = cmd3.ExecuteScalar().ToString();

                // 🔸 4. ESTOQUE BAIXO (qty <= reorder)
                SqlCommand cmd4 = new SqlCommand("SELECT COUNT(*) FROM tblProduct WHERE qty <= reorder", cn);
                lblEstoqueBaixo.Text = cmd4.ExecuteScalar().ToString();


                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao carregar dados do dashboard:\n" + ex.Message);
            }
        }




        public void LoadChart()
        {
            using (var cnn = new SqlConnection(_conn))
            using (var da = new SqlDataAdapter(@"
        SELECT 
            YEAR(sdate) AS [year],
            CONVERT(decimal(18,2), SUM(ISNULL(total,0))) AS total
        FROM tblCar
        WHERE status = 'Sold' AND sdate IS NOT NULL
        GROUP BY YEAR(sdate)
        ORDER BY YEAR(sdate);
    ", cnn))
            {
                var ds = new DataSet();
                da.Fill(ds, "Sales");
                var dt = ds.Tables["Sales"];

                chart1.Series.Clear();
                chart1.Titles.Clear();
                chart1.Legends.Clear();

                if (chart1.ChartAreas.Count == 0) chart1.ChartAreas.Add("Main");
                if (chart1.Legends.Count == 0) chart1.Legends.Add("MainLegend");

                if (dt.Rows.Count == 0)
                {
                    chart1.Titles.Add("Comparativo Anual de Vendas");
                    chart1.Titles.Add("Sem dados para exibir");
                    chart1.Titles[0].Font = new Font("Segoe UI", 11, FontStyle.Bold);
                    chart1.Titles[1].Font = new Font("Segoe UI", 9);
                    return;
                }

                var area = chart1.ChartAreas[0];
                area.BackColor = Color.White;
                area.Area3DStyle.Enable3D = true;
                area.Area3DStyle.Inclination = 20;
                area.Area3DStyle.PointDepth = 30;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisY.MajorGrid.Enabled = false;

                var totalKzs = dt.AsEnumerable().Sum(r => r.Field<decimal>("total"));
                chart1.Titles.Add("Comparativo Anual de Vendas");
                chart1.Titles[0].Font = new Font("Segoe UI", 12, FontStyle.Bold);
                chart1.Titles.Add($"Total: {totalKzs:N0} Kzs");
                chart1.Titles[1].Font = new Font("Segoe UI", 9);
                chart1.Titles[1].ForeColor = Color.DimGray;

                var legend = chart1.Legends[0];
                legend.Name = "MainLegend";
                legend.Docking = Docking.Right;
                legend.Alignment = StringAlignment.Center;
                legend.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                legend.Title = "Ano";

                var s = chart1.Series.Add("SALES");
                s.ChartType = SeriesChartType.Doughnut;
                s.XValueMember = "year";
                s.YValueMembers = "total";
                s.IsValueShownAsLabel = true;
                s.Label = "#VALX\n#VAL{N0} Kzs\n#PERCENT{P1}";
                s.LegendText = "#VALX";
                s.ToolTip = "#VALX: #VAL{N0} Kzs (#PERCENT)";
                s["DoughnutRadius"] = "65";
                s["PieDrawingStyle"] = "Concave";
                s["PieLabelStyle"] = "Outside";
                s.SmartLabelStyle.Enabled = true;

                chart1.Palette = ChartColorPalette.Excel;
                chart1.DataSource = dt;
                s.IsXValueIndexed = true;
                chart1.DataBind();
                s.Sort(PointSortOrder.Descending, "Y");

                int anoAtual = DateTime.Now.Year;
                foreach (var p in s.Points)
                    if (p.AxisLabel == anoAtual.ToString()) { p["Exploded"] = "true"; break; }

                chart1.Annotations.Clear();
                chart1.Annotations.Add(new TextAnnotation
                {
                    Text = $"{totalKzs:N0} Kzs",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.Black,
                    AnchorAlignment = ContentAlignment.MiddleCenter,
                    X = 50,
                    Y = 50,
                    AnchorDataPoint = s.Points[0]
                });
            }
        }

        private void LoadChartAnual_BarrasZoom()
        {
            using (var cnn = new SqlConnection(new DBConnection().MyConnection()))
            using (var da = new SqlDataAdapter(@"
        SELECT YEAR(sdate) AS ano,
               CONVERT(decimal(18,2), SUM(ISNULL(total,0))) AS total
        FROM tblCar
        WHERE status='Sold' AND sdate IS NOT NULL
        GROUP BY YEAR(sdate)
        ORDER BY ano;", cnn))
            {
                var ds = new DataSet();
                da.Fill(ds, "Sales");
                var dt = ds.Tables["Sales"];

                chart1.Series.Clear();
                chart1.Titles.Clear();
                chart1.Legends.Clear();
                if (chart1.ChartAreas.Count == 0) chart1.ChartAreas.Add("Main");

                var area = chart1.ChartAreas[0];
                area.BackColor = Color.White;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisY.MajorGrid.Enabled = false;
                area.AxisX.Interval = 1;
                area.AxisX.Title = "Ano";
                area.AxisY.Title = "Total (Kzs)";
                area.AxisY.LabelStyle.Format = "#,##0";

                // habilitar zoom
                area.CursorX.IsUserEnabled = true;
                area.CursorX.IsUserSelectionEnabled = true;
                area.AxisX.ScaleView.Zoomable = true;
                area.CursorY.IsUserEnabled = true;
                area.CursorY.IsUserSelectionEnabled = true;
                area.AxisY.ScaleView.Zoomable = true;
                area.AxisX.ScrollBar.Enabled = false; // sem barra de rolagem

                decimal total = dt.AsEnumerable().Sum(r => r.Field<decimal>("total"));
                chart1.Titles.Add("Vendas por Ano (zoom)");
                chart1.Titles[0].Font = new Font("Segoe UI", 11, FontStyle.Bold);
                chart1.Titles.Add($"Total: {total:N0} Kzs");
                chart1.Titles[1].Font = new Font("Segoe UI", 9);
                chart1.Titles[1].ForeColor = Color.DimGray;

                var s = chart1.Series.Add("Vendas Anuais");
                s.ChartType = SeriesChartType.Column;
                s.XValueMember = "ano";
                s.YValueMembers = "total";
                s.IsValueShownAsLabel = true;
                s.Label = "#VALY{N0} Kzs";
                s.ToolTip = "Ano #VALX: #VALY{N0} Kzs";
                s["PointWidth"] = "0.6";
                s.Color = Color.FromArgb(0, 122, 204);
                s.Font = new Font("Segoe UI", 8, FontStyle.Bold);

                chart1.DataSource = dt;
                chart1.DataBind();
            }
        }

        // reseta zoom no duplo clique
        private void chart1_DoubleClick(object sender, EventArgs e)
        {
            var a = chart1.ChartAreas[0];
            a.AxisX.ScaleView.ZoomReset(0);
            a.AxisY.ScaleView.ZoomReset(0);
        }


        // reset de zoom no anual (barras) com duplo clique
       





        private void PrintDoc_PrintPageChart1(object sender, PrintPageEventArgs e)
        {
            // Define o tamanho desejado (ajuste conforme necessário)
            int desiredWidth = 700;
            int desiredHeight = 500;

            // Cria a imagem do gráfico com tamanho controlado
            Bitmap bmp = new Bitmap(desiredWidth, desiredHeight);
            chart1.DrawToBitmap(bmp, new Rectangle(0, 0, desiredWidth, desiredHeight));

            // Centraliza na página
            int x = (e.PageBounds.Width - desiredWidth) / 2;
            int y = 100; // distância do topo

            e.Graphics.DrawImage(bmp, x, y);
        }


        private void ImprimirChart1()
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDoc_PrintPageChart1;

            // ✅ Força a impressora "Microsoft Print to PDF" como padrão
            printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";

            // ✅ Define margens da página
            printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);

            // ✅ Mostra a caixa de diálogo de impressão (com opção de salvar como PDF)
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDoc;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print(); // Imprime ou salva em PDF, dependendo da impressora escolhida
            }
        }













        public void LoadChartMensal()
        {
            using (var cnn = new SqlConnection(_conn))
            using (var da = new SqlDataAdapter(@"
        SELECT 
            CAST(DATEFROMPARTS(YEAR(sdate), MONTH(sdate), 1) AS date) AS periodo,
            CONVERT(decimal(18,2), SUM(ISNULL(total,0))) AS total
        FROM tblCar
        WHERE status='Sold' AND sdate IS NOT NULL
        GROUP BY DATEFROMPARTS(YEAR(sdate), MONTH(sdate), 1)
        HAVING SUM(ISNULL(total,0)) > 0           -- só meses com venda
        ORDER BY periodo;", cnn))
            {
                var dt = new DataTable();
                da.Fill(dt);

                chart2.Series.Clear();
                chart2.Titles.Clear();
                chart2.Legends.Clear();
                if (chart2.ChartAreas.Count == 0) chart2.ChartAreas.Add("Main");

                var area = chart2.ChartAreas[0];
                area.BackColor = Color.White;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisY.MajorGrid.Enabled = false;
                area.AxisX.Title = "Mês/Ano";
                area.AxisY.Title = "Total (Kzs)";
                area.AxisY.LabelStyle.Format = "#,##0";

                // X categórico: só rótulos que existem (evita desalinhado)
                area.AxisX.MajorTickMark.Enabled = false;
                area.AxisX.MinorTickMark.Enabled = false;
                area.AxisX.IsMarginVisible = false; // começa exatamente no 1º ponto
                area.AxisX.LabelStyle.Angle = -45;

                // Zoom + Scroll (aparecem quando der zoom)
                area.CursorX.IsUserEnabled = true;
                area.CursorX.IsUserSelectionEnabled = true;
                area.AxisX.ScaleView.Zoomable = true;
                area.AxisX.ScrollBar.Enabled = true;
                area.AxisX.ScrollBar.IsPositionedInside = true;

                area.CursorY.IsUserEnabled = true;
                area.CursorY.IsUserSelectionEnabled = true;
                area.AxisY.ScaleView.Zoomable = true;
                area.AxisY.ScrollBar.Enabled = true;
                area.AxisY.ScrollBar.IsPositionedInside = true;

                chart2.AntiAliasing = AntiAliasingStyles.All;
                chart2.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

                // Série construída ponto a ponto com AxisLabel = "MMM/yy"
                var s = chart2.Series.Add("Vendas Mensais");
                s.ChartType = SeriesChartType.Column;
                s.Color = Color.FromArgb(0, 122, 204);
                s.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                s["PointWidth"] = "0.6";
                s.IsXValueIndexed = true; // eixo categórico
                s.ToolTip = "#AXISLABEL: #VALY{N0} Kzs";

                foreach (DataRow r in dt.Rows)
                {
                    var mes = ((DateTime)r["periodo"]).ToString("MMM/yy");
                    var val = Convert.ToDouble((decimal)r["total"]);
                    var p = new DataPoint();
                    p.AxisLabel = mes;         // rótulo no eixo = apenas os existentes
                    p.YValues = new[] { val };
                    s.Points.Add(p);
                }

                // Labels nas barras só se não poluir
                bool mostrarLabels = s.Points.Count <= 24;
                s.IsValueShownAsLabel = mostrarLabels;
                s.Label = mostrarLabels ? "#VALY{N0} Kzs" : "";

                // Títulos
                decimal totalPeriodo = dt.AsEnumerable().Sum(r => r.Field<decimal>("total"));
                chart2.Titles.Add("Evolução de Vendas por Mês").Font = new Font("Segoe UI", 11, FontStyle.Bold);
                var t2 = chart2.Titles.Add($"Total no período: {totalPeriodo:N0} Kzs");
                t2.Font = new Font("Segoe UI", 9); t2.ForeColor = Color.DimGray;

                // Visão inicial: mostra TUDO (sem zoom) e, portanto, sem barras de rolagem
                area.AxisX.ScaleView.ZoomReset(0);
                area.AxisY.ScaleView.ZoomReset(0);
            }
        }

       








        private void PrintDoc_PrintPageChart2(object sender, PrintPageEventArgs e)
        {
            // Cria uma imagem do gráfico
            Bitmap bmp = new Bitmap(chart2.Width, chart2.Height);
            chart2.DrawToBitmap(bmp, new Rectangle(0, 0, chart2.Width, chart2.Height));

            // Calcula posição para centralizar na página
            int pageWidth = e.PageBounds.Width;
            int pageHeight = e.PageBounds.Height;

            int x = (pageWidth - chart2.Width) / 2;
            int y = (pageHeight - chart2.Height) / 2;

            // Desenha o gráfico na posição calculada
            e.Graphics.DrawImage(bmp, x, y);
        }



        private void ImprimirChart2()
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDoc_PrintPageChart2;

            // 💡 Define margens e impressora padrão como PDF
            printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
            printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDoc;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ImprimirChart2(); // Exibe a prévia do gráfico chart2
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ImprimirChart1();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void chart2_DoubleClick_1(object sender, EventArgs e)
        {
            var a = chart2.ChartAreas[0];
            a.AxisX.ScaleView.ZoomReset(0);
            a.AxisY.ScaleView.ZoomReset(0);
        }

        private void chart1_DockChanged(object sender, EventArgs e)
        {
            if (chkZoomAnual.Checked) LoadChartAnual_BarrasZoom();
            else LoadChart(); // seu método de rosquinha já melhorado
        }

        private void chkZoomAnual_CheckStateChanged(object sender, EventArgs e)
        {
            if (chkZoomAnual.Checked) LoadChartAnual_BarrasZoom();
            else LoadChart(); // seu método de rosquinha já melhorado
        }

        private void chkZoomAnual_CheckedChanged(object sender, EventArgs e)
        {

            if (chkZoomAnual.Checked) LoadChartAnual_BarrasZoom();
            else LoadChart();
        }
    }
}
