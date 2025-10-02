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
    public partial class frmChar : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public frmChar()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

    

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public void CarregarGraficoVendas(string sql, DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                chart1.Series.Clear();
                chart2.Series.Clear();
                chart1.Titles.Clear();
                chart2.Titles.Clear();
                chart1.ChartAreas.Clear();
                chart2.ChartAreas.Clear();
                chart1.Legends.Clear();
                chart2.Legends.Clear();

                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                using (SqlCommand cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", dataInicio);
                    cm.Parameters.AddWithValue("@to", dataFim);

                    SqlDataAdapter da = new SqlDataAdapter(cm);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Nenhum dado encontrado para gerar os gráficos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 📊 Gráfico de Rosquinha
                    ChartArea areaRosquinha = new ChartArea("Rosquinha");
                    chart1.ChartAreas.Add(areaRosquinha);
                    chart1.Legends.Add(new Legend("LegendaRosquinha") { Docking = Docking.Right, Font = new Font("Segoe UI", 8) });

                    Series serieRosquinha = new Series("Produtos")
                    {
                        ChartType = SeriesChartType.Doughnut,
                        ["DoughnutRadius"] = "60",
                        ["PieLabelStyle"] = "Outside",
                        ["PieLineColor"] = "Black",
                        IsValueShownAsLabel = false // Para não sobrecarregar o centro
                    };

                    foreach (DataRow row in dt.Rows)
                    {
                        string produto = row["pdesc"].ToString();
                        double total = Convert.ToDouble(row["total_real"]);
                        var ponto = new DataPoint(0, total)
                        {
                            LegendText = $"{produto}\n{total:N0} Kz"
                        };
                        serieRosquinha.Points.Add(ponto);
                    }

                    chart1.Series.Add(serieRosquinha);
                    chart1.Titles.Add(new Title("Distribuição de Vendas (Rosquinha)", Docking.Top, new Font("Segoe UI", 10, FontStyle.Bold), Color.Black));

                    // 📈 Gráfico de Barras
                    ChartArea areaBarras = new ChartArea("Barras");
                    areaBarras.AxisX.Title = "Total Vendido (Kz)";
                    areaBarras.AxisY.Title = "Produto";
                    areaBarras.AxisX.LabelStyle.Format = "N0";
                    areaBarras.AxisX.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
                    areaBarras.AxisY.LabelStyle.Font = new Font("Segoe UI", 8);
                    chart2.ChartAreas.Add(areaBarras);

                    Series serieBarras = new Series("Top Produtos")
                    {
                        ChartType = SeriesChartType.Bar,
                        Font = new Font("Segoe UI", 8),
                        IsValueShownAsLabel = true,
                        LabelForeColor = Color.Black
                    };

                    foreach (DataRow row in dt.Rows)
                    {
                        string produto = row["pdesc"].ToString();
                        double total = Convert.ToDouble(row["total_real"]);
                        int index = serieBarras.Points.AddXY(produto, total);
                        serieBarras.Points[index].Label = $"{total:N0} Kz";
                    }

                    chart2.Series.Add(serieBarras);
                    chart2.Titles.Add(new Title("Top Produtos por Total Vendido", Docking.Top, new Font("Segoe UI", 10, FontStyle.Bold), Color.Black));

                    // Estética
                    chart1.BackColor = Color.White;
                    chart2.BackColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar gráfico de vendas: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void PrintChartsPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                // Cria imagens dos gráficos
                Bitmap bmpChart1 = new Bitmap(chart1.Width, chart1.Height);
                chart1.DrawToBitmap(bmpChart1, new Rectangle(0, 0, chart1.Width, chart1.Height));

                Bitmap bmpChart2 = new Bitmap(chart2.Width, chart2.Height);
                chart2.DrawToBitmap(bmpChart2, new Rectangle(0, 0, chart2.Width, chart2.Height));

                int pageWidth = e.PageBounds.Width;

                // Título geral centralizado
                string titulo = "📊 Gráficos de Vendas (Rosquinha + Barras)";
                using (Font fonteTitulo = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    SizeF tamanhoTitulo = e.Graphics.MeasureString(titulo, fonteTitulo);
                    float xTitulo = (pageWidth - tamanhoTitulo.Width) / 2;
                    e.Graphics.DrawString(titulo, fonteTitulo, Brushes.Black, xTitulo, 40);
                }

                // Posicionamento dos gráficos
                int margemSuperior = 100;
                int espacamentoEntreGraficos = 50;

                int x1 = (pageWidth - bmpChart1.Width) / 2;
                e.Graphics.DrawImage(bmpChart1, x1, margemSuperior);

                int y2 = margemSuperior + bmpChart1.Height + espacamentoEntreGraficos;
                int x2 = (pageWidth - bmpChart2.Width) / 2;
                e.Graphics.DrawImage(bmpChart2, x2, y2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao imprimir os gráficos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintChartsPage;

            PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 900,
                Height = 600
            };

            preview.ShowDialog(); // Mostra pré-visualização com opção de imprimir/salvar como PDF
        }
    }
}
