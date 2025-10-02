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
    public partial class frmUserAccount : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public frmUserAccount()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void frmUserAccount_Resize(object sender, EventArgs e)
        {
            metroTabControl1.Left = (this.Width - metroTabControl1.Width) / 2;
            metroTabControl1.Top = (this.Height - metroTabControl1.Height) / 2;
        }

        private void frmUserAccount_Load(object sender, EventArgs e)
        {
            txtUserChange.Text = SessaoUsuario.Username;
            txtUserChange.Enabled = false; // torna o campo não editável
            LoadUsuarios();

            dgvUsers.CellClick += dgvUsers_CellClick;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validação de campos obrigatórios
            if (txtUser.Text == "" || txtPass.Text == "" || txtRetype.Text == "" || cbRole.Text == "" || txtName.Text == "")
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Campos obrigatórios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verifica se as senhas coincidem
            if (txtPass.Text != txtRetype.Text)
            {
                MessageBox.Show("As senhas inseridas não coincidem. Verifique e tente novamente.", "Validação de Senha", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Abre a conexão
                cn.Open();

                // Verifica se o nome de usuário já existe
                string checkUser = "SELECT COUNT(*) FROM tblUser WHERE username = @username";
                cm = new SqlCommand(checkUser, cn);
                cm.Parameters.AddWithValue("@username", txtUser.Text.Trim());

                int count = Convert.ToInt32(cm.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Este nome de usuário já está em uso. Por favor, escolha outro.", "Usuário Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Comando SQL de inserção
                string sql = "INSERT INTO tblUser (username, password, role, name) VALUES (@username, @password, @role, @name)";
                cm = new SqlCommand(sql, cn);
                cm.Parameters.AddWithValue("@username", txtUser.Text.Trim());
                cm.Parameters.AddWithValue("@password", txtPass.Text.Trim());
                cm.Parameters.AddWithValue("@role", cbRole.Text);
                cm.Parameters.AddWithValue("@name", txtName.Text.Trim());

                cm.ExecuteNonQuery();
                MessageBox.Show("Conta de usuário registrada com sucesso.", "Operação Concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Limpa os campos
                txtName.Clear();
                txtPass.Clear();
                txtRetype.Clear();
                txtUser.Clear();
                cbRole.SelectedIndex = -1;
                txtUser.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao registrar o usuário. Detalhes do erro: " + ex.Message, "Erro ao salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }




        public void Clear()
        {
            txtName.Clear();
            txtPass.Clear();
            txtRetype.Clear();
            txtUser .Clear();
            txtRetype .Clear();
            cbRole.SelectedIndex = -1;
            txtUser.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            // Verificações
            if (txtOldPass.Text == "" || txtNewPass.Text == "" || txtConfirmNew.Text == "")
            {
                MessageBox.Show("Preencha todos os campos!", "Campos obrigatórios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPass.Text != txtConfirmNew.Text)
            {
                MessageBox.Show("A nova senha e a confirmação não coincidem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                cn.Open();

                // Verifica se a senha atual está correta
                string query = "SELECT COUNT(*) FROM tblUser WHERE username = @user AND password = @pass";
                SqlCommand cmCheck = new SqlCommand(query, cn);
                cmCheck.Parameters.AddWithValue("@user", SessaoUsuario.Username);
                cmCheck.Parameters.AddWithValue("@pass", txtOldPass.Text.Trim());

                int count = Convert.ToInt32(cmCheck.ExecuteScalar());

                if (count == 0)
                {
                    MessageBox.Show("Palavra-passe atual incorreta!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Atualiza a nova senha
                string update = "UPDATE tblUser SET password = @newpass WHERE username = @user";
                SqlCommand cmUpdate = new SqlCommand(update, cn);
                cmUpdate.Parameters.AddWithValue("@newpass", txtNewPass.Text.Trim());
                cmUpdate.Parameters.AddWithValue("@user", SessaoUsuario.Username);

                cmUpdate.ExecuteNonQuery();

                MessageBox.Show("Palavra-passe atualizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Limpa os campos
                txtOldPass.Clear();
                txtNewPass.Clear();
                txtConfirmNew.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao trocar palavra-passe: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }


        private void LoadUsuarios()
        {
            try
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close(); // ← Fecha antes de abrir novamente

                dgvUsers.Rows.Clear();
                cn.Open(); // Agora sim pode abrir
                SqlCommand cm = new SqlCommand("SELECT username, name, role, ativo FROM tblUser", cn);
                SqlDataReader dr = cm.ExecuteReader();
                while (dr.Read())
                {
                    bool ativo = dr["ativo"] != DBNull.Value && (bool)dr["ativo"];
                    dgvUsers.Rows.Add(
                        dr["username"].ToString(),
                        dr["name"].ToString(),
                        dr["role"].ToString(),
                        ativo ? "Ativo" : "Desativado"
                    );
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar usuários: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecione um usuário.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string username = dgvUsers.SelectedRows[0].Cells[0].Value.ToString();
            string statusAtual = dgvUsers.SelectedRows[0].Cells[3].Value.ToString(); // do grid
            string statusDoLabel = lblStatus.Text.Replace("Status: ", "");

            // Verifica se bate com o label
            if (statusAtual != statusDoLabel)
            {
                MessageBox.Show("Selecione novamente o usuário. Os dados estão inconsistentes.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool novoEstado = statusDoLabel == "Ativo" ? false : true;

            try
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();

                cn.Open();

                SqlCommand cm = new SqlCommand("UPDATE tblUser SET ativo = @ativo WHERE username = @username", cn);
                cm.Parameters.AddWithValue("@ativo", novoEstado);
                cm.Parameters.AddWithValue("@username", username);
                cm.ExecuteNonQuery();

                MessageBox.Show($"Usuário \"{username}\" foi {(novoEstado ? "ativado" : "desativado")} com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadUsuarios(); // Atualiza o grid
                lblStatus.Text = "Status: " + (novoEstado ? "Ativo" : "Desativado");
                lblStatus.ForeColor = novoEstado ? Color.Green : Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar estado da conta: " + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }

        private void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string status = dgvUsers.Rows[e.RowIndex].Cells[3].Value.ToString();
                lblStatus.Text = "Status: " + status;

                // Opcional: mudar cor
                lblStatus.ForeColor = status == "Ativo" ? Color.Green : Color.Red;
            }
        }
    }
}
