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
    public partial class fmrCategoryListcs : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        private fmrCategoryListcs _parent;

        public fmrCategoryListcs(fmrCategoryListcs parent)
        {
            InitializeComponent();
            LoadCategory();
            _parent = parent;
        }

        // Construtor sem parâmetro para casos em que não tenha lista
        public fmrCategoryListcs() : this(null) { }







        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadCategory()
        {
            try
            {
                int i = 0;
                dataGridView1.Rows.Clear();

                using (SqlConnection cn = new SqlConnection(dbcon.MyConnection()))
                {
                    cn.Open();
                    using (SqlCommand cm = new SqlCommand("SELECT * FROM tblCategory ORDER BY category", cn))
                    {
                        using (SqlDataReader dr = cm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                i++;
                                dataGridView1.Rows.Add(i, dr[0].ToString(), dr[1].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar categorias: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                frmCategory frm = new frmCategory(this);
                frm.txtCategory.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.lblD.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.ShowDialog();
            }

            else if (colName == "Delete")
            {
                if (MessageBox.Show("Tens certeza que queres eliminar esta categoria?", "Eliminar Registro", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        SqlConnection cn = new SqlConnection(dbcon.MyConnection());
                        cn.Open();
                        SqlCommand cm = new SqlCommand("DELETE FROM tblCategory WHERE id = @id", cn);
                        cm.Parameters.AddWithValue("@id", dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                        cm.ExecuteNonQuery();
                        cn.Close();

                        MessageBox.Show("Categoria eliminada com sucesso.", "Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCategory(); // Atualiza a lista
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            frmCategory frm = new frmCategory(this);
            frm.btnSave.Enabled = true;
            frm.btnUpdate.Enabled = false;
            frm.ShowDialog();
        }
    }
}
