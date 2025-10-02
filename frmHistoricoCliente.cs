using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmHistoricoCliente : Form
    {
        private readonly int _customerId;
        private readonly string _conn = new DBConnection().MyConnection();

        public frmHistoricoCliente(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            Text = $"Histórico do Cliente #{customerId}";
            CarregarHistorico();
        }

        private void CarregarHistorico()
        {
            var dt = new DataTable();
            using (var cn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(@"
                SELECT 
                    CONVERT(date, si.sdate) AS Data,
                    SUM((si.price*si.qty) - si.disc) AS TotalDoDia,
                    SUM(si.qty) AS Itens
                FROM tblCar si
                WHERE si.status='Sold' AND si.CustomerId=@id
                GROUP BY CONVERT(date, si.sdate)
                ORDER BY Data DESC;", cn))
                            {
                cmd.Parameters.AddWithValue("@id", _customerId);
                cn.Open();
                dt.Load(cmd.ExecuteReader());
            }

            dgvHist.DataSource = dt;

            // resumo
            decimal total = 0; DateTime? ultima = null;
            foreach (DataRow r in dt.Rows)
            {
                total += r.Field<decimal>("TotalDoDia");
                var d = r.Field<DateTime>("Data");
                if (!ultima.HasValue || d > ultima.Value) ultima = d;
            }
            lblTotais.Text = $"Total vendido: {total:N2} Kz | Última compra: {(ultima.HasValue ? ultima.Value.ToString("dd/MM/yyyy") : "-")}";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
