using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace lojaCanuma
{
    public partial class frmRecords : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public frmRecords()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadRecord()
        {
            try
            {
                int i = 0;
                dataGridView1.Rows.Clear();

                cn.Open();

                // Verifica opção selecionada
                if (cboTopSellect.Text == "ORD. POR QTD.")
                {
                    cm = new SqlCommand(@"SELECT TOP 10 pcode, pdesc, ISNULL(SUM(qty), 0) AS qty, ISNULL(SUM(total), 0) AS total
                                  FROM vwSoldItems
                                  WHERE sdate BETWEEN @from AND @to AND status = 'Sold'
                                  GROUP BY pcode, pdesc
                                  ORDER BY qty DESC", cn);
                }
                else if (cboTopSellect.Text == "ORD. POR VALOR")
                {
                    cm = new SqlCommand(@"SELECT TOP 10 pcode, pdesc, ISNULL(SUM(qty), 0) AS qty, ISNULL(SUM(total), 0) AS total
                                  FROM vwSoldItems
                                  WHERE sdate BETWEEN @from AND @to AND status = 'Sold'
                                  GROUP BY pcode, pdesc
                                  ORDER BY total DESC", cn);
                }
                else
                {
                    // Se não houver opção selecionada, usa padrão por quantidade
                    cm = new SqlCommand(@"SELECT TOP 10 pcode, pdesc, ISNULL(SUM(qty), 0) AS qty, ISNULL(SUM(total), 0) AS total
                                  FROM vwSoldItems
                                  WHERE sdate BETWEEN @from AND @to AND status = 'Sold'
                                  GROUP BY pcode, pdesc
                                  ORDER BY qty DESC", cn);
                }

                // Parâmetros de data
                cm.Parameters.AddWithValue("@from", dateTimePicker1.Value.Date);
                cm.Parameters.AddWithValue("@to", dateTimePicker2.Value.Date.AddDays(1).AddTicks(-1));

                dr = cm.ExecuteReader();

                // Preenche a grid
                while (dr.Read())
                {
                    i++;
                    double total = Convert.ToDouble(dr["total"]);

                    dataGridView1.Rows.Add(
                        i,
                        dr["pcode"].ToString(),
                        dr["pdesc"].ToString(),
                        dr["qty"].ToString(),
                        total.ToString("N2") + " Kzs"
                    );
                }

                dr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }



        public void CancelledOrders()
        {
            int i = 0;
            dataGridView5.Rows.Clear(); // Limpa o DataGrid antes de carregar novos dados
            cn.Open(); // Abre a conexão com o banco

            // Consulta parametrizada para evitar SQL Injection
            string query = @"
                            SELECT * FROM vwCancelOrders 
                            WHERE CONVERT(date, sdate) BETWEEN @startDate AND @endDate";

            // Cria o comando e adiciona os parâmetros de forma segura
            cm = new SqlCommand(query, cn);
            cm.Parameters.AddWithValue("@startDate", dateTimePicker5.Value.Date);
            cm.Parameters.AddWithValue("@endDate", dateTimePicker6.Value.Date);

            dr = cm.ExecuteReader();

            // Percorre os dados retornados e adiciona ao DataGridView
            while (dr.Read())
            {
                i++;
                dataGridView5.Rows.Add(
                    i,
                    dr["transno"].ToString(),
                    dr["pcode"].ToString(),
                    dr["pdesc"].ToString(),
                    Convert.ToDecimal(dr["price"]).ToString("N2"),        // Preço formatado
                    dr["qty"].ToString(),
                    Convert.ToDecimal(dr["total"]).ToString("N2"),        // Total formatado
                    Convert.ToDateTime(dr["sdate"]).ToString("dd/MM/yyyy HH:mm"),
                    dr["voidby"].ToString().ToUpper(),                    // Cancelado por (em maiúsculas)
                    dr["cancelledby"].ToString().ToUpper(),              // Autorizado por (em maiúsculas)
                    dr["reason"].ToString(),
                    dr["acao"].ToString()
                );
            }

            dr.Close();
            cn.Close(); // Fecha a conexão com segurança
        }


        

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void frmRecords_Load(object sender, EventArgs e)
        {
            LoadRecord();
            GerarRecomendacoes();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int i = 0;
                dataGridView2.Rows.Clear();
                cn.Open();

                string sql = @"
            SELECT 
                c.pcode, 
                p.pdesc, 
                c.price, 
                SUM(c.qty) AS tot_qty, 
                SUM(c.disc) AS tot_disc,
                SUM((c.price * c.qty) - c.disc) AS total_real
            FROM tblcar AS c
            INNER JOIN tblProduct AS p ON c.pcode = p.pcode
            WHERE c.status = 'Sold' 
              AND c.sdate BETWEEN @from AND @to
            GROUP BY c.pcode, p.pdesc, c.price";

                cm = new SqlCommand(sql, cn);
                cm.Parameters.AddWithValue("@from", dateTimePicker4.Value.Date);
                cm.Parameters.AddWithValue("@to", dateTimePicker3.Value.Date.AddDays(1).AddTicks(-1));

                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    i++;
                    double preco = Convert.ToDouble(dr["price"]);
                    int qty = Convert.ToInt32(dr["tot_qty"]);
                    double desconto = Convert.ToDouble(dr["tot_disc"]);
                    double totalReal = Convert.ToDouble(dr["total_real"]);

                    dataGridView2.Rows.Add(
                        i,
                        dr["pcode"].ToString(),
                        dr["pdesc"].ToString(),
                        preco.ToString("N2") + " Kz",
                        qty,
                        desconto.ToString("N2") + " Kz",
                        totalReal.ToString("N2") + " Kz"
                    );
                }
                dr.Close();

                // Soma total final com base no DataGridView (coluna de TOTAL)
                double totalFinal = 0;
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells[6].Value != null)
                    {
                        string texto = row.Cells[6].Value.ToString().Replace("Kz", "").Trim();
                        if (double.TryParse(texto, out double valor))
                        {
                            totalFinal += valor;
                        }
                    }
                }

                lblTotal.Text = totalFinal.ToString("N2") + " Kz";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close();
            }
        }



        public void LoadCriticalItems()
        {
            try
            {
                dataGridView3.Rows.Clear(); // Limpa o grid antes de carregar os dados
                int i = 0;
                cn.Open();

                cm = new SqlCommand("SELECT * FROM vwCriticalItems", cn); // Usa a View vwCritical
                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    i++;
                    dataGridView3.Rows.Add(
                        i,                              // Nº
                        dr[0].ToString(),               // Código do produto (pcode)
                        dr[1].ToString(),               // Código de barras
                        dr[2].ToString(),               // Descrição do produto
                        dr[3].ToString(),               // Marca
                        dr[4].ToString(),               // Categoria
                        Convert.ToDouble(dr[5]).ToString("N2"), // Preço formatado
                        dr[6].ToString(),               // Reorder
                        dr[7].ToString()                // Quantidade atual
                    );
                }

                dr.Close();
                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "Erro ao carregar produtos críticos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void LoadInventory()
        {
            try
            {
                int i = 0;
                dataGridView4.Rows.Clear();
                cn.Open();

                cm = new SqlCommand(@"SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, 
                                     p.price, p.reorder, p.qty 
                                    FROM tblProduct AS p
                                    INNER JOIN tblBrand AS b ON p.bid = b.id
                                    INNER JOIN tblCategory AS c ON p.cid = c.id
                                    ", cn);

                dr = cm.ExecuteReader();
                while (dr.Read())
                {
                    i++;
                    dataGridView4.Rows.Add(i,
                        dr["pcode"].ToString(),
                        dr["barcode"].ToString(),
                        dr["pdesc"].ToString(),
                        dr["brand"].ToString(),
                        dr["category"].ToString(),
                        Convert.ToDecimal(dr["price"]).ToString("N2"),
                        dr["reorder"].ToString(),
                        dr["qty"].ToString());
                }
                dr.Close();
                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport frm = new frmInventoryReport();
            frm.LoadReport();
            frm.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CancelledOrders();
        }


        public void LoadStockInHistory()
        {
            int i = 0;
            dataGridView6.Rows.Clear(); // Limpa o DataGridView antes de carregar novos dados

            try
            {
                cn.Open(); // Abre a conexão com o banco de dados

                // Consulta com parâmetros para evitar SQL Injection e garantir integridade
                string query = @"
                            SELECT * FROM vwStockin 
                            WHERE CAST(sdate AS DATE) 
                            BETWEEN @startDate AND @endDate 
                            AND status = 'Done'";

                using (SqlCommand cm = new SqlCommand(query, cn))
                {
                    // Adiciona os parâmetros de forma segura
                    cm.Parameters.AddWithValue("@startDate", dateTimePicker7.Value.Date);
                    cm.Parameters.AddWithValue("@endDate", dateTimePicker8.Value.Date);

                    using (SqlDataReader dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            i++; // Contador de linhas

                            dataGridView6.Rows.Add(
                                i,
                                dr["id"].ToString(),
                                dr["refno"].ToString(),
                                dr["pcode"].ToString(),
                                dr["pdesc"].ToString(),
                                dr["qty"].ToString(),
                                Convert.ToDateTime(dr["sdate"]).ToString("dd/MM/yyyy"),
                                dr["stockinby"].ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar histórico de estoque:\n" + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cn.Close(); // Garante que a conexão seja fechada mesmo em caso de erro
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadStockInHistory();
        }

      

        private void btnSave_Click(object sender, EventArgs e)
        {
            LoadRecord();
        }

        private void linkLabel6_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboTopSellect.Text))
            {
                MessageBox.Show("Por favor, selecione o critério de ordenação.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ordenacao = cboTopSellect.Text;

            frmInventoryReport frm = new frmInventoryReport();
            frm.LoadTopSelling(dateTimePicker1.Value, dateTimePicker2.Value, ordenacao); // agora com 3 argumentos
            frm.ShowDialog();

        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var frm = new frmInventoryReport();
            frm.LoadSoldItemsReport(dateTimePicker4.Value, dateTimePicker3.Value);
            frm.ShowDialog();

        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport frm = new frmInventoryReport();
            frm.LoadCriticalItemsReport();
            frm.ShowDialog();

        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DateTime startDate = dateTimePicker5.Value.Date;
            DateTime endDate = dateTimePicker6.Value.Date;

            frmInventoryReport frm = new frmInventoryReport();
            frm.LoadCancelledOrdersReport(startDate, endDate);
            frm.ShowDialog();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DateTime startDate = dateTimePicker7.Value.Date;
            DateTime endDate = dateTimePicker8.Value.Date;

            frmInventoryReport frm = new frmInventoryReport();
            frm.LoadStockInHistoryReport(startDate, endDate);
            frm.ShowDialog();
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {


            if (string.IsNullOrWhiteSpace(cboTopSellect.Text))
            {
                MessageBox.Show(
                    "Por favor, selecione um critério de ordenação antes de continuar.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
                LoadRecord();
            LoadChartTopSelling();
        }

        private void cboTopSellect_TabIndexChanged(object sender, EventArgs e)
        {
            LoadRecord(); // Recarrega os dados com nova ordenação
        }

        private void cboTopSellect_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecord(); // Recarrega os dados com nova ordenação
        }



        public void LoadChartTopSelling()
        {
            try
            {
                cn.Open();
                string ordenacao = cboTopSellect.Text;

                // Define a consulta SQL conforme a ordenação
                string query = @"
            SELECT TOP 10 pcode, pdesc, ISNULL(SUM(qty), 0) AS qty, ISNULL(SUM(total), 0) AS total
            FROM vwSoldItems
            WHERE sdate BETWEEN @from AND @to AND status = 'Sold'
            GROUP BY pcode, pdesc
            ORDER BY " + (ordenacao == "ORD. POR VALOR" ? "total" : "qty") + " DESC";

                cm = new SqlCommand(query, cn);
                cm.Parameters.AddWithValue("@from", dateTimePicker1.Value.Date);
                cm.Parameters.AddWithValue("@to", dateTimePicker2.Value.Date.AddDays(1).AddTicks(-1));

                SqlDataAdapter da = new SqlDataAdapter(cm);
                DataSet ds = new DataSet();
                da.Fill(ds, "TOPSELLING");

                // Configura o gráfico
                chart1.Series.Clear();
                chart1.DataSource = ds.Tables["TOPSELLING"];

                Series series = new Series("TOP VENDAS");
                series.ChartType = SeriesChartType.Doughnut;
                series.XValueMember = "pdesc";
                series.YValueMembers = (ordenacao == "ORD. POR VALOR") ? "total" : "qty";
                series.IsValueShownAsLabel = true;

                // Formatação dos valores
                if (ordenacao == "ORD. POR VALOR")
                {
                    series.LabelFormat = "#,##0.00' Kz'";
                }
                else
                {
                    series.LabelFormat = "#,##0";
                }

                // Adiciona a série ao gráfico
                chart1.Series.Add(series);
                chart1.Legends[0].Enabled = true;
                chart1.Legends[0].Docking = Docking.Right;
                chart1.Legends[0].Font = new Font("Segoe UI", 8, FontStyle.Regular);
                chart1.Legends[0].IsDockedInsideChartArea = false;

                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar gráfico: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (cn.State == ConnectionState.Open) cn.Close();
            }
        }


        private void PrintChartPage(object sender, PrintPageEventArgs e)
        {
            // Desenha o gráfico como imagem
            Bitmap bmp = new Bitmap(chart1.Width, chart1.Height);
            chart1.DrawToBitmap(bmp, new Rectangle(0, 0, chart1.Width, chart1.Height));

            int x = (e.PageBounds.Width - bmp.Width) / 2;
            int y = 100;

            // Título centralizado
            using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
            {
                string titulo = "Top 10 Produtos Mais Vendidos";
                SizeF tamanho = e.Graphics.MeasureString(titulo, font);
                float tituloX = (e.PageBounds.Width - tamanho.Width) / 2;
                e.Graphics.DrawString(titulo, font, Brushes.Black, tituloX, 40);
            }

            e.Graphics.DrawImage(bmp, x, y);
        }



        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintChartPage;

            PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 900,
                Height = 600
            };

            preview.ShowDialog(); // Pré-visualiza com opção de imprimir/salvar em PDF
        
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmChar frm = new frmChar();

            // Título formatado
            string dataInicial = dateTimePicker4.Value.ToString("dd MMM yyyy");
            string dataFinal = dateTimePicker3.Value.ToString("dd MMM yyyy");
            frm.lblTitle.Text = $"📊 Relatório de Vendas: {dataInicial} até {dataFinal}";

            // SQL corrigido
            string sql = @"
        SELECT 
            p.pdesc, 
            SUM((c.price * c.qty) - c.disc) AS total_real
        FROM tblcar AS c
        INNER JOIN tblProduct AS p ON c.pcode = p.pcode
        WHERE c.status = 'Sold' 
          AND c.sdate BETWEEN @from AND @to
        GROUP BY p.pdesc
        ORDER BY total_real DESC";

            frm.CarregarGraficoVendas(sql, dateTimePicker4.Value.Date, dateTimePicker3.Value.Date);
            frm.ShowDialog();
        }


        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }



        private void GerarRecomendacoes()
        {
            try
            {
                dgvRecomendacoes.Rows.Clear();
                var dataIni = DateTime.Today.AddDays(-30);
                var dataFim = DateTime.Today;

                lblPeriodo.Text = $"Analisando últimos 30 dias de vendas: {dataIni:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}";

                string sql = "SELECT * FROM vwRecomendacoesIA";

                using (var cn = new SqlConnection(new DBConnection().MyConnection()))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        int abaixoIdeal = 0;

                        while (dr.Read())
                        {
                            string pcode = dr["pcode"].ToString();
                            string pdesc = dr["pdesc"].ToString();
                            int vendidos30 = Convert.ToInt32(dr["vendidos30dias"]);
                            int mediaSemanal = Convert.ToInt32(dr["mediaSemanal"]);
                            int estoqueAtual = Convert.ToInt32(dr["estoqueAtual"]);
                            int recomendado = Convert.ToInt32(dr["recomendado"]);
                            string status = dr["status"].ToString();
                            double impacto = Convert.ToDouble(dr["impactoPercentual"]);

                            if (status == "Crítico") abaixoIdeal++;

                            dgvRecomendacoes.Rows.Add(
                                pcode, pdesc, vendidos30, mediaSemanal,
                                estoqueAtual, recomendado, $"{impacto:N2}%", status
                            );
                        }

                        lblResumo.Text = $"Total de produtos com estoque crítico: {abaixoIdeal}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar recomendações:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dateTimePicker10_ValueChanged(object sender, EventArgs e)
        {

        }

        private void metroContextMenu1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void btnCarregar_Click(object sender, EventArgs e)
        {
            try
            {
                dgvInventario.Rows.Clear();
                btnSalvar.Enabled = true;

                DateTime dataRef = new DateTime(dtpMes.Value.Year, dtpMes.Value.Month, 1);

                // 👉 Verifica se já existe inventário no banco
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tblInventarioMensal WHERE dataRef = @ref", cn))
                    {
                        cmd.Parameters.AddWithValue("@ref", dataRef);
                        int count = (int)cmd.ExecuteScalar();

                        // 👉 Se não existir, gera o inventário para o mês
                        if (count == 0)
                        {
                            GerarInventarioMensal(dataRef);
                        }
                    }
                }

                // 👉 Agora carrega os dados da tabela tblInventarioMensal
                string sql = @"
            SELECT 
                p.pcode,
                p.pdesc,
                inv.estoqueInicial,
                inv.vendidos,
                inv.esperado,
                inv.estoqueAtual,
                inv.contado,
                inv.diferenca,
                inv.status
            FROM tblInventarioMensal inv
            INNER JOIN tblProduct p ON p.pcode = inv.pcode
            WHERE inv.dataRef = @ref";

                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ref", dataRef);
                    cn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            dgvInventario.Rows.Add(
                                dr["pcode"],
                                dr["pdesc"],
                                dr["estoqueInicial"],
                                dr["vendidos"],
                                dr["esperado"],
                                dr["estoqueAtual"],
                                dr["contado"],
                                dr["diferenca"],
                                dr["status"]
                            );
                        }

                        AplicarCoresInventario();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar o inventário: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void GerarInventarioMensal(DateTime dataRef)
        {
            // Define data de início e fim do mês
            DateTime inicio = dataRef;
            DateTime fim = dataRef.AddMonths(1).AddDays(-1);

            string insertSql = @"
        INSERT INTO tblInventarioMensal (pcode, dataRef, estoqueInicial, vendidos, esperado, estoqueAtual, contado, diferenca, status)
        SELECT 
            p.pcode,
            @dataRef,
            p.qty + ISNULL(v.vendidos, 0),       -- estoqueInicial
            ISNULL(v.vendidos, 0),               -- vendidos
            (p.qty + ISNULL(v.vendidos, 0)) - ISNULL(v.vendidos, 0), -- esperado
            p.qty,                               -- estoqueAtual
            NULL, NULL, NULL
        FROM tblProduct p
        LEFT JOIN (
            SELECT c.pcode, SUM(c.qty) AS vendidos
            FROM tblCar c
            WHERE c.status='Sold' AND c.sdate BETWEEN @inicio AND @fim
            GROUP BY c.pcode
        ) v ON v.pcode = p.pcode";

            using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
            using (SqlCommand cmd = new SqlCommand(insertSql, cn))
            {
                cmd.Parameters.AddWithValue("@dataRef", dataRef);
                cmd.Parameters.AddWithValue("@inicio", inicio);
                cmd.Parameters.AddWithValue("@fim", fim);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();

                    foreach (DataGridViewRow row in dgvInventario.Rows)
                    {
                        if (row.IsNewRow) continue;

                        // Verifica se a célula "contado" está vazia
                        if (row.Cells[6].Value == null || row.Cells[6].Value == DBNull.Value || string.IsNullOrWhiteSpace(row.Cells[6].Value.ToString()))
                            continue;

                        string pcode = row.Cells[0].Value.ToString();
                        int esperado = Convert.ToInt32(row.Cells[4].Value);
                        int contado = Convert.ToInt32(row.Cells[6].Value);

                        int diferenca = contado - esperado;
                        string status;

                        if (diferenca == 0)
                            status = "Inventário Correto";
                        else if (diferenca < 0)
                            status = "Diferença Negativa (Possível Perda)";
                        else
                            status = "Diferença Positiva (Estoque a Mais)";

                        // Atualiza na base de dados
                        string updateSql = @"
                    UPDATE tblInventarioMensal
                    SET contado = @contado,
                        diferenca = @diferenca,
                        status = @status
                    WHERE pcode = @pcode AND dataRef = @dataRef";

                        using (SqlCommand cmd = new SqlCommand(updateSql, cn))
                        {
                            cmd.Parameters.AddWithValue("@contado", contado);
                            cmd.Parameters.AddWithValue("@diferenca", diferenca);
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@pcode", pcode);
                            cmd.Parameters.AddWithValue("@dataRef", new DateTime(dtpMes.Value.Year, dtpMes.Value.Month, 1));
                            cmd.ExecuteNonQuery();
                        }

                        // Atualiza visualmente a grid
                        row.Cells[7].Value = diferenca;
                        row.Cells[8].Value = status;


                        AplicarCoresInventario();


                    }

                    MessageBox.Show("Inventário salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar inventário: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarCoresInventario()
        {
            foreach (DataGridViewRow row in dgvInventario.Rows)
            {
                if (row.IsNewRow) continue;

                string status = row.Cells["colStatus"].Value?.ToString(); // substitua "colStatus" pelo nome correto da coluna Status
                if (status == null) continue;

                // Define a cor com base no status
                Color cor;

                if (status.StartsWith("Inventário"))
                    cor = Color.LightGreen;
                else if (status.StartsWith("Diferença Negativa"))
                    cor = Color.LightCoral;
                else if (status.StartsWith("Diferença Positiva"))
                    cor = Color.Khaki;
                else
                    cor = Color.White;

                // Aplica a cor em todas as células da linha
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = cor;
                }
            }
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var resultado = MessageBox.Show("Deseja imprimir com os dados preenchidos?\n(Clique em Não para imprimir o modelo em branco)",
                                  "Impressão do Inventário",
                                  MessageBoxButtons.YesNoCancel,
                                  MessageBoxIcon.Question);

            if (resultado == DialogResult.Cancel)
                return;

            bool comDados = (resultado == DialogResult.Yes);
            DateTime dataRef = new DateTime(dtpMes.Value.Year, dtpMes.Value.Month, 1);

            frmRelatorioInventario frm = new frmRelatorioInventario(dataRef, comDados);
            frm.ShowDialog();
        }
    }
}
