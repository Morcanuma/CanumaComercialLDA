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
    public partial class frmCustomer : Form
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        public frmCustomer()
        {
            InitializeComponent();
        
        }

       


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmCustomer_Load(object sender, EventArgs e)
        {
            
        }





        private void btnSave_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = textBox1.Text.Trim();

            // 1) Validação mínima
            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("O nome é obrigatório.", "Atenção",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            // 2) Verificar se já existe cliente com esse nome
            bool existe = _repo.GetAll()
                              .Any(c => c.Name.Equals(nome,
                                     StringComparison.OrdinalIgnoreCase));
            if (existe)
            {
                MessageBox.Show(
                    "Já existe um cliente cadastrado com este nome.",
                    "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            // 3) Criar DTO e salvar no banco
            var novo = new CustomerModels
            {
                Name = nome,
                Phone = phone,
                Email = email
            };

            try
            {
                int idGerado = _repo.Add(novo);

                MessageBox.Show(
                    $"Cliente \"{nome}\" (ID={idGerado}) cadastrado com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 4) Limpar campos (opcional, porque vamos fechar logo em seguida)
                txtNome.Clear();
                txtPhone.Clear();
                textBox1.Clear();

                // 5) Fechar o formulário
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao salvar cliente:\n" + ex.Message,
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
