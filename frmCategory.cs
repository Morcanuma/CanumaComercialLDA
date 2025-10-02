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
using System.Collections;

namespace lojaCanuma
{
    public partial class frmCategory : Form
    {


        SqlConnection cn; // ✅ correto
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        fmrCategoryListcs flist;

        // construtor “padrão” (sem parent)
        public frmCategory()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            flist = null;
        }
        public frmCategory(fmrCategoryListcs frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            flist = frm;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void Clear()
        {
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtCategory.Clear();
            txtCategory.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Tens certeza que queres salvar esta categoria?", "Salvar Registro", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open();
                    cm = new SqlCommand("INSERT INTO tblCategory(category) VALUES(@category)", cn);
                    cm.Parameters.AddWithValue("@category", txtCategory.Text);
                    cm.ExecuteNonQuery();
                    cn.Close();

                    MessageBox.Show("Categoria salva com sucesso.");
                    Clear();
                    flist.LoadCategory();
                }
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro: " + ex.Message);
            }


        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Pergunta ao usuário se ele tem certeza que deseja atualizar a categoria
                if (MessageBox.Show("Tens certeza que queres atualizar esta categoria?", "Atualizar Categoria", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open(); // Abre a conexão com o banco de dados

                    // Comando SQL para atualizar a categoria com base no ID
                    cm = new SqlCommand("UPDATE tblCategory SET category = @category WHERE id LIKE '" + lblD.Text + "'", cn);

                    // Adiciona o valor digitado no campo de texto como parâmetro @category
                    cm.Parameters.AddWithValue("@category", txtCategory.Text);

                    // Executa o comando de atualização
                    cm.ExecuteNonQuery();

                    cn.Close(); // Fecha a conexão

                    // Mensagem de confirmação
                    MessageBox.Show("Categoria atualizada com sucesso!");

                    flist.LoadCategory(); // Recarrega a lista de categorias
                    this.Dispose(); // Fecha o formulário atual
                }
            }
            catch (Exception ex)
            {
                cn.Close(); // Fecha a conexão caso ocorra erro
                MessageBox.Show(ex.Message); // Exibe o erro
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void frmCategory_Load(object sender, EventArgs e)
        {

        }
    }
}
