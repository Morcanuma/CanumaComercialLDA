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
    public partial class frmProduct : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        SqlDataReader dr;
        frmProductListcs flist;
        public bool isUpdate = false; // ← Indica se o formulário está em modo de atualização

        public frmProduct(frmProductListcs frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            flist = frm;
        }

        public void LoadCategory() // Método que carrega as categorias no ComboBox
        {
            cbxCategory.Items.Clear(); // Limpa todos os itens existentes no ComboBox para evitar duplicações

            cn.Open(); // Abre a conexão com o banco de dados

            // Cria um comando SQL para selecionar todos os nomes das categorias da tabela 'tblcategory'
            cm = new SqlCommand("select category from tblcategory", cn);

            // Executa o comando e retorna os resultados em um SqlDataReader
            dr = cm.ExecuteReader();

            // Percorre cada linha retornada pela consulta
            while (dr.Read())
            {
                // Adiciona o valor da primeira coluna (índice 0) como item no ComboBox
                cbxCategory.Items.Add(dr[0].ToString());
            }

            dr.Close(); // Fecha o leitor de dados (SqlDataReader)
            cn.Close(); // Fecha a conexão com o banco de dados
        }

        public void LoadBrand() // Método que carrega as categorias no ComboBox
        {
            cbxBrand.Items.Clear(); // Limpa todos os itens existentes no ComboBox para evitar duplicações

            cn.Open(); // Abre a conexão com o banco de dados

            // Cria um comando SQL para selecionar todos os nomes das categorias da tabela 'tblcategory'
            cm = new SqlCommand("select brand from tblbrand", cn);

            // Executa o comando e retorna os resultados em um SqlDataReader
            dr = cm.ExecuteReader();

            // Percorre cada linha retornada pela consulta
            while (dr.Read())
            {
                // Adiciona o valor da primeira coluna (índice 0) como item no ComboBox
                cbxBrand.Items.Add(dr[0].ToString());
            }

            dr.Close(); // Fecha o leitor de dados (SqlDataReader)
            cn.Close(); // Fecha a conexão com o banco de dados
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void frmProduct_Load(object sender, EventArgs e)
        {
            if (!isUpdate)
            {
                GenerateProductCodeAndBarcode(); // Só gera se for inserção
            }

            LoadCategory();
            LoadBrand();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Exibe pergunta ao usuário
                if (MessageBox.Show("Tens certeza que queres salvar este produto?", "Salvar Produto", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string bid = ""; string cid = ""; // Variáveis para armazenar os IDs da marca e da categoria

                    // Buscar ID da marca no banco de dados
                    cn.Open();
                    cm = new SqlCommand("SELECT id FROM tblBrand WHERE brand LIKE '" + cbxBrand.Text + "'", cn);
                    dr = cm.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) { bid = dr[0].ToString(); }
                    dr.Close();
                    cn.Close();

                    // Buscar ID da categoria no banco de dados
                    cn.Open();
                    cm = new SqlCommand("SELECT id FROM tblCategory WHERE category LIKE '" + cbxCategory.Text + "'", cn);
                    dr = cm.ExecuteReader();
                    dr.Read();
                    if (dr.HasRows) { cid = dr[0].ToString(); }
                    dr.Close();
                    cn.Close();

                    // Inserir novo produto na tabela de produtos
                    cn.Open();
                    cm = new SqlCommand(@"INSERT INTO tblProduct (pcode, barcode, pdesc, bid, cid, price, qty, reorder, cost_price)
                      VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @qty, @reorder, @cost_price)", cn);

                    cm.Parameters.AddWithValue("@pcode", txtProdCode.Text);
                    cm.Parameters.AddWithValue("@barcode", txtBarcode.Text);
                    cm.Parameters.AddWithValue("@pdesc", txtDescription.Text);
                    cm.Parameters.AddWithValue("@bid", bid);
                    cm.Parameters.AddWithValue("@cid", cid);
                    cm.Parameters.AddWithValue("@price", double.Parse(txtPreco.Text));
                    cm.Parameters.AddWithValue("@reorder", int.Parse(txtReorder.Text));
                    cm.Parameters.AddWithValue("@cost_price", Convert.ToDecimal(txtPrecoCompra.Text));
                    cm.Parameters.AddWithValue("@qty", int.Parse("0")); // ou txtQty.Text se tiver o campo visível


                    cm.ExecuteNonQuery();
                    cn.Close();

                    // Mensagem de confirmação
                    MessageBox.Show("Produto salvo com sucesso.");
                    Clear();
                    flist.LoadRecords();
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message);
            }

        }


        public void Clear()
        {
            txtPreco.Clear();              // Limpa o campo de preço
            txtDescription.Clear();        // Limpa o campo de descrição
            txtProdCode.Clear();           // Limpa o campo de código do produto
            cbxBrand.Text = "";            // Limpa a seleção da marca
            cbxCategory.Text = "";
            txtBarcode.Text = "";// Limpa a seleção da categoria
            txtProdCode.Focus();
            txtPrecoCompra.Clear();
            // Foca no campo de código do produto
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtReorder.Text = string.Empty;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrEmpty(txtProdCode.Text))
                {
                    MessageBox.Show("Selecione um produto para atualizar", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Confirmar atualização deste produto?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string bid = "", cid = "";

                    // Busca ID da marca
                    cn.Open();
                    cm = new SqlCommand("SELECT id FROM tblBrand WHERE brand = @brand", cn);
                    cm.Parameters.AddWithValue("@brand", cbxBrand.Text);
                    dr = cm.ExecuteReader();
                    if (dr.Read()) bid = dr["id"].ToString();
                    dr.Close();
                    cn.Close();

                    // Busca ID da categoria
                    cn.Open();
                    cm = new SqlCommand("SELECT id FROM tblCategory WHERE category = @category", cn);
                    cm.Parameters.AddWithValue("@category", cbxCategory.Text);
                    dr = cm.ExecuteReader();
                    if (dr.Read()) cid = dr["id"].ToString();
                    dr.Close();
                    cn.Close();

                    // Atualização com parâmetros seguros
                    cn.Open();
                    cm = new SqlCommand(@"UPDATE tblProduct SET barcode = @barcode, pdesc = @pdesc, bid = @bid, cid = @cid,
                      price = @price, qty = @qty, reorder = @reorder, cost_price = @cost_price
                      WHERE pcode = @pcode", cn);

                    cm.Parameters.AddWithValue("@pcode", txtProdCode.Text);
                    cm.Parameters.AddWithValue("@barcode", txtBarcode.Text);
                    cm.Parameters.AddWithValue("@pdesc", txtDescription.Text);
                    cm.Parameters.AddWithValue("@bid", bid);
                    cm.Parameters.AddWithValue("@cid", cid);
                    cm.Parameters.AddWithValue("@price", double.Parse(txtPreco.Text)); // Conversão explícita
                    cm.Parameters.AddWithValue("@reorder", int.Parse(txtReorder.Text));
                    cm.Parameters.AddWithValue("@cost_price", Convert.ToDecimal(txtPrecoCompra.Text));
                    cm.Parameters.AddWithValue("@qty", 0); // Adicione esta linha com valor padrão ou txtQt

                    int rowsAffected = cm.ExecuteNonQuery();
                    cn.Close();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Produto atualizado com sucesso!");
                        flist.LoadRecords();
                        this.Dispose();
                    }
                    else
                    {
                        MessageBox.Show("Nenhum registro foi atualizado. Verifique o código do produto.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (cn.State == ConnectionState.Open) cn.Close();
                MessageBox.Show($"Erro ao atualizar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Clear();   
        }

        private void txtPreco_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 46)
            {
                // Aceita o caractere ponto "." (ASCII 46)
            }
            else if (e.KeyChar == 8)
            {
                // Aceita a tecla Backspace (ASCII 8)
            }
            else if ((e.KeyChar < 48) || (e.KeyChar > 57)) // ASCII entre 48 e 57 representa os números de 0 a 9
            {
                e.Handled = true; // Bloqueia qualquer outro caractere
            }


        }

        private void GenerateProductCodeAndBarcode()
        {
            try
            {
                cn.Open(); // Abre a conexão com o banco de dados

                // Seleciona o último código de produto (pcode) inserido, ordenando do maior para o menor
                cm = new SqlCommand("SELECT TOP 1 pcode FROM tblProduct ORDER BY pcode DESC", cn);
                dr = cm.ExecuteReader();

                string lastCode = ""; // Variável que armazenará o último código encontrado
                if (dr.Read())
                {
                    lastCode = dr["pcode"].ToString(); // Lê o valor do pcode
                }
                dr.Close(); // Fecha o leitor
                cn.Close(); // Fecha a conexão

                int nextId = 1; // Valor padrão caso não haja nenhum código ainda

                // Se encontrou um código anterior e começa com "PROD", extrai a parte numérica
                if (!string.IsNullOrEmpty(lastCode) && lastCode.StartsWith("PROD"))
                {
                    string numberPart = lastCode.Substring(4); // Ex: de "PROD000123" pega "000123"
                    int.TryParse(numberPart, out nextId); // Converte a parte numérica em número
                    nextId++; // Incrementa para gerar o próximo código
                }

                // Formata o novo código como "PROD000124" por exemplo
                string newPCode = "PROD" + nextId.ToString("D6");

                // Gera um código de barras começando de 1000000000001 em diante
                string newBarcode = (1000000000000 + nextId).ToString();

                // Preenche os campos no formulário
                txtProdCode.Text = newPCode;
                txtBarcode.Text = newBarcode;
            }
            catch (Exception ex)
            {
                // Fecha a conexão em caso de erro e mostra a mensagem
                if (cn.State == ConnectionState.Open) cn.Close();
                MessageBox.Show("Erro ao gerar código automático: " + ex.Message);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 46)
            {
                // Aceita o caractere ponto "." (ASCII 46)
            }
            else if (e.KeyChar == 8)
            {
                // Aceita a tecla Backspace (ASCII 8)
            }
            else if ((e.KeyChar < 48) || (e.KeyChar > 57)) // ASCII entre 48 e 57 representa os números de 0 a 9
            {
                e.Handled = true; // Bloqueia qualquer outro caractere
            }
        }
    }
}
