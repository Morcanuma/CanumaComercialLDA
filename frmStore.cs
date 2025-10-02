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

namespace lojaCanuma
{
    public partial class frmStore : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        
        private frmFornecedor frm;

        public frmStore(frmFornecedor f)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            CarregarFornecedores();
            frm = f;
        }
        public frmStore()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            CarregarFornecedores();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

      

        private void LimparCampos()
        {
            txtStore.Clear();
            txtAddress.Clear();
            txtTelefone.Clear();
            txtEmail.Clear();
            txtNIF.Clear();
            txtMoeda.SelectedIndex = -1;
            txtImposto.Clear();
            txtMensagemCliente.SelectedIndex = -1;
            pictureBox2.Image = null;
        }

        private void txtImposto_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir apenas números e tecla Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Bloqueia a tecla
            }
        }

        private void CarregarDadosLoja()
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM tblStore", cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        txtStore.Text = dr["store"].ToString();
                        txtAddress.Text = dr["address"].ToString();
                        txtTelefone.Text = dr["telefone"].ToString();
                        txtEmail.Text = dr["email"].ToString();
                        txtNIF.Text = dr["nif"].ToString();
                        txtMoeda.Text = dr["moeda"].ToString();
                        txtImposto.Text = dr["imposto_padrao"].ToString();
                        txtMensagemCliente.Text = dr["mensagem_cliente"]?.ToString();

                        if (dr["logo"] != DBNull.Value)
                        {
                            byte[] logoBytes = (byte[])dr["logo"];
                            using (MemoryStream ms = new MemoryStream(logoBytes))
                            {
                                pictureBox2.Image = Image.FromStream(ms);
                            }
                        }
                    }

                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os dados da loja: " + ex.Message);
            }
        }


        private void frmStore_Load(object sender, EventArgs e)
        {
            CarregarDadosLoja();
        }

    

        private void btnCarregarLogo_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Imagens (*.jpg;*.png)|*.jpg;*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image = Image.FromFile(ofd.FileName); // pictureBox onde aparece o logo
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(new DBConnection().MyConnection()))
                {
                    cn.Open();

                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM tblStore", cn);
                    int count = (int)checkCmd.ExecuteScalar();

                    SqlCommand cmd;
                    if (count == 0)
                    {
                        cmd = new SqlCommand(@"INSERT INTO tblStore 
                                            (store, address, telefone, email, nif, moeda, imposto_padrao, logo, criado_em, mensagem_cliente) 
                                            VALUES 
                                            (@store, @address, @telefone, @email, @nif, @moeda, @imposto, @logo, GETDATE(), @mensagem)", cn);
                    }
                    else
                    {
                        cmd = new SqlCommand(@"UPDATE tblStore SET 
                                    store=@store, address=@address, telefone=@telefone, 
                                    email=@email, nif=@nif, moeda=@moeda, 
                                    imposto_padrao=@imposto, logo=@logo, 
                                    mensagem_cliente=@mensagem, atualizado_em=GETDATE()", cn);
                    }

                    byte[] logoBytes = null;
                    if (pictureBox2.Image != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            pictureBox2.Image.Save(ms, pictureBox2.Image.RawFormat);
                            logoBytes = ms.ToArray();
                        }
                    }

                    cmd.Parameters.AddWithValue("@store", txtStore.Text);
                    cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@telefone", txtTelefone.Text);
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@nif", txtNIF.Text);
                    cmd.Parameters.AddWithValue("@moeda", txtMoeda.Text);
                    cmd.Parameters.AddWithValue("@imposto", decimal.TryParse(txtImposto.Text, out decimal imposto) ? imposto : 0);
                    cmd.Parameters.AddWithValue("@logo", (object)logoBytes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mensagem", txtMensagemCliente.Text);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Dados salvos com sucesso!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LimparCampos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            LimparCampos();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            frmFornecedor frm = new frmFornecedor(this);
            frm.ShowDialog();
        }


        public void CarregarFornecedores()
        {
            dtgvFornecedor.Rows.Clear(); // Limpa a tabela

            try
            {
                cn.Open();
                string sql = "SELECT * FROM tblVendor ORDER BY id DESC";
                cm = new SqlCommand(sql, cn);
                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    dtgvFornecedor.Rows.Add(
                        dr["id"].ToString(),
                        dr["vendedor"].ToString(),
                        dr["address"].ToString(),
                        dr["contactperson"].ToString(),
                        dr["telephone"].ToString(),
                        dr["email"].ToString(),
                        dr["fax"].ToString()
                    );
                }

                dr.Close();
                cn.Close();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao carregar fornecedores: " + ex.Message);
            }
        }

        

        private void dtgvFornecedor_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dtgvFornecedor.Columns[e.ColumnIndex].Name;

            if (colName == "Atualizar")
            {
                // Abre o formulário de edição de fornecedor
                frmFornecedor frm = new frmFornecedor(this);
                frm.isUpdate = true; // Define modo de atualização
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;

                frm.FornecedorId = Convert.ToInt32(dtgvFornecedor.Rows[e.RowIndex].Cells[0].Value);


                // Preenche os campos do formulário com os dados da linha selecionada
                frm.txtFornecedor.Text = dtgvFornecedor.Rows[e.RowIndex].Cells[1].Value.ToString(); // FORNECEDOR
                frm.txtEndereco.Text = dtgvFornecedor.Rows[e.RowIndex].Cells[2].Value.ToString();   // ENDEREÇO
                frm.txtContato.Text = dtgvFornecedor.Rows[e.RowIndex].Cells[3].Value.ToString();    // CONTACT PERSON
                frm.txtTelefone.Text = dtgvFornecedor.Rows[e.RowIndex].Cells[4].Value.ToString();   // TELEFONE
                frm.txtEmail.Text = dtgvFornecedor.Rows[e.RowIndex].Cells[5].Value.ToString();      // EMAIL
                frm.txtFax.Text = dtgvFornecedor.Rows[e.RowIndex].Cells[6].Value.ToString();        // FAX

                frm.ShowDialog();
            }
            else if (colName == "Apagar")
            {
                // Confirmação antes de excluir
                if (MessageBox.Show("Tens certeza que queres eliminar este fornecedor?",
                                    "Eliminar Fornecedor",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open();
                    cm = new SqlCommand("DELETE FROM tblVendor WHERE id = @id", cn);
                    cm.Parameters.AddWithValue("@id", dtgvFornecedor.Rows[e.RowIndex].Cells[0].Value.ToString());
                    cm.ExecuteNonQuery();
                    cn.Close();

                    MessageBox.Show("Fornecedor eliminado com sucesso!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CarregarFornecedores(); // Recarrega a lista
                }
            }
        }
    }
}
