using System;
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
    public partial class frmLucroVenda : Form
    {
        frmLucroEOutros f;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public frmLucroVenda(frmLucroEOutros frm)
        {
            InitializeComponent();
            f = frm;
        }

        private void frmLucroVenda_Load(object sender, EventArgs e)
        {
            CarregarRelatorioLucro();
            this.reportViewer1.RefreshReport();
           
        }

        private void CarregarRelatorioLucro()
        {
            // 1. Carrega os dados da loja
            var lojaData = CarregarDadosDaLoja();

            // 2. Gera caminho do logotipo temporário
            string caminhoLogoTemp = Path.Combine(Path.GetTempPath(), "logo_temp.png");

            if (lojaData.Logo != null && lojaData.Logo.Length > 0)
            {
                File.WriteAllBytes(caminhoLogoTemp, lojaData.Logo);
            }
            else
            {
                caminhoLogoTemp = "";
            }

            string caminhoFinalLogo = !string.IsNullOrEmpty(caminhoLogoTemp)
                ? new Uri(caminhoLogoTemp).AbsoluteUri
                : "";

            // 3. Carrega dados do relatório
            var ds = new LucroVendasDataSet(); // Substitua pelo nome real do seu DataSet tipado

            using (var cn = new SqlConnection(new DBConnection().MyConnection()))
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

                // Aplica filtro de funcionário se houver
                if (!string.IsNullOrWhiteSpace(f.cbFuncionario.Text))
                {
                    sql += " AND si.funcionario = @funcionario";
                }

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@inicio", f.dtInicio.Value.Date);
                cmd.Parameters.AddWithValue("@fim", f.dtFim.Value.Date.AddDays(1).AddTicks(-1));

                if (!string.IsNullOrWhiteSpace(f.cbFuncionario.Text))
                {
                    cmd.Parameters.AddWithValue("@funcionario", f.cbFuncionario.Text);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds, "LucroVendasDataSetTable"); // Nome da tabela criada no DataSet
            }

            // 4. Define caminho do relatório .rdlc
            string reportPath = Path.Combine(Application.StartupPath, "Reports", "rptLucroVendas.rdlc");

            // 5. Parâmetros: período e funcionário
            string periodo = $"{f.dtInicio.Value:dd/MM/yyyy} a {f.dtFim.Value:dd/MM/yyyy}";
            string funcionario = string.IsNullOrWhiteSpace(f.cbFuncionario.Text) ? "Todos" : f.cbFuncionario.Text;

            // 6. Define todos os parâmetros do relatório
            var parametros = new List<ReportParameter>
                {
                    new ReportParameter("pStore", lojaData.Nome ?? ""),
                    new ReportParameter("pAddress", lojaData.Endereco ?? ""),
                    new ReportParameter("pTelefone", lojaData.Telefone ?? ""),
                    new ReportParameter("pEmail", lojaData.Email ?? ""),
                    new ReportParameter("pLogo", caminhoFinalLogo),
                    new ReportParameter("pPeriodo", periodo),
                    new ReportParameter("pFuncionario", funcionario)
                };

            // 7. Configura o ReportViewer
            reportViewer1.LocalReport.ReportPath = reportPath;
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables["LucroVendasDataSetTable"]));
            reportViewer1.LocalReport.EnableExternalImages = true;
            reportViewer1.LocalReport.SetParameters(parametros);

            // 8. Exibe o relatório
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.ZoomMode = ZoomMode.Percent;
            reportViewer1.ZoomPercent = 100;
            reportViewer1.RefreshReport();
        }



        private class StoreData
        {
            public string Nome { get; set; }
            public string Endereco { get; set; }
            public string Telefone { get; set; }
            public string Email { get; set; }
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
                    SqlCommand cmd = new SqlCommand("SELECT TOP 1 store, address, telefone, email, logo FROM tblStore", cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        loja.Nome = dr["store"]?.ToString() ?? "Canuma Comercial, LDA";
                        loja.Endereco = dr["address"]?.ToString() ?? "Endereço não informado";
                        loja.Telefone = dr["telefone"]?.ToString() ?? "Telefone não disponível";
                        loja.Email = dr["email"]?.ToString() ?? "Email não disponível";
                        loja.Logo = dr["logo"] != DBNull.Value ? (byte[])dr["logo"] : Array.Empty<byte>();
                    }
                    else
                    {
                        loja.Nome = "Canuma Comercial, LDA";
                        loja.Endereco = "Endereço não definido";
                        loja.Telefone = "000-000-000";
                        loja.Email = "sememail@canuma.ao";
                        loja.Logo = Array.Empty<byte>();
                    }

                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados da loja:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                loja.Nome = "Canuma Comercial, LDA";
                loja.Endereco = "Erro ao carregar endereço";
                loja.Telefone = "-";
                loja.Email = "-";
                loja.Logo = Array.Empty<byte>();
            }

            return loja;
        }
    }
}
