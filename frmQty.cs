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
    public partial class frmQty : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        string stitle = "Sistema de Vendas(morcanuma)";
        frmPOS fpos;
        private String pcode;
        private double price;
        private int qty;
        private String transno;
        public frmQty(frmPOS frmpos)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            fpos = frmpos;
        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmQty_Load(object sender, EventArgs e)
        {

        }




        public void ProductDetails(String pcode, double price, String transno, int qty)
        {
            this.pcode = pcode;
            this.price = price;
            this.transno = transno;
            this.qty = qty;
        }

        private void txtQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verifica se a tecla pressionada foi Enter (char 13) e se o campo de quantidade não está vazio
            if (e.KeyChar == (char)Keys.Enter && !string.IsNullOrWhiteSpace(txtQty.Text))
            {
                try
                {
                    String id = "";
                    int cart_qty = 0;
                    bool found = false;

                    // Abre a conexão com o banco de dados
                    if (cn.State == ConnectionState.Closed)
                        cn.Open();

                    // Verifica se o item já existe no carrinho
                    cm = new SqlCommand("SELECT * FROM tblCar WHERE transno = @transno AND pcode = @pcode", cn);
                    cm.Parameters.AddWithValue("@transno", fpos.lblTransno.Text); // Número da transação
                    cm.Parameters.AddWithValue("@pcode", pcode); // Código do produto

                    dr = cm.ExecuteReader();
                    if (dr.Read())
                    {
                        found = true;
                        id = dr["id"].ToString(); // ID da linha no carrinho
                        cart_qty = int.Parse(dr["qty"].ToString()); // Quantidade já no carrinho
                    }
                    dr.Close();
                    cn.Close();

                    int novaQtd = int.Parse(txtQty.Text); // Quantidade sendo adicionada
                    int totalSolicitado = novaQtd + cart_qty;

                    // Validação se a soma da nova quantidade com a que já está no carrinho ultrapassa o estoque
                    if (totalSolicitado > qty)
                    {
                        MessageBox.Show(
                                         $"Não foi possível adicionar essa quantidade.\n\n" +
                                         $"Você já tem {cart_qty} unidade(s) no carrinho.\n" +
                                         $"Com mais {novaQtd}, seriam {totalSolicitado}, mas o estoque atual é de apenas {qty}.\n" +
                                         $"Por favor, ajuste a quantidade.",
                                         "Estoque Limitado",
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Exclamation
                                     );

                        return;
                    }

                    if (found)
                    {
                        // Atualiza a quantidade no carrinho
                        if (cn.State == ConnectionState.Closed)
                            cn.Open();

                        cm = new SqlCommand("UPDATE tblCar SET qty = qty + @qty WHERE id = @id", cn);
                        cm.Parameters.AddWithValue("@qty", novaQtd);
                        cm.Parameters.AddWithValue("@id", id);
                        cm.ExecuteNonQuery();
                    }
                    else
                    {
                        // Insere novo item no carrinho
                        if (cn.State == ConnectionState.Closed)
                            cn.Open();

                        cm = new SqlCommand("INSERT INTO tblCar (transno, pcode, price, qty, sdate, disc, total, funcionario) " +
                                            "VALUES (@transno, @pcode, @price, @qty, @sdate, @disc, @total, @funcionario)", cn);

                        cm.Parameters.AddWithValue("@transno", transno);
                        cm.Parameters.AddWithValue("@pcode", pcode);
                        cm.Parameters.AddWithValue("@price", price);
                        cm.Parameters.AddWithValue("@qty", novaQtd);
                        cm.Parameters.AddWithValue("@sdate", DateTime.Now);
                        cm.Parameters.AddWithValue("@disc", 0); // Sem desconto por padrão
                        cm.Parameters.AddWithValue("@total", price * novaQtd);
                        cm.Parameters.AddWithValue("@funcionario", fpos.lblName.Text ?? "Desconhecido");

                        cm.ExecuteNonQuery();
                    }

                    // Atualiza a interface do formulário POS
                    fpos.txtSearch.Clear();
                    fpos.txtSearch.Focus();
                    fpos.LoadCar(); // Recarrega o carrinho
                    this.Dispose(); // Fecha o formulário de quantidade
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar: {ex.Message}", stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (cn.State == ConnectionState.Open)
                        cn.Close();
                }
            }

        }

    }
}

