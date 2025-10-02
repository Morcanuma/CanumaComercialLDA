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
    public partial class frmVoid : Form
    {

        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        frmCancelDetais f;
        public frmVoid(frmCancelDetais frm)
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtPass.Text))
                {
                    string user;

                    cn.Open();
                    cm = new SqlCommand("SELECT * FROM tbluser WHERE username = @username AND password = @password", cn);
                    cm.Parameters.AddWithValue("@username", txtUser.Text);
                    cm.Parameters.AddWithValue("@password", txtPass.Text);
                    dr = cm.ExecuteReader();

                    if (dr.Read())
                    {
                        user = dr["username"].ToString();

                        dr.Close();
                        cn.Close();

                        // Salva o cancelamento
                        SaveCancelOrder(user);

                        // Devolve item ao estoque, se necessário
                        if (f.cboAction.Text == "Sim")
                        {
                            cn.Open();
                            cm = new SqlCommand("UPDATE tblproduct SET qty = qty + @qty WHERE pcode = @pcode", cn);
                            cm.Parameters.AddWithValue("@qty", int.Parse(f.txtCancelQty.Text));
                            cm.Parameters.AddWithValue("@pcode", f.txtCProd.Text);
                            cm.ExecuteNonQuery();
                            cn.Close();
                        }

                        // Atualiza a quantidade no carro
                        cn.Open();
                        cm = new SqlCommand("UPDATE tblCar SET qty = qty - @qty WHERE id = @id", cn);
                        cm.Parameters.AddWithValue("@qty", int.Parse(f.txtCancelQty.Text));
                        cm.Parameters.AddWithValue("@id", f.txtId.Text);
                        cm.ExecuteNonQuery();
                        cn.Close();

                        // Mensagem de sucesso
                        MessageBox.Show("Transação do pedido cancelada com sucesso!", "Cancelar Pedido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Dispose();
                        f.RefreshList();
                        f.Dispose();
                    }

                    dr.Close();
                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        public void SaveCancelOrder(string user)
        {
            cn.Open();

            cm = new SqlCommand(@"INSERT INTO tblcancel 
        (transno, pcode, price, qty, sdate, voidby, cancelledby, reason, acao) 
        VALUES (@transno, @pcode, @price, @qty, @sdate, @voidby, @cancelledby, @reason, @acao)", cn);

            cm.Parameters.AddWithValue("@transno", f.txtTransno.Text);
            cm.Parameters.AddWithValue("@pcode", f.txtCProd.Text);
            cm.Parameters.AddWithValue("@price", double.Parse(f.txtPrice.Text));
            cm.Parameters.AddWithValue("@qty", int.Parse(f.txtCancelQty.Text));
            cm.Parameters.AddWithValue("@sdate", DateTime.Now);
            cm.Parameters.AddWithValue("@voidby", user);
            cm.Parameters.AddWithValue("@cancelledby", f.txtCancel.Text);
            cm.Parameters.AddWithValue("@reason", f.txtReason.Text);
            cm.Parameters.AddWithValue("@acao", f.cboAction.Text);


            cm.ExecuteNonQuery();

            cn.Close();
        }

       

    }
}
