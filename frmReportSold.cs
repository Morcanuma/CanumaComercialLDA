
using System;
using System.Collections;
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
using Microsoft.Reporting.WinForms;

namespace lojaCanuma
{
    public partial class frmReportSold : Form
    {

        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        frmSoldItem f;
        public frmReportSold(frmSoldItem frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void frmReportSold_Load(object sender, EventArgs e)
        {


        }

        public void LoadReport()
        {
            try
            {
                // 🔹 Carrega os dados da loja
                var loja = CarregarDadosDaLoja();

                // 🔹 Gera imagem temporária do logotipo
                if (loja.Logo == null || loja.Logo.Length == 0)
                    throw new Exception("Logotipo da loja não encontrado.");
                string tempLogoPath = Path.Combine(Path.GetTempPath(), "logo_temp_sold.png");
                File.WriteAllBytes(tempLogoPath, loja.Logo);

                // 🔹 Caminho do relatório RDLC
                string reportPath = Path.Combine(Application.StartupPath, "Reports", "ReportSoldItem.rdlc");
                this.reportViewer1.LocalReport.ReportPath = reportPath;
                this.reportViewer1.LocalReport.DataSources.Clear();

                // 🔹 Monta o dataset
                DataSet1 ds = new DataSet1();
                SqlDataAdapter da = new SqlDataAdapter();
                cn.Open();

                // 🔹 Consulta base com filtro por datas
                string query = @"
            SELECT 
                c.id, c.transno, c.pcode, p.pdesc, 
                c.price, c.qty, c.disc AS discount, 
                c.total, c.sdate 
            FROM tblCar AS c
            INNER JOIN tblProduct AS p ON c.pcode = p.pcode
            WHERE c.status = 'Sold' 
              AND c.sdate BETWEEN @date1 AND @date2";

                // 🔹 Filtro opcional por funcionário
                bool filtrarFuncionario = f.cboFuncionario.Text.Trim().ToLower() != "todos";
                if (filtrarFuncionario)
                    query += " AND c.funcionario LIKE @funcionario";

                // 🔹 Parâmetros para a consulta
                da.SelectCommand = new SqlCommand(query, cn);
                da.SelectCommand.Parameters.AddWithValue("@date1", f.dt1.Value.Date);
                da.SelectCommand.Parameters.AddWithValue("@date2", f.dt2.Value.Date.AddDays(1).AddTicks(-1));
                if (filtrarFuncionario)
                    da.SelectCommand.Parameters.AddWithValue("@funcionario", "%" + f.cboFuncionario.Text.Trim() + "%");

                // 🔹 Executa e preenche os dados
                da.Fill(ds.Tables["dtSoldReport"]);
                cn.Close();

                // 🔹 Fonte de dados para o ReportViewer
                var rptDS = new ReportDataSource("DataSet1", ds.Tables["dtSoldReport"]);
                reportViewer1.LocalReport.DataSources.Add(rptDS);

                // 🔹 Parâmetros visuais esperados no RDLC
                string nomeLoja = loja.Nome ?? "Canuma Comercial, LDA";
                
                string periodo = $"{f.dt1.Value:dd/MM/yyyy} a {f.dt2.Value:dd/MM/yyyy}";
                string funcionario = f.cboFuncionario.Text == "Todos" ? "Todos os funcionários" : f.cboFuncionario.Text;

                ReportParameter[] rptParams = new ReportParameter[]
                      {
                        new ReportParameter("pStore", loja.Nome ?? "Canuma Comercial, LDA"),
                        new ReportParameter("pDate", f.dt1.Value.ToShortDateString() + " a " + f.dt2.Value.ToShortDateString()),
                        new ReportParameter("pHeader", "RELATÓRIO DE VENDAS"),
                        new ReportParameter("pFuncionario", f.cboFuncionario.Text == "Todos" ? "Todos os funcionários" : f.cboFuncionario.Text),
                        new ReportParameter("pLogo", new Uri(tempLogoPath).AbsoluteUri)
                      };


                reportViewer1.LocalReport.EnableExternalImages = true;
                reportViewer1.LocalReport.SetParameters(rptParams);

                // 🔹 Modo de visualização
                reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.Percent;
                reportViewer1.ZoomPercent = 100;
                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(
                    "Erro ao gerar relatório:\n\n" + ex.Message +
                    "\n\nVerifique se todos os parâmetros esperados no RDLC existem: pStoreName, pAddress, pDate, pFuncionario, pHeader, pLogo.",
                    "Erro no Relatório", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
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







    }
}
