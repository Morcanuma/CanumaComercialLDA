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
    public partial class fmrBrandcs : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        public fmrBrandcs()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            LoadRecords();

        }

        public void LoadRecords() // Método público que carrega os dados da tabela tblbrand para o DataGridView
        {
            int i = 0; // Inicializa um contador para exibir números sequenciais na tabela
            dataGridView1.Rows.Clear(); // Limpa todas as linhas da tabela visual (DataGridView)

            cn.Open(); // Abre a conexão com o banco de dados

            cm = new SqlCommand("select * from tblbrand order by brand", cn);
            // Cria um comando SQL que busca todos os registros da tabela tblbrand, ordenando por nome da marca (brand)

            dr = cm.ExecuteReader(); // Executa a consulta e retorna um SqlDataReader

            while (dr.Read()) // Enquanto houver registros no resultado da consulta...
            {
                i += 1; // Incrementa o contador
                dataGridView1.Rows.Add(i, dr["id"].ToString(), dr["brand"].ToString());
                // Adiciona uma nova linha ao DataGridView com os valores:
                // 1. Número sequencial (i)
                // 2. ID da marca
                // 3. Nome da marca
            }

            dr.Close(); // Fecha o leitor de dados
            cn.Close(); // Fecha a conexão com o banco de dados
        }



        private void pictureBox2_Click(object sender, EventArgs e)
        {
            frmBrandPrincipal frm = new frmBrandPrincipal(this, false);
            frm.ShowDialog();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            // Captura o nome da coluna que foi clicada pelo usuário

            if (colName == "Edit")
            // Verifica se o nome da coluna clicada é "Edit"
            // Isso geralmente vem de uma coluna tipo botão de edição

            {
                frmBrandPrincipal frm = new frmBrandPrincipal(this, true);
                // Cria uma nova instância do formulário de marca (frmBrand)
                // Passando o formulário atual como referência (para possível atualização)
               

                frm.lblD.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();

                frm.txtBrand.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                // Define o texto da caixa de texto 'txtBrand' com o valor da célula da linha 2 e coluna clicada
                frm.ShowDialog();


            }

            else if (colName == "Delete")
            {
                // Exibe uma caixa de confirmação antes de excluir o registro
                if (MessageBox.Show("Tens certeza que queres apagar este registro?",
                                    "Apagar Registro",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open(); // Abre a conexão com o banco de dados

                    // Cria o comando SQL para deletar o item selecionado com base no ID
                    cm = new SqlCommand("DELETE FROM tblbrand WHERE id LIKE '" +
                                        dataGridView1[1, e.RowIndex].Value.ToString() + "'", cn);

                    cm.ExecuteNonQuery(); // Executa o comando DELETE
                    cn.Close(); // Fecha a conexão

                    // Mostra mensagem de sucesso
                    MessageBox.Show("Marca eliminada com sucesso.",
                                    "POS",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                    LoadRecords(); // Atualiza a tabela no DataGridView
                }
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
