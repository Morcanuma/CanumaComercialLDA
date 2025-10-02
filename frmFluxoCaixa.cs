using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmFluxoCaixa : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm;
        SqlDataReader dr;
        DBConnection db = new DBConnection();
        public frmFluxoCaixa()
        {
            InitializeComponent();
            cn.ConnectionString = db.MyConnection();
            this.AutoScroll = true;
        }


        private void CarregarFluxoCaixa()
        {
            dgvFluxoCaixa.Rows.Clear();

            try
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT 
                DataMovimento,
                TipoMovimento,
                Categoria,
                Valor,
                Saldo
            FROM vwFluxoCaixaComSaldo
            WHERE DataMovimento BETWEEN @inicio AND @fim
            ORDER BY DataMovimento", cn))
                {
                    cmd.Parameters.AddWithValue("@inicio", dtInicio.Value.Date);
                    cmd.Parameters.AddWithValue("@fim", dtFim.Value.Date.AddDays(1).AddTicks(-1));

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            dgvFluxoCaixa.Rows.Add(
                                dr.GetDateTime(0).ToString("g"),
                                dr.GetString(1),
                                dr.GetString(2),
                                dr.GetDecimal(3).ToString("#,##0.00"),
                                dr.GetDecimal(4).ToString("#,##0.00")
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar fluxo de caixa: " + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close();
            }

            // Mostra o saldo final (lucro líquido)
            // Atualiza os cálculos dos lucros
            CalcularLucros();

        }



       
            private void frmFluxoCaixa_Load(object sender, EventArgs e)
        {
            // 🕒 Define dtInicio e dtFim com base nos movimentos do mês atual até hoje
            try
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT 
                MIN(CAST(DataMovimento AS date)) AS DataMin,
                MAX(CAST(DataMovimento AS date)) AS DataMax
            FROM vwFluxoCaixaComSaldo
            WHERE MONTH(DataMovimento) = MONTH(GETDATE())
              AND YEAR(DataMovimento) = YEAR(GETDATE())
              AND DataMovimento <= GETDATE()", cn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["DataMin"] != DBNull.Value && dr["DataMax"] != DBNull.Value)
                            {
                                dtInicio.Value = Convert.ToDateTime(dr["DataMin"]);
                                dtFim.Value = Convert.ToDateTime(dr["DataMax"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao definir datas do fluxo: " + ex.Message,
                                "Erro de Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                cn.Close();
            }

            // ✅ Agora que as datas estão corretas, pode carregar os dados
            CarregarFluxoCaixa();
            CarregarGraficoLucro();
        }




        

        private void btnCarregarFluxo_Click(object sender, EventArgs e)
        {
            CarregarFluxoCaixa();
            CarregarGraficoLucro();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CalcularLucros()
        {
            decimal lucroBruto = 0;
            decimal despesasTotais = 0;

            foreach (DataGridViewRow row in dgvFluxoCaixa.Rows)
            {
                if (row.IsNewRow) continue;

                string tipo = row.Cells[1].Value?.ToString() ?? "";
                decimal valor = 0;

                decimal.TryParse(row.Cells[3].Value?.ToString(), out valor);

                if (tipo == "Venda")
                {
                    lucroBruto += valor;
                }
                else if (
                    tipo.Contains("Despesa") ||
                    tipo.Contains("Salário") ||
                    tipo.Contains("Cancelamento")
                )
                {
                    despesasTotais += Math.Abs(valor); // soma o valor como positivo
                }
            }

            decimal lucroLiquido = lucroBruto - despesasTotais;

            lblLucroBruto.Text = $"💰 Lucro Bruto: {lucroBruto:N2} Kz";
            lblDespesasTotais.Text = $"📉 Despesas totais: -{despesasTotais:N2} Kz";
            lblLucroLiquido.Text = $"🟢 Lucro Líquido: {lucroLiquido:N2} Kz";

            // Altera a cor se negativo
            lblLucroLiquido.ForeColor = lucroLiquido >= 0 ? Color.Green : Color.Red;
        }





        private void CarregarGraficoLucro()
        {
            // 1) Limpa configurações anteriores
            chartLucro.Series.Clear();
            chartLucro.ChartAreas.Clear();
            chartLucro.Legends.Clear();
            chartLucro.Titles.Clear();

            // 2) Área e legenda
            var area = new System.Windows.Forms.DataVisualization.Charting.ChartArea("MainArea");
            chartLucro.ChartAreas.Add(area);
            chartLucro.Legends.Add(new System.Windows.Forms.DataVisualization.Charting.Legend("Legenda")
            {
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Title = "Legenda"
            });

            // 3) Série de colunas para Lucro Líquido
            var serie = new System.Windows.Forms.DataVisualization.Charting.Series("Lucro Líquido")
            {
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.String,
                YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double,
                Legend = "Legenda",
                ChartArea = "MainArea",
                IsValueShownAsLabel = true,
                Label = "#VALY Kz",
                LabelForeColor = Color.Black,
                Color = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            chartLucro.Series.Add(serie);

            // 4) Preenchendo dados
            try
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT 
                FORMAT(DataMovimento, 'MMM/yyyy') AS mes_ano,
                ISNULL(SUM(Valor), 0.0) AS lucro
            FROM vwFluxoCaixaComSaldo
            WHERE TipoMovimento = 'Venda'
              AND DataMovimento BETWEEN @inicio AND @fim
            GROUP BY FORMAT(DataMovimento, 'MMM/yyyy'),
                     YEAR(DataMovimento), MONTH(DataMovimento)
            ORDER BY YEAR(DataMovimento), MONTH(DataMovimento)
        ", cn))
                {
                    cmd.Parameters.AddWithValue("@inicio", dtInicio.Value.Date);
                    cmd.Parameters.AddWithValue("@fim", dtFim.Value.Date.AddDays(1).AddTicks(-1));

                    var da = new SqlDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds, "FluxoMensal");

                    chartLucro.DataSource = ds.Tables["FluxoMensal"];
                    serie.XValueMember = "mes_ano";
                    serie.YValueMembers = "lucro";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar gráfico de lucro: " + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close();
            }

            // 5) Título e estética
            chartLucro.Titles.Add("Lucro Mensal com Base em Vendas")
                     .Font = new Font("Segoe UI", 10, FontStyle.Bold);

            var ax = chartLucro.ChartAreas[0].AxisX;
            var ay = chartLucro.ChartAreas[0].AxisY;

            ax.Title = "Mês/Ano";
            ax.LabelStyle.Format = "";
            ax.LabelStyle.Angle = -45;
            ax.Interval = 1;
            ax.MajorGrid.Enabled = false;

            ay.Title = "Lucro Bruto (Kz)";
            ay.LabelStyle.Format = "#,##0";
            ay.MajorGrid.Enabled = false;
            serie.Label = "#VALY{#,##0} Kz";
            

            chartLucro.BackColor = Color.White;
            chartLucro.ChartAreas[0].BackColor = Color.White;

            // 6) Zoom com mouse
            ax.ScaleView.Zoomable = true;
            chartLucro.ChartAreas[0].CursorX.IsUserEnabled = true;
            chartLucro.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;

            ay.ScaleView.Zoomable = true;
            chartLucro.ChartAreas[0].CursorY.IsUserEnabled = true;
            chartLucro.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        }


        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var relatorio = new frmRelatorioLucros
            {
                DataInicio = dtInicio.Value,
                DataFim = dtFim.Value
            };
            relatorio.ShowDialog();

        }

    }
}
