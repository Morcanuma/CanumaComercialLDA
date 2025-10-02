using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lojaCanuma
{
    public partial class frmCancelDetais : Form
    {
        frmSoldItem f;
        public frmCancelDetais(frmSoldItem frm)
        {
            InitializeComponent();
            f = frm;
        }

        private void txtDesc_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Verifica se todos os campos obrigatórios estão preenchidos
                if (!string.IsNullOrEmpty(cboAction.Text) &&
                    !string.IsNullOrEmpty(txtQty.Text) &&
                    !string.IsNullOrEmpty(txtReason.Text))
                {
                    if (int.Parse(txtQty.Text) >= int.Parse(txtCancelQty.Text))
                    {
                        // Cria e exibe o formulário de anulação
                        frmVoid f = new frmVoid(this);
                        f.ShowDialog();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, exibe uma mensagem de aviso com os detalhes
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        public void RefreshList()
        {
            f.LoadRecord();
        }

        public void SetAuthorizedUser(string nome)
        {
            txtVoid.Text = nome; // campo "Autorizada por"
        }

        private void frmCancelDetais_Load(object sender, EventArgs e)
        {

        }
    }
}
