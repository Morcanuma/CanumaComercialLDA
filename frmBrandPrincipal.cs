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

namespace lojaCanuma
{
    public partial class frmBrandPrincipal : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand(); 
        DBConnection dbcon = new DBConnection();
        fmrBrandcs frmList;

        public frmBrandPrincipal(fmrBrandcs flist, bool isEdit)
        
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            frmList = flist;

            if (isEdit)
            {
                btnSave.Enabled = false;      // desativa o botão Salvar
                btnUpdate.Enabled = true;     // ativa o botão Atualizar
            }
            else
            {
                btnSave.Enabled = true;
                btnUpdate.Enabled = false;
            }
        }

        private void frmBrandPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void Clear()
        {
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtBrand.Clear();
            txtBrand.Focus();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Exibe uma caixa de diálogo perguntando se o usuário quer salvar a marca.
                if (MessageBox.Show("Tens certeza que queres salvar esta marca?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Abre a conexão com o banco de dados.
                    cn.Open();

                    // Cria o comando SQL para inserir uma nova marca na tabela "tblBrand".
                    cm = new SqlCommand("INSERT INTO tblBrand(Brand) VALUES(@brand)", cn);

                    // Adiciona o valor da caixa de texto (txtBrand) como parâmetro para o comando SQL.
                    cm.Parameters.AddWithValue("@brand", txtBrand.Text);

                    // Executa o comando de inserção no banco (sem retornar dados).
                    cm.ExecuteNonQuery();

                    // Fecha a conexão com o banco de dados.
                    cn.Close();
                    MessageBox.Show("O registro foi salvo com sucesso.");
                    Clear();
                    frmList.LoadRecords();
                }
            }
            catch (Exception ex)
            {
                // Se ocorrer algum erro, mostra uma mensagem com o erro.
                MessageBox.Show(ex.Message);
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Exibe uma caixa de confirmação para o usuário
                if (MessageBox.Show("Tens certeza que queres atualizar esta marca?",
                                    "Atualizar Registro",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open(); // Abre a conexão com o banco de dados

                    // Cria o comando SQL para atualizar o campo 'brand' da tabela tblBrand
                    cm = new SqlCommand("UPDATE tblBrand SET brand = @brand WHERE id = @id", cn);

                    // Define os parâmetros do comando SQL
                    cm.Parameters.AddWithValue("@brand", txtBrand.Text);
                    cm.Parameters.AddWithValue("@id", lblD.Text); // Pega o ID da label invisível

                    cm.ExecuteNonQuery(); // Executa o comando (sem retorno de dados)
                    cn.Close(); // Fecha a conexão com o banco de dados

                    // Mostra mensagem de sucesso
                    MessageBox.Show("Marca atualizada com sucesso.");

                    Clear(); // Limpa os campos
                    frmList.LoadRecords(); // Atualiza os registros exibidos no formulário da lista
                    this.Dispose(); // Fecha o formulário atual (de edição)
                }

            }
            catch (Exception ex)
            {
                // Mostra o erro, se houver
                MessageBox.Show(ex.Message);
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }
    }
}
