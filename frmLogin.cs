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
    public partial class frmLogin : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public frmLogin()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
    
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUser.Text == "" || txtPass.Text == "")
            {
                MessageBox.Show("Por favor, preencha o nome de usuário e a senha!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                cn.Open();
                cm = new SqlCommand("SELECT * FROM tblUser WHERE username=@username AND password=@password", cn);
                cm.Parameters.AddWithValue("@username", txtUser.Text.Trim());
                cm.Parameters.AddWithValue("@password", txtPass.Text.Trim());
                dr = cm.ExecuteReader();

                if (dr.Read())
                {
                    // Verifica se a conta está ativa
                    bool ativo = dr["ativo"] != DBNull.Value && Convert.ToBoolean(dr["ativo"]);
                    if (!ativo)
                    {
                        MessageBox.Show("Esta conta está desativada.\n\nFale com o administrador da Canuma Business Manager para reativá-la.",
                                        "Conta Desativada",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                        txtPass.Clear();
                        txtPass.Focus();
                        dr.Close();
                        cn.Close();
                        return;
                    }

                    // ✅ Conta ativa: salva dados na sessão
                    SessaoUsuario.Nome = dr["name"].ToString();
                    SessaoUsuario.Cargo = dr["role"].ToString();
                    SessaoUsuario.Username = dr["username"].ToString();

                    MessageBox.Show("Bem-vindo, " + SessaoUsuario.Nome + "!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtPass.Clear();
                    txtUser.Clear();
                    this.Hide();

                    // Redireciona para a tela correspondente
                    if (SessaoUsuario.Cargo == "Administrador do Sistema")
                    {
                        Form1 frm = new Form1();
                        frm.ShowDialog();
                    }
                    else if (SessaoUsuario.Cargo == "Operador de Caixa")
                    {
                        frmPOS frm = new frmPOS();
                        frm.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("Usuário ou senha inválidos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPass.Clear();
                    txtPass.Focus();
                }

                dr.Close();
                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }




        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
