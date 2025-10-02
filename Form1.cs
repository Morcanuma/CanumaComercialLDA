using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Tulpep.NotificationWindow;



namespace lojaCanuma
{
    public partial class Form1 : Form

    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand(); 
        DBConnection dbcon = new DBConnection();
        string nomeLogado = SessaoUsuario.Nome;
        string cargoLogado = SessaoUsuario.Cargo;
        SqlDataReader dr;

        public Form1()
        {

            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            
            //cn.Open();
        }

        // ====== IMPORTAÇÃO PARA ARRASTAR A JANELA ======
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0); // Faz o formulário se mover
        }

        // =================================================




        private void AbrirNoPanel(Form frm)
        {
            panel3.Controls.Clear(); // Limpa visualmente

            if (frm != null)
            {
                frm.TopLevel = false;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.Dock = DockStyle.Fill;
                panel3.Controls.Add(frm);
                frm.Show();
            }
            else
            {
                frmDashboard dash = new frmDashboard();
                dash.TopLevel = false;
                dash.FormBorderStyle = FormBorderStyle.None;
                dash.Dock = DockStyle.Fill;
                panel3.Controls.Add(dash);
                dash.Show();
            }
        }




        public void NotifyCriticalItems()
        {
            try
            {
                // 🔸 Consulta 1: Total de itens críticos
                string totalCriticos;
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM vwCriticalItems", cn))
                {
                    cn.Open();
                    totalCriticos = cmd.ExecuteScalar()?.ToString() ?? "0";
                    cn.Close();
                }

                // 🔸 Consulta 2: Lista dos itens críticos
                StringBuilder listaCriticos = new StringBuilder();
                int contador = 0;

                cn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM vwCriticalItems", cn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contador++;
                        string produto = reader["pdesc"].ToString();
                        string codigo = reader["pcode"].ToString();
                        string estoque = reader["qty"].ToString();

                        listaCriticos.AppendLine($"{contador}. {produto} ({codigo}) - Estoque: {estoque}");
                    }
                }
                cn.Close();

                // 🔊 Toca som de aviso (Windows beep)
                System.Media.SystemSounds.Exclamation.Play();

                // 🔔 Exibe popup persistente com botão de fechar
                PopupNotifier popup = new PopupNotifier
                {
                    Image = Properties.Resources.icons8_critical_64, // ícone de alerta
                    TitleText = $"⚠ {totalCriticos} ITENS CRÍTICOS ENCONTRADOS",
                    ContentText = listaCriticos.ToString(),

                    // Estilo do popup
                    TitleColor = Color.DarkRed,
                    ContentColor = Color.Black,
                    TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                    ContentFont = new Font("Segoe UI", 9),
                    BodyColor = Color.WhiteSmoke,
                    BorderColor = Color.DarkRed,

                    // ⚠️ Mantém o popup aberto até o usuário clicar
                    Delay = int.MaxValue,
                    ShowCloseButton = true,

                    // Animação suave
                    AnimationInterval = 10,
                    AnimationDuration = 500
                };

                popup.Popup(); // Exibe
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao gerar notificação de estoque crítico:\n" + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void btnBrand_Click(object sender, EventArgs e)
        {
            
            AbrirNoPanel(new fmrBrandcs());


        }

        private void button9_Click(object sender, EventArgs e)
        {
            
            this.Close();
            frmLogin login = new frmLogin();
            login.Show();

        }

        private void button3_Click(object sender, EventArgs e)
        {

            AbrirNoPanel(new fmrCategoryListcs());

        }

        private void panel1_MouseDown_1(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            AbrirNoPanel(new frmProductListcs());

        }

        private void button6_Click(object sender, EventArgs e)
        {
            AbrirNoPanel(new frmStockIn());

        }



        private void button7_Click(object sender, EventArgs e)
        {

            AbrirNoPanel(new frmUserAccount());


        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblName.Text = SessaoUsuario.Nome;
            lblRole.Text = SessaoUsuario.Cargo;

            AbrirNoPanel(null); // Carrega o dashboard

            NotifyCriticalItems();

        }

        private void btnhVenda_Click(object sender, EventArgs e)
        {
            frmPOS posForm = new frmPOS();      // Cria o POS
            frmSoldItem frm = new frmSoldItem(posForm);  // Passa o POS como argumento
            frm.ShowDialog();


        }


        private void button8_Click(object sender, EventArgs e)
        {

            AbrirNoPanel(new frmStore());

        }

        private void button10_Click(object sender, EventArgs e)
        {
            frmRecords frm = new frmRecords();
            frm.LoadCriticalItems();
            frm.LoadStockInHistory();
            frm.CancelledOrders();
            frm.LoadInventory();

            AbrirNoPanel(frm);





        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            AbrirNoPanel(new frmClientesAnalise());


        }

        private void button1_Click(object sender, EventArgs e)
        {

            AbrirNoPanel(null);

        }

        private void button5_Click(object sender, EventArgs e)
        {
           frmAjustement frm = new frmAjustement(this); 
            frm.LoadRecords();
            frm.txtUsuario.Text = lblName.Text;
            frm.ShowDialog();
        }

        private void btnLucro_Click(object sender, EventArgs e)
        {
            AbrirNoPanel( new frmLucroEOutros());
        }

        private void button11_Click(object sender, EventArgs e)
        {
            AbrirNoPanel(new fmrCategoryListcs());
        }
    }
}
