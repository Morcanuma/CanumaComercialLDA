using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lojaCanuma
{
    public partial class frmClientesAnalise : Form
    {
        private readonly string _conn = new DBConnection().MyConnection();

        public frmClientesAnalise()
        {
            InitializeComponent();

            // valores padrão dos filtros
            dtIni.Value = DateTime.Today.AddMonths(-3);
            dtFim.Value = DateTime.Today;
            numMinCompras.Value = 1;
            numTop.Value = 20;

            ConfigurarGrid();
            CarregarDados();   // primeira carga
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ConfigurarGrid()
        {
            dgvClientes.AutoGenerateColumns = false;
            dgvClientes.Columns.Clear();

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerId", HeaderText = "ID", Width = 60 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Nome", Width = 200 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QtdCompras", HeaderText = "Compras", Width = 80 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UltimaCompra", HeaderText = "Última Compra", Width = 120 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalGasto", HeaderText = "Total Gasto (Kz)", Width = 120, DefaultCellStyle = { Format = "N2" } });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TicketMedio", HeaderText = "Ticket Médio (Kz)", Width = 120, DefaultCellStyle = { Format = "N2" } });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DiasSemComprar", HeaderText = "Dias Sem Comprar", Width = 110 });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Segmento", HeaderText = "Segmento", Width = 100 });

            dgvClientes.CellFormatting += dgvClientes_CellFormatting; // cores por segmento
            dgvClientes.DoubleClick += (s, e) => AbrirHistoricoClienteAtual();
        }

        private void dgvClientes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvClientes.Columns[e.ColumnIndex].DataPropertyName == "Segmento" && e.Value != null)
            {
                var seg = e.Value.ToString();
                var row = dgvClientes.Rows[e.RowIndex];
                if (seg == "VIP") row.DefaultCellStyle.BackColor = System.Drawing.Color.Honeydew;
                else if (seg == "Frequente") row.DefaultCellStyle.BackColor = System.Drawing.Color.AliceBlue;
                else if (seg == "Inativo") row.DefaultCellStyle.BackColor = System.Drawing.Color.MistyRose;
                else row.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e) => CarregarDados();

       

      
        private void AbrirHistoricoClienteAtual()
        {
            if (dgvClientes.CurrentRow == null) return;
            var drv = dgvClientes.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            int id = Convert.ToInt32(drv["CustomerId"]);
            using (var frm = new frmHistoricoCliente(id))
                frm.ShowDialog();
        }

        private void AtualizarGrafico(DataTable dt)
        {
            chartTop.Series.Clear();
            var s = chartTop.Series.Add("Top clientes");
            s.ChartType = SeriesChartType.Column;
            s.IsValueShownAsLabel = true;
            s.LabelFormat = "N0";

            // top 10 do DataTable
            int i = 0;
            foreach (DataRow r in dt.Rows)
            {
                if (i++ >= 10) break;
                s.Points.AddXY(r["Name"].ToString(), Convert.ToDecimal(r["TotalGasto"]));
            }
        }

        //private void CarregarDados()
        //{
        //    // ➤ OPÇÃO A) Usando a VIEW que você criou (sem filtro de período)
        //    //    Se você quer PERÍODO, use a OPÇÃO B logo abaixo.

        //    var tabela = new DataTable();
        //    using (var cn = new SqlConnection(_conn))
        //    using (var cmd = new SqlCommand(@"
        //                    SELECT TOP (@top)
        //                           v.CustomerId,
        //                           v.Name,
        //                           v.QtdCompras,
        //                           v.UltimaCompra,
        //                           v.TotalGasto,
        //                           v.TicketMedio,
        //                           DATEDIFF(day, v.UltimaCompra, GETDATE()) AS DiasSemComprar,
        //                           CASE 
        //                             WHEN v.TotalGasto >= 1000000 THEN 'VIP'
        //                             WHEN v.QtdCompras >= 5 THEN 'Frequente'
        //                             WHEN DATEDIFF(day, v.UltimaCompra, GETDATE()) > 60 THEN 'Inativo'
        //                             ELSE 'Regular'
        //                           END AS Segmento
        //                    FROM vwClientesResumo v
        //                    WHERE v.QtdCompras >= @minCompras
        //                    ORDER BY v.TotalGasto DESC;", cn))
        //    {
        //        cmd.Parameters.AddWithValue("@minCompras", (int)numMinCompras.Value);
        //        cmd.Parameters.AddWithValue("@top", (int)numTop.Value);
        //        cn.Open();
        //        tabela.Load(cmd.ExecuteReader());
        //    }

        //    dgvClientes.DataSource = tabela;

        //    // resumo
        //    decimal total = 0;
        //    foreach (DataRow r in tabela.Rows) total += r.Field<decimal>("TotalGasto");
        //    lblResumo.Text = $"Clientes: {tabela.Rows.Count} | Total Gasto: {total:N2} Kz";

        //    AtualizarGrafico(tabela);
        //}

        private void btnHistorico_Click_1(object sender, EventArgs e) => AbrirHistoricoClienteAtual();

        private void btnExportar_Click_1(object sender, EventArgs e)
        {
            if (dgvClientes.DataSource is DataTable dt)
            {
                using (var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "ClientesAnalise.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var sb = new StringBuilder();
                        // cabeçalho
                        sb.AppendLine(string.Join(";", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
                        // linhas
                        foreach (DataRow r in dt.Rows)
                            sb.AppendLine(string.Join(";", dt.Columns.Cast<DataColumn>().Select(c => r[c].ToString())));
                        System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Exportado com sucesso!");
                    }
                }
            }
        }

        private void btnAtualizar_Click_1(object sender, EventArgs e) => CarregarDados();
   




        ///*  ➤ OPÇÃO B) Se precisar filtrar por PERÍODO, substitua CarregarDados()
        //    por esta versão que consulta a TABELA de vendas:

        private void CarregarDados()
        {
            var dt = new DataTable();
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
SELECT TOP(@top)
    x.CustomerId,
    x.Name,
    x.QtdCompras,
    x.UltimaCompra,
    x.TotalGasto,
    x.TicketMedio,
    DATEDIFF(day, x.UltimaCompra, GETDATE()) AS DiasSemComprar,
    CASE 
       WHEN x.TotalGasto >= 1000000 THEN 'VIP'
       WHEN x.QtdCompras >= 5 THEN 'Frequente'
       WHEN DATEDIFF(day, x.UltimaCompra, GETDATE()) > 60 THEN 'Inativo'
       ELSE 'Regular'
    END AS Segmento
FROM (
    SELECT
        c.CustomerId,
        c.Name,
        COUNT(*) AS QtdCompras,
        MAX(si.sdate) AS UltimaCompra,
        SUM((si.price * si.qty) - si.disc) AS TotalGasto,
        CAST(SUM((si.price * si.qty) - si.disc) / NULLIF(COUNT(*),0) AS decimal(18,2)) AS TicketMedio
    FROM tblCar si
    JOIN tblCustomer c ON c.CustomerId = si.CustomerId
    WHERE si.status='Sold'
      AND si.sdate BETWEEN @ini AND @fim
    GROUP BY c.CustomerId, c.Name
    HAVING COUNT(*) >= @minCompras
) x
ORDER BY x.TotalGasto DESC;", cn))
            {
                cmd.Parameters.AddWithValue("@ini", dtIni.Value.Date);
                cmd.Parameters.AddWithValue("@fim", dtFim.Value.Date.AddDays(1).AddTicks(-1));
                cmd.Parameters.AddWithValue("@minCompras", (int)numMinCompras.Value);
                cmd.Parameters.AddWithValue("@top", (int)numTop.Value);

                cn.Open();
                dt.Load(cmd.ExecuteReader());
            }

            dgvClientes.DataSource = dt;
            decimal total = 0;
            foreach (DataRow r in dt.Rows) total += r.Field<decimal>("TotalGasto");
            lblResumo.Text = $"Clientes: {dt.Rows.Count} | Total Gasto: {total:N2} Kz";
            AtualizarGrafico(dt);
        }

        private void btnProgramaPontos_Click(object sender, EventArgs e)
        {
            frmLoyaltyAndPromos frm = new frmLoyaltyAndPromos();
            frm.ShowDialog();
        }
    }
}
