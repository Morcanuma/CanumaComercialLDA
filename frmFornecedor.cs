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
    public partial class frmFornecedor : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        private frmStore f;
        public bool isUpdate = false;

        public int FornecedorId { get; set; }  // 👈 agora pode ser acessada no btnUpdate

        public frmFornecedor(frmStore frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtFornecedor.Text == "" || txtEndereco.Text == "")
                {
                    MessageBox.Show("Por favor, preencha os campos obrigatórios (*).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cn.Open();
                cm = new SqlCommand(@"INSERT INTO tblVendor (vendedor, address, contactperson, telephone, email, fax) 
                              VALUES (@vendedor, @address, @contactperson, @telephone, @email, @fax)", cn);
                cm.Parameters.AddWithValue("@vendedor", txtFornecedor.Text);
                cm.Parameters.AddWithValue("@address", txtEndereco.Text);
                cm.Parameters.AddWithValue("@contactperson", txtContato.Text);
                cm.Parameters.AddWithValue("@telephone", txtTelefone.Text);
                cm.Parameters.AddWithValue("@email", txtEmail.Text);
                cm.Parameters.AddWithValue("@fax", txtFax.Text);

                cm.ExecuteNonQuery();
                cn.Close();

                MessageBox.Show("Fornecedor salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                f.CarregarFornecedores();
                LimparCampos();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao salvar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void LimparCampos()
        {
            txtFornecedor.Clear();
            txtEndereco.Clear();
            txtContato.Clear();
            txtTelefone.Clear();
            txtEmail.Clear();
            txtFax.Clear();
            txtFornecedor.Focus();
        }

        private void txtTelefone_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permite apenas dígitos e a tecla backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Bloqueia o caractere
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtFornecedor.Text == "" || txtEndereco.Text == "")
                {
                    MessageBox.Show("Por favor, preencha os campos obrigatórios (*).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Pega o ID do fornecedor da linha selecionada (você pode salvar isso em uma variável ao abrir o form)
                int idSelecionado = FornecedorId; // 👈 Aqui está o ID salvo quando o formulário foi aberto

                cn.Open();
                cm = new SqlCommand(@"UPDATE tblVendor SET 
                                vendedor = @vendedor, 
                                address = @address, 
                                contactperson = @contactperson, 
                                telephone = @telephone, 
                                email = @email, 
                                fax = @fax 
                              WHERE id = @id", cn);

                cm.Parameters.AddWithValue("@vendedor", txtFornecedor.Text);
                cm.Parameters.AddWithValue("@address", txtEndereco.Text);
                cm.Parameters.AddWithValue("@contactperson", txtContato.Text);
                cm.Parameters.AddWithValue("@telephone", txtTelefone.Text);
                cm.Parameters.AddWithValue("@email", txtEmail.Text);
                cm.Parameters.AddWithValue("@fax", txtFax.Text);
                cm.Parameters.AddWithValue("@id", idSelecionado);

                cm.ExecuteNonQuery();
                cn.Close();

                MessageBox.Show("Fornecedor atualizado com sucesso!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);

                f.CarregarFornecedores(); // Atualiza a lista no formulário principal
                this.Dispose(); // Fecha o formulário de edição
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao atualizar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
