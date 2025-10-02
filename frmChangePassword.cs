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
    public partial class frmChangePassword : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        public frmChangePassword()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Verificações básicas
            if (txtOldPass.Text == "" || txtNewPass.Text == "" || txtConfirmPass.Text == "")
            {
                MessageBox.Show("Por favor, preencha todos os campos!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPass.Text != txtConfirmPass.Text)
            {
                MessageBox.Show("As senhas novas não coincidem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();

                    // Verifica se a senha antiga está correta
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM tblUser WHERE username=@username AND password=@oldpass", cn);
                    checkCmd.Parameters.AddWithValue("@username", SessaoUsuario.Username);
                    checkCmd.Parameters.AddWithValue("@oldpass", txtOldPass.Text.Trim());

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count == 1)
                    {
                        // Atualiza a senha
                        SqlCommand updateCmd = new SqlCommand("UPDATE tblUser SET password=@newpass WHERE username=@username", cn);
                        updateCmd.Parameters.AddWithValue("@newpass", txtNewPass.Text.Trim());
                        updateCmd.Parameters.AddWithValue("@username", SessaoUsuario.Username);

                        updateCmd.ExecuteNonQuery();

                        MessageBox.Show("Senha alterada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close(); // Fecha o formulário após alterar
                    }
                    else
                    {
                        MessageBox.Show("Senha atual incorreta!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtOldPass.Clear();
                        txtOldPass.Focus();
                    }

                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao alterar senha: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
