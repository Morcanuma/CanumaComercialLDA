using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq; // precisa para Take/Skip

namespace lojaCanuma
{
    public partial class frmDespesaGrafico : Form
    {
        DBConnection db = new DBConnection();

        public frmDespesaGrafico()
        {
            InitializeComponent();
            this.Load += frmDespesaGrafico_Load; // garante o vínculo do evento
        }

        private void frmDespesaGrafico_Load(object sender, EventArgs e)
        {
            CarregarGraficoDespesas();
        }


        private void CarregarGraficoDespesas()
        {
            try
            {
                chartDespesas.Series.Clear();
                chartDespesas.Titles.Clear();

                // ChartArea básica
                ChartArea area;
                if (chartDespesas.ChartAreas.Count == 0)
                    area = chartDespesas.ChartAreas.Add("Main");
                else
                    area = chartDespesas.ChartAreas[0];

                area.BackColor = Color.White;
                area.AxisX.MajorGrid.LineColor = Color.Gainsboro;
                area.AxisY.MajorGrid.Enabled = false;
                area.AxisX.LabelStyle.Format = "#,##0' Kz'";
                area.AxisX.Title = "Valor (Kz)";
                area.AxisY.Title = "Tipo de Despesa";

                // Série de barras horizontais (Bar)
                var serie = new Series("Despesas");
                serie.ChartType = SeriesChartType.Bar;
                serie.IsValueShownAsLabel = true;
                serie.ChartArea = area.Name;
                serie.Font = new Font("Segoe UI", 9, FontStyle.Bold); // sem SemiBold

                // Paleta simples
                Color[] cores =
                {
            Color.FromArgb(52,120,246),
            Color.FromArgb(0,184,148),
            Color.FromArgb(255,159,67),
            Color.FromArgb(232,67,147),
            Color.FromArgb(111,66,193),
            Color.FromArgb(255,99,132),
            Color.FromArgb(75,192,192),
            Color.FromArgb(253,203,110)
        };

                // Carrega dados do banco (tipo, total)
                var dados = new List<(string tipo, decimal total)>();
                using (var cn = new SqlConnection(new DBConnection().MyConnection()))
                using (var cmd = new SqlCommand(@"
            SELECT tipo, SUM(valor) AS total
            FROM tblDespesasFixas
            GROUP BY tipo
            HAVING SUM(valor) > 0
            ORDER BY total DESC;", cn))
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string tipo = dr.GetString(0);
                            decimal total = dr.GetDecimal(1);
                            dados.Add((tipo, total));
                        }
                    }
                }

                if (dados.Count == 0)
                {
                    chartDespesas.Titles.Add("Sem dados de despesas para exibir");
                    chartDespesas.Invalidate();
                    return;
                }

                // Top N + “Outras” (opcional). Ajuste o N se quiser.
                int topN = 8;
                if (dados.Count > topN)
                {
                    var top = dados.Take(topN).ToList();
                    decimal outras = dados.Skip(topN).Sum(x => x.total);
                    if (outras > 0) top.Add(("Outras", outras));
                    dados = top;
                }

                // Preenche pontos
                for (int i = 0; i < dados.Count; i++)
                {
                    int idx = serie.Points.AddXY(dados[i].tipo, (double)dados[i].total);
                    var pt = serie.Points[idx];
                    pt.Color = cores[i % cores.Length];
                    pt.Label = string.Format("{0:#,##0} Kz", dados[i].total);
                    pt.ToolTip = dados[i].tipo + ": " + string.Format("{0:#,##0} Kz", dados[i].total);
                }

                chartDespesas.Series.Add(serie);

                // Título clean
                var titulo = new Title("Despesas Fixas por Tipo", Docking.Top,
                                       new Font("Segoe UI", 11, FontStyle.Bold), Color.Black);
                chartDespesas.Titles.Add(titulo);

                // Aparência geral
                chartDespesas.BackColor = Color.White;
                chartDespesas.Legends.Clear(); // sem legenda para ficar limpo
                chartDespesas.BorderlineColor = Color.Gainsboro;
                chartDespesas.BorderlineWidth = 1;
                chartDespesas.BorderlineDashStyle = ChartDashStyle.Solid;

                chartDespesas.Invalidate(); // redesenha
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar gráfico:\n" + ex.Message,
                                "Erro", System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
            }
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
