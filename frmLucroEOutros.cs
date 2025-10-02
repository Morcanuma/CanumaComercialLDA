using lojaCanuma.Services;
using Newtonsoft.Json.Linq;
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
using System.Text.RegularExpressions;

using System.Globalization;






namespace lojaCanuma
{
    public partial class frmLucroEOutros : Form
    {
        List<(DateTime dia, double total)> historicoGlobal;
        List<(DateTime dia, double previsao)> previsaoGlobal;


        // serviço de dados + IA
        private DataService _dataSvc;
        private LlamaService _llama = new LlamaService();
        private string _connString;


        private PrintDocument printDocumentGrafico = new PrintDocument();

        SqlConnection cn = new SqlConnection();
        SqlCommand cm;
        SqlDataReader dr;
        DBConnection db = new DBConnection();


        // construtor “antigo” – que não recebe nada
        public frmLucroEOutros()
            : this(new List<(DateTime, double)>(), new List<(DateTime, double)>())
        { }


        public frmLucroEOutros(
            List<(DateTime dia, double total)> historico,
            List<(DateTime dia, double previsao)> previsao
        )
        {
            InitializeComponent();
            cn.ConnectionString = db.MyConnection();
            _connString = new DBConnection().MyConnection();
            _dataSvc = new DataService(_connString, _llama);

            lblSaldoLucroAtual2.Text = lblSaldoLucroAtual.Text;
            // aqui você pode usar 'historico' e 'previsao'…






            // No construtor, logo após InitializeComponent():
            dtpPrevisao.MinDate = DateTime.Today.AddDays(1);
            dtpPrevisao.MaxDate = DateTime.Today.AddDays(30);  // só permite até 30 dias adiante





        }

        private void lucroPorEstoqueAtualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void amorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmLucro frmLucro = new frmLucro();
            //frmLucro.ShowDialog();
        }

        private void frmLucroEOutros_Load(object sender, EventArgs e)
        {
            CarregarLucroTabela();
            CarregarLucroGrafico();
            CalcularTotaisGlobais();
            CarregarFuncionarios();
            CalcularTotais();
            CarregarGraficoLucroPorProduto();
            
           
            CarregarResumoFinanceiro();

            CalcularLucroDisponivel();

            printDocumentGrafico.PrintPage += printDocumentGrafico_PrintPage;



            dtInicio.Value = DateTime.Now.AddDays(-30); // padrão: últimos 30 dias
            dtFim.Value = DateTime.Now;


            CarregarPainelRisco();


            try
            {
                cn.Open();

                string sql = @"
        SELECT 
            MIN(CAST(DataMovimento AS date)) AS DataMin,
            MAX(CAST(DataMovimento AS date)) AS DataMax
        FROM vwFluxoCaixaComSaldo
        WHERE MONTH(DataMovimento) = MONTH(GETDATE())
          AND YEAR(DataMovimento) = YEAR(GETDATE())
          AND DataMovimento <= GETDATE()";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        if (dr["DataMin"] != DBNull.Value && dr["DataMax"] != DBNull.Value)
                        {
                            dtInicioRisco.Value = Convert.ToDateTime(dr["DataMin"]);
                            dtFimRisco.Value = Convert.ToDateTime(dr["DataMax"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar datas de risco: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }




        }





        private void CarregarLucroTabela()
        {
            dgvLucro.Rows.Clear();
            try
            {
                cn.Open();
                cm = new SqlCommand("SELECT pcode, pdesc, price, cost_price, qty FROM tblProduct WHERE qty > 0 AND cost_price IS NOT NULL", cn);
                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    string pcode = dr["pcode"].ToString();
                    string pdesc = dr["pdesc"].ToString();
                    decimal precoVenda = Convert.ToDecimal(dr["price"]);
                    decimal precoCompra = Convert.ToDecimal(dr["cost_price"]);
                    int estoque = Convert.ToInt32(dr["qty"]);

                    decimal lucroUnit = precoVenda - precoCompra;
                    decimal lucroTotal = lucroUnit * estoque;

                    // Adiciona os dados formatados
                    dgvLucro.Rows.Add(
                        pcode,
                        pdesc,
                        precoCompra.ToString("#,##0.00"), // Compra com separador
                        precoVenda.ToString("#,##0.00"),  // Venda com separador
                        estoque.ToString("#,##0"),        // Quantidade com separador
                        lucroUnit.ToString("#,##0.00"),   // Lucro unitário formatado
                        lucroTotal.ToString("#,##0.00")   // Lucro total formatado
                    );
                }
                dr.Close();
                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao carregar lucro: " + ex.Message);
            }
        }


        private void CarregarLucroGrafico()
        {
            // Limpa gráfico
            chartLucro.Series.Clear();
            chartLucro.Titles.Clear();
            chartLucro.Legends.Clear();

            // Lista de dados
            var dados = new List<(string Nome, decimal Lucro)>();

            // 1) Ler dados do DataGridView
            foreach (DataGridViewRow row in dgvLucro.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[1].Value == null || row.Cells[6].Value == null) continue;

                string nomeProduto = Convert.ToString(row.Cells[1].Value)?.Trim();
                if (string.IsNullOrEmpty(nomeProduto)) continue;

                // Valor de lucro - pode estar com Kz/Kzs ou formatado
                string sVal = Convert.ToString(row.Cells[6].Value);

                // Remove Kz/Kzs ignorando maiúsc/minúsc
                sVal = Regex.Replace(sVal, "(?i)kzs?", "").Trim();

                // Remove espaços extras
                sVal = sVal.Replace(" ", "");

                // Faz o parse seguro
                decimal lucro;
                if (!decimal.TryParse(sVal, out lucro))
                {
                    // fallback para formatos com separador de milhar e vírgula decimal
                    var s2 = sVal.Replace(".", "").Replace(",", ".");
                    decimal.TryParse(s2,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out lucro);
                }

                if (lucro < 0) lucro = 0;
                dados.Add((nomeProduto, lucro));
            }

            // Se não houver dados, sai
            if (dados.Count == 0)
            {
                MessageBox.Show("Nenhum dado de lucro encontrado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2) Criar série
            Series serie = new Series("Lucro Total")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                IsValueShownAsLabel = true,
                LabelForeColor = Color.Black
            };

            // Adiciona pontos
            foreach (var (Nome, Lucro) in dados)
            {
                int idx = serie.Points.AddXY(Nome, Lucro);
                serie.Points[idx].Label = Lucro.ToString("#,##0") + " Kz";
                serie.Points[idx].ToolTip = $"{Nome}: {Lucro:#,##0} Kz";
            }

            chartLucro.Series.Add(serie);

            // 3) Título
            chartLucro.Titles.Add("Lucro Total por Produto");
            chartLucro.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // 4) Legenda
            Legend legend = new Legend
            {
                Title = "Produtos",
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            chartLucro.Legends.Add(legend);

            // 5) Estética
            var area = chartLucro.ChartAreas[0];
            area.BackColor = Color.White;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.Interval = 1;
            area.AxisX.Title = "Produto";
            area.AxisX.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
            area.AxisY.Title = "Lucro Total (Kz)";
            area.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
            area.AxisY.LabelStyle.Format = "#,##0";
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;

            // Zoom
            area.CursorX.IsUserEnabled = true;
            area.CursorX.IsUserSelectionEnabled = true;
            area.AxisX.ScaleView.Zoomable = true;
            area.CursorY.IsUserEnabled = true;
            area.CursorY.IsUserSelectionEnabled = true;
            area.AxisY.ScaleView.Zoomable = true;
        }


        private void CalcularTotaisGlobais()
        {
            decimal lucroTotal = 0;
            decimal custoTotal = 0;

            foreach (DataGridViewRow row in dgvLucro.Rows)
            {
                if (row.Cells[6].Value != null && row.Cells[2].Value != null && row.Cells[4].Value != null)
                {
                    // lucro total
                    decimal lucroItem = decimal.Parse(row.Cells[6].Value.ToString());
                    lucroTotal += lucroItem;

                    // custo total = preco de compra * estoque
                    decimal precoCompra = decimal.Parse(row.Cells[2].Value.ToString());
                    int qtdEstoque = int.Parse(row.Cells[4].Value.ToString());
                    custoTotal += precoCompra * qtdEstoque;
                }
            }

            lblLucroTotalEstoque.Text = $"Lucro Total em Estoque: {lucroTotal:N2} Kz";
            lblCustoTotalEstoque.Text = $"Total Investido no Estoque: {custoTotal:N2} Kz";
        }











        private void CarregarLucroDeVendas()
        {
            dgvLucroVendas.Rows.Clear();
            try
            {
                cn.Open();

                string sql = @"
        SELECT 
            si.pcode,
            p.pdesc,
            si.price AS preco_venda,
            p.cost_price AS preco_compra,
            si.qty,
            si.disc,
            si.sdate,
            si.funcionario,
            (si.price - p.cost_price) * si.qty AS lucro_total
        FROM 
            tblCar si
        JOIN 
            tblProduct p ON si.pcode = p.pcode
        WHERE 
            si.status = 'Sold'
            AND si.sdate BETWEEN @inicio AND @fim";

                // Aplica filtro de funcionário se necessário
                if (!string.IsNullOrWhiteSpace(cbFuncionario.Text))
                {
                    sql += " AND si.funcionario = @funcionario";
                }

                cm = new SqlCommand(sql, cn);
                cm.Parameters.AddWithValue("@inicio", dtInicio.Value.Date);
                cm.Parameters.AddWithValue("@fim", dtFim.Value.Date.AddDays(1).AddTicks(-1));

                if (!string.IsNullOrWhiteSpace(cbFuncionario.Text))
                {
                    cm.Parameters.AddWithValue("@funcionario", cbFuncionario.Text);
                }

                dr = cm.ExecuteReader();
                decimal totalLucro = 0;

                while (dr.Read())
                {
                    decimal lucro = Convert.ToDecimal(dr["lucro_total"]);
                    totalLucro += lucro;

                    dgvLucroVendas.Rows.Add(
                        dr["pcode"].ToString(),
                        dr["pdesc"].ToString(),
                        Convert.ToDecimal(dr["preco_venda"]).ToString("#,##0.00"),
                        Convert.ToDecimal(dr["preco_compra"]).ToString("#,##0.00"),
                        Convert.ToInt32(dr["qty"]).ToString("#,##0"),
                        lucro.ToString("#,##0.00"),
                        Convert.ToDateTime(dr["sdate"]).ToString("g"),
                        dr["funcionario"].ToString()
                    );
                }

                dr.Close();

                // 🔻 Subtrai prejuízos por cancelamentos com acao = 'Não'
                SqlCommand cmdCancel = new SqlCommand(@"
                        SELECT ISNULL(SUM(c.price * c.qty), 0)
                        FROM tblCancel AS c
                        WHERE c.acao = 'Não'
              AND c.sdate BETWEEN @inicio AND @fim", cn);

                cmdCancel.Parameters.AddWithValue("@inicio", dtInicio.Value.Date);
                cmdCancel.Parameters.AddWithValue("@fim", dtFim.Value.Date.AddDays(1).AddTicks(-1));

                decimal prejuizo = Convert.ToDecimal(cmdCancel.ExecuteScalar());

                totalLucro -= prejuizo; // 🔹 subtrai prejuízo do lucro total

                cn.Close();

                // Exibe o resultado final
                lblLucroTotal.Text = "Lucro Realizado: " + totalLucro.ToString("#,##0.00") + " Kz";
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao buscar lucros: " + ex.Message);
            }
        }



        private void CalcularTotais()
        {
            decimal totalLucro = 0;
            foreach (DataGridViewRow row in dgvLucroVendas.Rows)
            {
                if (row.Cells[5].Value != null)
                    totalLucro += Convert.ToDecimal(row.Cells[5].Value);
            }

            lblLucroTotal.Text = "Lucro Realizado: " + totalLucro.ToString("#,##0.00") + " Kz";
        }



        private void CarregarFuncionarios()
        {
            cbFuncionario.Items.Clear();
            cbFuncionario.Items.Add(""); // Todos

            try
            {
                cn.Open();
                cm = new SqlCommand("SELECT DISTINCT name FROM tblUser", cn);
                dr = cm.ExecuteReader();
                while (dr.Read())
                {
                    cbFuncionario.Items.Add(dr["name"].ToString());
                }
                dr.Close();
                cn.Close();
            }
            catch
            {
                cn.Close();
            }
        }


        private void CarregarGraficoLucroPorProduto(int topN = 10, bool agruparOutros = true, bool barrasVerticais = true)
        {
            var c = chartLucroPorVendas;

            // limpa tudo
            c.Series.Clear();
            c.Titles.Clear();
            c.Legends.Clear();

            // garante ChartArea
            ChartArea area = (c.ChartAreas.Count > 0) ? c.ChartAreas[0] : c.ChartAreas.Add("Main");
            area.BackColor = Color.White;
            area.AxisX.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;

            // agrega a partir da grid (produto = col 1, lucro = col 5)
            var lucroPorProduto = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewRow row in dgvLucroVendas.Rows)
            {
                if (row.IsNewRow) continue;

                string prod = Convert.ToString(row.Cells[1].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(prod)) continue;

                string bruto = Convert.ToString(row.Cells[5].Value) ?? "";

                // remove "Kz"/"Kzs" e espaços (inclui NBSP)
                bruto = Regex.Replace(bruto, @"\s*kzs?\s*", "", RegexOptions.IgnoreCase);
                bruto = bruto.Replace("\u00A0", "").Trim();

                // tenta decimal com pt-PT (1.234,56)
                decimal valor;
                if (!decimal.TryParse(bruto,
                                      NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                                      new CultureInfo("pt-PT"), out valor))
                {
                    // fallback: tira milhar "." e usa "." como decimal
                    string norm = bruto.Replace(".", "").Replace(",", ".");
                    decimal.TryParse(norm, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
                }

                if (!lucroPorProduto.ContainsKey(prod)) lucroPorProduto[prod] = 0m;
                lucroPorProduto[prod] += valor;
            }

            if (lucroPorProduto.Count == 0)
            {
                c.Titles.Add("Sem dados de lucro por produto no período selecionado");
                c.Invalidate();
                return;
            }

            // ordena, aplica TopN e agrupa "Outros"
            var itens = lucroPorProduto.OrderByDescending(kv => kv.Value).ToList();
            decimal totalGeral = itens.Sum(i => i.Value);

            if (agruparOutros && itens.Count > topN)
            {
                var top = itens.Take(topN).ToList();
                decimal outras = itens.Skip(topN).Sum(i => i.Value);
                if (outras > 0) top.Add(new KeyValuePair<string, decimal>("Outros", outras));
                itens = top;
            }

            // série
            var s = new Series("Lucro por Produto")
            {
                ChartArea = area.Name,
                ChartType = barrasVerticais ? SeriesChartType.Column : SeriesChartType.Bar,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            // paleta simples
            Color[] cores =
            {
        Color.FromArgb(52,120,246),  // azul
        Color.FromArgb(0,184,148),   // verde
        Color.FromArgb(255,159,67),  // laranja
        Color.FromArgb(232,67,147),  // rosa
        Color.FromArgb(111,66,193),  // roxo
        Color.FromArgb(255,99,132),  // vermelho
        Color.FromArgb(75,192,192),  // teal
        Color.FromArgb(253,203,110)  // amarelo
    };

            // adiciona pontos
            for (int i = 0; i < itens.Count; i++)
            {
                string prod = itens[i].Key;
                decimal val = itens[i].Value;

                // encurta nomes muito grandes (evita sobreposição)
                string prodCurto = prod.Length > 28 ? prod.Substring(0, 28) + "…" : prod;

                int idx = s.Points.AddXY(prodCurto, (double)val);
                var p = s.Points[idx];
                p.Color = cores[i % cores.Length];
                p.Label = string.Format("{0:#,##0} Kz", val);  // rótulo curto
                double perc = totalGeral > 0 ? (double)(val / totalGeral) : 0;
                p.ToolTip = $"{prod}\n{val:#,##0} Kz ({perc:P0})";
            }

            // smart labels pra evitar colisão
            s.SmartLabelStyle.Enabled = true;
            s.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            s.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Right;

            // aparência geral e eixos
            c.BackColor = Color.White;
            c.BorderlineColor = Color.Gainsboro;
            c.BorderlineWidth = 1;
            c.BorderlineDashStyle = ChartDashStyle.Solid;

            if (barrasVerticais)
            {
                // X = categorias (produtos), Y = valores
                area.AxisX.Title = "Produto";
                area.AxisY.Title = "Lucro (Kz)";
                area.AxisX.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
                area.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);

                area.AxisX.Interval = 1;
                area.AxisX.LabelStyle.Angle = -45;
                area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8, FontStyle.Regular);
                area.AxisY.LabelStyle.Format = "#,##0";
                area.AxisY.Minimum = 0;

                // folga no topo
                double max = itens.Max(i => (double)i.Value);
                if (max <= 0) max = 1000;
                area.AxisY.Maximum = Math.Ceiling(max * 1.15);

                // largura da barra
                s["PointWidth"] = "0.6";
            }
            else
            {
                // HORIZONTAL: X = valores, Y = categorias
                area.AxisX.Title = "Lucro (Kz)";
                area.AxisY.Title = "Produto";
                area.AxisX.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
                area.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);

                area.AxisY.Interval = 1;
                area.AxisY.LabelStyle.Angle = 0;
                area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8, FontStyle.Regular);
                area.AxisX.LabelStyle.Format = "#,##0";
                area.AxisX.Minimum = 0;

                double max = itens.Max(i => (double)i.Value);
                if (max <= 0) max = 1000;
                area.AxisX.Maximum = Math.Ceiling(max * 1.15);

                s["PointWidth"] = "0.6";
            }

            c.Series.Add(s);

            // título
            var titulo = new Title(
                $"Lucro por Produto (Top {Math.Min(topN, lucroPorProduto.Count)})",
                Docking.Top,
                new Font("Segoe UI", 11, FontStyle.Bold),
                Color.Black
            );
            c.Titles.Add(titulo);

            c.Invalidate();
        }




        private void printDocumentGrafico_PrintPage(object sender, PrintPageEventArgs e)
        {
            // 🔄 Força o gráfico a ser desenhado antes de capturar imagem
            chartLucroPorVendas.Update();
            chartLucroPorVendas.Refresh();

            // 🖼️ Cria bitmap com o tamanho do gráfico
            Bitmap bmp = new Bitmap(chartLucroPorVendas.Width, chartLucroPorVendas.Height);

            // 🧲 Captura o gráfico para imagem
            chartLucroPorVendas.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            // 🖨️ Imprime no local desejado
            e.Graphics.DrawImage(bmp, 100, 100);
        }



        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintPreviewDialog printPreview = new PrintPreviewDialog();
            printPreview.Document = printDocumentGrafico;
            printPreview.ShowDialog();

        }





        private void CarregarResumoFinanceiro()
        {
            using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
            {
                cn.Open();

                // 1. Vendas Confirmadas (vwSoldItems já tem o campo "total")
                lblTotalVendas.Text = ObterTotal(cn, "SELECT ISNULL(SUM(total), 0) FROM vwSoldItems") + " Kzs";

                // 2. Compras (usando qty * price)
               lblTotalCompras.Text = lblCustoTotalEstoque.Text.Replace("Total Investido no Estoque:", "");


                // 3. Salários
                lblTotalSalarioPagos.Text = ObterTotal(cn, "SELECT ISNULL(SUM(valor), 0) FROM tblPagamentosFuncionarios") + " Kzs";

                // 4. Despesas Fixas
                lblTotalDespesasFixas.Text = ObterTotal(cn, "SELECT ISNULL(SUM(valor), 0) FROM tblDespesasFixas") + " Kzs";

                // 5. Cancelamentos (usando view que tem price e qty)
                lblTotalCancelamento.Text = ObterTotal(cn,
                    "SELECT ISNULL(SUM(c.price * c.qty), 0) FROM tblCancel AS c INNER JOIN tblProduct AS p ON c.pcode = p.pcode") + " Kzs";

            }
        }


        public async void CalcularLucroDisponivel()
        {
            using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
            {
                cn.Open();

                decimal lucroBruto = ObterTotalDecimal(cn, @"
                        SELECT ISNULL(SUM((c.price - p.cost_price) * c.qty), 0)
                        FROM tblCar AS c
                        JOIN tblProduct AS p ON c.pcode = p.pcode
                        WHERE c.status = 'Sold'");

                decimal despesasFixas = ObterTotalDecimal(cn, "SELECT ISNULL(SUM(valor), 0) FROM tblDespesasFixas");
                decimal salarios = ObterTotalDecimal(cn, "SELECT ISNULL(SUM(valor), 0) FROM tblPagamentosFuncionarios");
                decimal cancelamentos = ObterTotalDecimal(cn, @"
                    SELECT ISNULL(SUM(c.price * c.qty), 0)
                    FROM tblCancel AS c
                    WHERE acao = 'Não'"); // só prejuízo real

                decimal lucroDisponivel = lucroBruto - (despesasFixas + salarios + cancelamentos);
                lblSaldoLucroAtual.Text = lucroDisponivel.ToString("N2") + " Kz";
                lblSaldoLucroAtual2.Text = lblSaldoLucroAtual.Text;


                // Sugestão com IA
                string prompt = $@"
                               Sou um pequeno empresário angolano do setor alimentício (cantinas/lojas) com um lucro mensal atual de {lucroDisponivel:N2} Kz.  
                            **Objetivo:** Acumular 6.500.000,00 Kz em 6 meses para abrir uma nova cantina em Luanda (custo médio em 2025), sem comprometer:  
                            - Folha salarial atual  
                            - Impostos (incluindo riscos de atualizações fiscais)  
                            - Reserva para emergências (10% do lucro)  

                            **Contexto Econômico (junho/2025):**  
                            - Inflação: 19,73% (em queda, mas ainda impactante)  
                            - Crescimento do PIB não petrolífero: 3,6%  
                            - Setor alimentar urbano: resiliente, porém com custos operacionais crescentes (logística, energia)  

                            **Requisitos para o Conselho:**  
                            1. **Cálculo Realista:**  
                               - Qual % do meu lucro atual posso retirar mensalmente **sem prejudicar** a meta dos 6.500.000 Kz?  
                               - Considere:  
                                 - Inflação sobre custos futuros (ex: preços de equipamentos/estoques podem subir)  
                                 - Margem de segurança (ex: atrasos na abertura?)  

                            2. **Estratégia de Acumulação:**  
                               - Devo aplicar parte do lucro em instrumentos de curto prazo (ex: letras do Tesouro do BNA com liquidez em 6 meses)?  
                               - Como mitigar riscos cambiais (ex: se parte dos equipamentos for importado)?  

                            3. **Priorização:**  
                               - Se o lucro atual for insuficiente, qual é o mínimo necessário para atingir a meta sem endividamento?  

                            **Formato da Resposta:**  
                            - Passo a passo simples (ex: ""1. Guarde X Kz/mês; 2. Reduza Y custos..."")  
                            - Advertências claras (ex: ""Evite Z se quiser manter liquidez"")  
                            - Comparação com benchmarks do setor (ex: ""Outras cantinas em Luanda estão reservando 15% do lucro para inflação"")  
                                ";



                var ia = new LlamaService();
                string respostaIA = await ia.EnviarPerguntaAsync(prompt);

                richTextBoxIA.Text = "🤖 " + respostaIA;
            }
        }


        private decimal ObterTotalDecimal(SqlConnection cn, string query)
        {
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }





        private string ObterTotal(SqlConnection cn, string query)
        {
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                return Convert.ToDecimal(cmd.ExecuteScalar()).ToString("N2");
            }
        }






        




        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            CarregarLucroDeVendas();              // 🔄 carrega os dados na dgvLucroVendas
            CalcularTotais();                     // 🧮 atualiza o valor do lucro total
            CarregarGraficoLucroPorProduto(topN: 10, agruparOutros: true, barrasVerticais: true);

        }



        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void btnAddDespesa_Click(object sender, EventArgs e)
        {
            frmDespesas frm = new frmDespesas();
            frm.FrmPai = this; // passa referência do formulário atual (pai)
            frm.ShowDialog();
        }

        private void btnPagrFunc_Click(object sender, EventArgs e)
        {
            frmPagarFunc frm = new frmPagarFunc();
            frm.FrmPai = this; // passa referência do formulário atual (pai)
            frm.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmFluxoCaixa frm = new frmFluxoCaixa();
            frm.ShowDialog();
        }

      
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            frmLucroVenda frm = new frmLucroVenda(this);
            frm.ShowDialog();
        }

        private async void btnGerar_Click(object sender, EventArgs e)
        {
            // 🗓️ Data que o usuário escolheu para prever até lá
            DateTime dataFim = dtpPrevisao.Value.Date;

            // 🔍 Define período de histórico: 7 dias antes da data escolhida
            DateTime dataIni = dataFim.AddDays(-7);

            // 📊 Busca histórico de vendas
            var historico = await _dataSvc.GetHistoricalSalesAsync(dataIni, dataFim);


            // 🚨 Se não houver vendas no histórico, avisa o usuário
            if (historico.Count == 0)
            {
                MessageBox.Show(
                    "Não há vendas registradas no histórico entre " +
                    dataIni.ToString("dd/MM") + " e " + dataFim.ToString("dd/MM") +
                    ".\n\nTente escolher uma data mais próxima de hoje.",
                    "Sem Histórico",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // ✅ Garante que o último dia real do histórico seja usado como referência
            DateTime ultimaDataHistorico = historico.Max(h => h.dia);

            // ✅ Calcula corretamente os dias que realmente precisam ser previstos
            int diasFuturos = (dataFim - ultimaDataHistorico).Days;
            if (diasFuturos < 1) diasFuturos = 1;
            if (diasFuturos > 30)
            {
                MessageBox.Show(
                    "Só conseguimos prever até 30 dias à frente para manter a qualidade e performance.",
                    "Limite de Previsão",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 🤖 Pede à IA a previsão dos próximos dias reais
            var previsao = await _dataSvc.ForecastSalesAsync(historico, diasFuturos);

            // 2) Armazena nos campos globais para relatórios e IA
            historicoGlobal = historico;
            previsaoGlobal = previsao;

            // 🧹 Limpa as tabelas de exibição
            dgvHistorico.Rows.Clear();
            dgvPrevisao.Rows.Clear();

            // 📈 Mostra o histórico na tela
            foreach (var (d, t) in historico)
                dgvHistorico.Rows.Add(d.ToString("dd/MM"), t.ToString("N2"));

            // 🔮 Mostra previsão da IA na tela
            foreach (var (d, p) in previsao)
                dgvPrevisao.Rows.Add(d.ToString("dd/MM"), p.ToString("N2"));

            // 📊 Atualiza o gráfico com histórico + previsão
            AtualizarChartPrevisao(historico, previsao);
        }


        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (historicoGlobal == null || previsaoGlobal == null)
            {
                MessageBox.Show("Primeiro gere o histórico e a previsão clicando em Gerar.",
                                "Nenhum dado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new frmRelatorioPrevisao(historicoGlobal, previsaoGlobal);
            form.ShowDialog();
        }

        private void AtualizarChartPrevisao(
            List<(DateTime dia, double total)> hist,
            List<(DateTime dia, double prev)> previsao)
                {
            var c = chartPrevisao;
            c.Series.Clear();

            // Série histórico
            var s1 = c.Series.Add("Histórico");
            s1.ChartType = SeriesChartType.Line;
            s1.Color = Color.DodgerBlue;
            s1.BorderWidth = 2;
            s1.MarkerStyle = MarkerStyle.Circle;
            s1.MarkerSize = 6;
            s1.MarkerColor = Color.DodgerBlue;
            s1.MarkerBorderColor = Color.White;
            s1.MarkerBorderWidth = 1;
            s1.XValueType = ChartValueType.String;
            s1.YValueType = ChartValueType.Double;

            // Série previsão
            var s2 = c.Series.Add("Previsão");
            s2.ChartType = SeriesChartType.Line;
            s2.Color = Color.OrangeRed;
            s2.BorderDashStyle = ChartDashStyle.Dash;
            s2.BorderWidth = 2;
            s2.MarkerStyle = MarkerStyle.Circle;
            s2.MarkerSize = 6;
            s2.MarkerColor = Color.OrangeRed;
            s2.MarkerBorderColor = Color.White;
            s2.MarkerBorderWidth = 1;

            // Adicionar pontos
            foreach (var (d, t) in hist)
                s1.Points.AddXY(d.ToString("dd/MM"), t);
            foreach (var (d, p) in previsao)
                s2.Points.AddXY(d.ToString("dd/MM"), p);

            // Eixos e escala
            var area = c.ChartAreas[0];
            area.AxisX.Title = "Dia/Mês";
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.MajorGrid.Enabled = false;

            // 🔍 Define o valor máximo para escalar o eixo Y corretamente
            double maxValor = Math.Max(
                hist.Select(h => h.total).DefaultIfEmpty(0).Max(),
                previsao.Select(p => p.prev).DefaultIfEmpty(0).Max());

            // 🔢 Calcula um intervalo proporcional ao máximo
            double intervalo = Math.Ceiling(maxValor / 6 / 1000) * 1000;
            if (intervalo < 1000) intervalo = 1000;

            // 🧼 Ajuste do eixo Y
            area.AxisY.Title = "Vendas (Kz)";
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.Interval = intervalo;
            area.AxisY.LabelStyle.Format = "N0"; // mostra 10.000 em vez de 10000.00

            // Legenda
            c.Legends.Clear();
            c.Legends.Add(new Legend { Docking = Docking.Bottom, Title = "Séries" });

            // Título
            c.Titles.Clear();
            c.Titles.Add("Vendas: Histórico x Previsão")
                .Font = new Font("Segoe UI", 11, FontStyle.Bold);


            // 🧵 Linha de separação entre Histórico e Previsão
            if (previsao.Any())
            {
                string dataInicioPrevisao = previsao.First().dia.ToString("dd/MM");

                StripLine separador = new StripLine();
                separador.StripWidth = 0;
                separador.BackColor = Color.Red;
                separador.BorderColor = Color.Red;
                separador.BorderWidth = 2;
                separador.BorderDashStyle = ChartDashStyle.Dash;
                separador.IntervalOffset = chartPrevisao.Series["Histórico"].Points.Count; // base na ordem

                // Aplica a faixa vertical no eixo X
                area.AxisX.StripLines.Clear();
                area.AxisX.StripLines.Add(separador);
            }

        }




        private async void CarregarPainelRisco()
        {
            // 📅 Leitura das datas selecionadas nos DateTimePickers
            DateTime inicio = dtInicioRisco.Value.Date;
            DateTime fim = dtFimRisco.Value.Date.AddDays(1).AddTicks(-1); // incluir o fim do dia

            // 📊 Instancia o serviço de risco com conexão
            var riscoSvc = new RiscoFinanceiroService(_connString);
            var dados = await riscoSvc.AnalisarRiscoAsync(inicio, fim);

            // 🔴🟡🟢 Alerta visual mensal
            lblRiscoMensal.Text = dados.AlertaMensal;
            lblRiscoMensal.ForeColor = dados.AlertaMensal.Contains("🔴") ? Color.Red :
                                       dados.AlertaMensal.Contains("🟡") ? Color.Orange : Color.Green;

            // 🔴🟡🟢 Alerta visual total
            lblRiscoTotal.Text = dados.AlertaTotal;
            lblRiscoTotal.ForeColor = dados.AlertaTotal.Contains("🔴") ? Color.Red :
                                      dados.AlertaTotal.Contains("🟡") ? Color.Orange : Color.Green;

            // 🎯 Título dinâmico do gráfico
            string inicioTexto = inicio.ToString("dd/MM/yyyy");
            string fimTexto = fim.ToString("dd/MM/yyyy");

            chartRisco.Titles.Clear(); // Limpa títulos anteriores
            chartRisco.Titles.Add($"📉 Risco Financeiro de {inicioTexto} até {fimTexto}");
            chartRisco.Titles[0].Font = new Font("Segoe UI Emoji", 11F, FontStyle.Bold);
            chartRisco.Titles[0].ForeColor = Color.Black;
            chartRisco.Titles[0].Alignment = ContentAlignment.TopCenter;
            chartRisco.Titles[0].Docking = Docking.Top;
            chartRisco.Titles[0].IsDockedInsideChartArea = false;



            // 📊 Carrega gráfico de barras com dados
            CarregarGraficoRisco(
                dados.DespesasMensais,
                dados.SalariosMensais,
                dados.CancelamentosMensais,
                dados.LucroMensal
            );

            // 📌 Chama a análise inteligente
            string analise = GerarAnaliseInteligente(
                dados.LucroMensal,
                dados.DespesasMensais,
                dados.SalariosMensais,
                dados.CancelamentosMensais
            );

            // 🧠 Exibe a análise em um RichTextBox ou Label (por exemplo: richTextBoxRisco)
            richTextBox1.Text = analise;


        }

        private void CarregarGraficoRisco(
           decimal despesasFixas,
           decimal salarios,
           decimal cancelamentos,
           decimal lucro)
        {
            // --- Preparação segura ---
            chartRisco.Series.Clear();
            chartRisco.Titles.Clear();
            chartRisco.Legends.Clear();

            // Garante uma ChartArea
            ChartArea area = (chartRisco.ChartAreas.Count > 0)
                ? chartRisco.ChartAreas[0]
                : chartRisco.ChartAreas.Add("Main");

            area.BackColor = Color.White;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;

            area.AxisX.Title = "Categoria";
            area.AxisY.Title = "Valor (Kz)";
            area.AxisX.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
            area.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
            area.AxisX.LabelStyle.Angle = 0;
            area.AxisY.LabelStyle.Format = "#,##0";   // rótulos no eixo (sem Kz para não poluir)
            area.AxisY.Minimum = 0;

            // Calcula máximo com margem
            double max = new double[]
            {
        (double)despesasFixas, (double)salarios, (double)cancelamentos, (double)lucro
            }.Max();

            if (max <= 0) max = 1000;
            area.AxisY.Maximum = Math.Ceiling(max * 1.15); // +15% de respiro

            // --- Série principal (colunas) ---
            Series s = new Series("Distribuição");
            s.ChartType = SeriesChartType.Column;
            s.IsValueShownAsLabel = true;
            s.ChartArea = area.Name;
            s.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            s.LabelForeColor = Color.Black;

            // Ordem fixa das categorias para leitura
            string[] nomes = { "Despesas", "Salários", "Cancelamentos", "Lucro" };
            decimal[] valores = { despesasFixas, salarios, cancelamentos, lucro };
            Color[] cores =
            {
        Color.FromArgb(255,159,67),  // Despesas - laranja
        Color.FromArgb(52,120,246),  // Salários  - azul
        Color.FromArgb(220,53,69),   // Cancelamentos - vermelho
        Color.FromArgb(40,167,69)    // Lucro - verde
    };

            for (int i = 0; i < nomes.Length; i++)
            {
                int idx = s.Points.AddXY(nomes[i], (double)valores[i]);
                var p = s.Points[idx];
                p.Color = cores[i];
                p.Label = string.Format("{0:#,##0} Kz", valores[i]);     // rótulo em cada coluna
                p.ToolTip = nomes[i] + ": " + string.Format("{0:#,##0} Kz", valores[i]);
                p.BorderWidth = 0;
            }

            chartRisco.Series.Add(s);

            // --- Linha de referência do Lucro (ajuda a comparar) ---
            // mostra uma faixa/línea horizontal no valor de lucro
            area.AxisY.StripLines.Clear();
            var linhaLucro = new StripLine
            {
                Interval = 0,
                IntervalOffset = (double)lucro,
                BorderColor = Color.FromArgb(40, 167, 69),
                BorderDashStyle = ChartDashStyle.Dash,
                BorderWidth = 2
            };
            area.AxisY.StripLines.Add(linhaLucro);

            // (opcional) anotação de texto para a linha do lucro
            var anot = new TextAnnotation
            {
                Text = "Linha do Lucro: " + string.Format("{0:#,##0} Kz", lucro),
                ForeColor = Color.FromArgb(40, 167, 69),
                AnchorAlignment = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                X = 1, // relativo ao chart area
                Y = (double)lucro
            };
            chartRisco.Annotations.Clear();
            chartRisco.Annotations.Add(anot);

            // --- Título clean ---
            var titulo = new Title("Distribuição Financeira do Período",
                                   Docking.Top,
                                   new Font("Segoe UI", 11, FontStyle.Bold),
                                   Color.Black);
            chartRisco.Titles.Add(titulo);

            // Aparência geral
            chartRisco.BackColor = Color.White;
            chartRisco.BorderlineColor = Color.Gainsboro;
            chartRisco.BorderlineWidth = 1;
            chartRisco.BorderlineDashStyle = ChartDashStyle.Solid;

            chartRisco.Invalidate(); // redesenha
        }


        private double CalcularMaximoComMargem(params decimal[] valores)
        {
            decimal max = valores.Max();
            if (max == 0) return 1000; // valor padrão mínimo se tudo for 0
            return (double)(max * 1.15M); // adiciona 15% de margem no topo
        }




        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            CarregarPainelRisco();
        }

        private void dtFimRisco_ValueChanged(object sender, EventArgs e)
        {
            CarregarPainelRisco();
        }


        private string GerarAnaliseInteligente(decimal lucro, decimal despesas, decimal salarios, decimal cancelamentos)
        {
            decimal totalGastos = despesas + salarios + cancelamentos;
            decimal percentualGasto = (totalGastos == 0 || lucro == 0) ? 0 : (totalGastos / lucro) * 100;

            string status = "";
            if (percentualGasto < 30)
                status = "excelente";
            else if (percentualGasto < 50)
                status = "boa";
            else if (percentualGasto < 70)
                status = "regular";
            else
                status = "preocupante";

            string mensagem = "📊 Análise Inteligente:\n" +
                              $"- Lucro total: {lucro:N0} Kz\n" +
                              $"- Gastos totais: {totalGastos:N0} Kz ({percentualGasto:N1}%)\n" +
                              $"- Situação: {status.ToUpper()}.\n\n";

            if (percentualGasto < 50)
            {
                mensagem += "✅ Você pode reinvestir parte do lucro ou expandir com segurança.";
            }
            else if (percentualGasto < 70)
            {
                mensagem += "⚠️ Atenção: os gastos estão crescendo. Avalie reduzir custos ou aumentar vendas.";
            }
            else
            {
                mensagem += "❌ Risco alto: o lucro está sendo consumido por despesas. Reavalie sua operação.";
            }

            return mensagem;
        }




    }
}
