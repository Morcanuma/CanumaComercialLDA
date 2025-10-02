using lojaCanuma.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tulpep.NotificationWindow;


namespace lojaCanuma
{
    public partial class frmPOS : Form
    {
       
        public int pontosUsadosNaCompra = 0;
        public int pontosAntesDaCompra = 0;

        // 👶 flags de controle da venda atual
        private bool clienteFixado = false;         // travar troca de cliente
        private bool descontoAplicado = false;      // já usei pontos?
        private bool pontosJaRecreditados = false;  // pra não devolver 2x
        private int clienteIdNoDesconto = 0;       // quem recebeu/consumiu os pontos


        // 👇 Fica no topo da classe frmPOS (campos privados)
        private ListBox lstSugClientes;                   // a “caixinha” com os resultados
        private readonly CustomerRepository _custRepo = new CustomerRepository(); // para buscar no banco
                                                                                  // para “esperar” você parar de digitar 0,5s

        // === Loyalty/Promo ===
        private readonly lojaCanuma.Services.LoyaltyService _loySvc =
        new lojaCanuma.Services.LoyaltyService(new DBConnection().MyConnection());
        private lojaCanuma.Models.LoyaltySettings _loySet;



        string nomeLogado = SessaoUsuario.Nome;
        string cargoLogado = SessaoUsuario.Cargo;


        String id;
        String price;
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        int qty;
        DBConnection dbcon = new DBConnection();

        string stitle = "Sistema de Vendas(morcanuma)";
        public frmPOS()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            lblDate.Text = DateTime.Now.ToLongDateString();
            this.KeyPreview = true;

        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        // 👶 liga/desliga botões de acordo com as regras
        private void AtualizarEstadoUI()
        {
            bool temItens = dataGridView1.Rows.Count > 0;

            cmbCustomer.Enabled = !clienteFixado;                  // travar cliente após desconto
            btnDiscount.Enabled = temItens && !descontoAplicado;   // só aplica 1x
            btnSettle.Enabled = temItens;                        // finalizar quando tem itens
            btnCancel.Enabled = temItens;                        // esvaziar carrinho
            btnSearch.Enabled = true;                            // pesquisar produto sempre
            btnNew.Enabled = !temItens;                       // nova transação só com carrinho vazio
            txtCustumerSearch.Enabled = !clienteFixado;
            cmbCustomer.Enabled = !clienteFixado;


        }

        // chama quando o carrinho pode ter ficado vazio por remoções
        private void CheckCarrinhoVazioRecreditar()
        {
            if (dataGridView1.Rows.Count == 0)
            {
                // devolve pontos se havia desconto aplicado e ainda não pagou
                RecreditarPontosSePreciso("Carrinho ficou vazio por remoção de itens");
                // limpa flags e ajusta UI para a próxima venda
                ResetarEstadoDaVenda();
                AtualizarEstadoUI();
            }
        }


        // 👶 se usei pontos e não finalizei, devolvo ao cliente
        private void RecreditarPontosSePreciso(string motivo)
        {
            try
            {
                if (!descontoAplicado) return;
                if (pontosJaRecreditados) return;
                if (pontosUsadosNaCompra <= 0) return;

                int idCliente = clienteIdNoDesconto > 0
                    ? clienteIdNoDesconto
                    : (cmbCustomer.SelectedValue is int v ? v : 0);

                if (idCliente <= 0) return;

                using (var cnx = new SqlConnection(new DBConnection().MyConnection()))
                using (var cmd = new SqlCommand(
                    "UPDATE tblCustomer SET Points = Points + @pts WHERE CustomerId = @id", cnx))
                {
                    cnx.Open();
                    cmd.Parameters.AddWithValue("@pts", pontosUsadosNaCompra);
                    cmd.Parameters.AddWithValue("@id", idCliente);
                    cmd.ExecuteNonQuery();
                }

                pontosJaRecreditados = true;
                MessageBox.Show("Operação cancelada. Os pontos utilizados foram recreditados ao cliente.",
                                "Venda cancelada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AtualizarPontosCliente();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao recreditar pontos: " + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 👶 limpa todas as flags da venda atual
        private void ResetarEstadoDaVenda()
        {
            clienteFixado = false;
            descontoAplicado = false;
            pontosJaRecreditados = false;
            clienteIdNoDesconto = 0;
            pontosUsadosNaCompra = 0;
            AtualizarEstadoUI();
        }



        private void button7_Click(object sender, EventArgs e)
        {
            // Verifica se há uma transação em andamento (itens no carrinho)
            if (dataGridView1.Rows.Count > 0)
            {
                MessageBox.Show(
                    "Não é possível encerrar a sessão enquanto há uma venda em andamento.\n" +
                    "Por favor, finalize ou cancele a transação antes de sair.",
                    "Transação em andamento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return; // Impede o logout
            }

            // Confirma com o usuário se realmente deseja sair da sessão
            DialogResult confirmacao = MessageBox.Show(
                "Tem certeza de que deseja finalizar a sessão atual e voltar à tela de autenticação?",
                "Confirmação de Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // Se o usuário confirmar, fecha o formulário atual e abre o login
            if (confirmacao == DialogResult.Yes)
            {
                RecreditarPontosSePreciso("");
                ResetarEstadoDaVenda();
                // fecha o programa ou vai para tela de login
                this.Hide(); // Ou this.Close(); se preferir encerrar completamente a janela
                frmLogin login = new frmLogin();
                login.Show();
            }



        }




        public void GetTransNo()
        {
            try
            {
                if (cn.State == ConnectionState.Open) cn.Close();
                cn.Open();

                cm = new SqlCommand("sp_GerarTransNo", cn);
                cm.CommandType = CommandType.StoredProcedure;

                string novoTransNo = cm.ExecuteScalar().ToString();
                lblTransno.Text = novoTransNo;

                cn.Close();
            }
            catch (Exception ex)
            {
                if (cn.State == ConnectionState.Open) cn.Close();
                MessageBox.Show("Erro ao gerar número de transação: " + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool TransacaoFoiUsada(string transno)
        {
            bool usada = false;
            using (SqlConnection cn = new SqlConnection(dbcon.MyConnection()))
            {
                string query = "SELECT COUNT(*) FROM tblCar WHERE transno = @transno";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@transno", transno);

                cn.Open();
                int count = (int)cmd.ExecuteScalar();
                cn.Close();

                usada = count > 0;
            }
            return usada;
        }


        private string GerarNovoTransno()
        {
            string transno = "";

            using (SqlConnection cn = new SqlConnection(dbcon.MyConnection()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GerarTransNo", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.Open();
                    object result = cmd.ExecuteScalar();
                    cn.Close();

                    transno = result?.ToString();
                }
            }

            return transno;
        }



        private void btnNew_Click(object sender, EventArgs e)
        {
            // Se houver itens no carrinho, não permitir nova transação
            if (dataGridView1.Rows.Count > 0)
            {
                MessageBox.Show(
                    "Finalize ou cancele a transação atual antes de iniciar uma nova.",
                    "Transação em andamento",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Verifica se o transno atual foi usado
            string transAtual = lblTransno.Text.Trim();

            if (string.IsNullOrEmpty(transAtual) || transAtual == "00000000000000")
            {
                // Não havia transação anterior — gera nova
                lblTransno.Text = GerarNovoTransno();
            }
            else
            {
                // Se o transno anterior já foi usado, gera novo
                if (TransacaoFoiUsada(transAtual))
                {
                    lblTransno.Text = GerarNovoTransno();
                    ResetarEstadoDaVenda();
                }
                else
                {
                    MessageBox.Show(
                        "Você ainda não usou o número de transação anterior.\nContinuando com o mesmo número.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }

            txtSearch.Enabled = true;
            txtSearch.Focus();
        }


        private void txtSearch_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSearch.Text)) return;

                string _pcode;
                double _price;
                int _qty;

                if (cn.State == ConnectionState.Open) cn.Close();
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblProduct WHERE barcode = @barcode", cn))
                {
                    cmd.Parameters.AddWithValue("@barcode", txtSearch.Text);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            qty = int.Parse(dr["qty"].ToString());
                            _pcode = dr["pcode"].ToString();
                            _price = double.Parse(dr["price"].ToString());

                            if (!int.TryParse(txtQty.Text, out _qty))
                            {
                                MessageBox.Show("Por favor, insira uma quantidade válida.", "Quantidade Inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // FECHA O READER ANTES DE CHAMAR AddToCart
                            dr.Close();
                            cn.Close();

                            AddToCart(_pcode, _price, qty);
                        }
                        else
                        {
                            MessageBox.Show("Produto não encontrado.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void AddToCart(string _pcode, double _price, int qty)
        {
            try
            {
                bool found = false;
                int cart_qty = 0;
                string id = "";

                if (cn.State == ConnectionState.Closed)
                    cn.Open();

                // Verifica se o item já existe no carrinho
                cm = new SqlCommand("SELECT * FROM tblCar WHERE transno = @transno AND pcode = @pcode", cn);
                cm.Parameters.AddWithValue("@transno", lblTransno.Text); // Número da transação
                cm.Parameters.AddWithValue("@pcode", _pcode);            // Código do produto

                dr = cm.ExecuteReader();
                if (dr.Read())
                {
                    found = true;
                    id = dr["id"].ToString();                           // ID da linha no carrinho
                    cart_qty = int.Parse(dr["qty"].ToString());         // Quantidade já no carrinho
                }
                dr.Close();
                cn.Close();

                int novaQtd = int.Parse(txtQty.Text);                   // Quantidade sendo adicionada
                int totalSolicitado = novaQtd + cart_qty;               // Soma da quantidade

                // Verifica se a soma ultrapassa o estoque disponível
                if (totalSolicitado > qty)
                {
                    MessageBox.Show(
                        $"Não foi possível adicionar essa quantidade.\n\n" +
                        $"Você já tem {cart_qty} unidade(s) no carrinho.\n" +
                        $"Com mais {novaQtd}, seriam {totalSolicitado}, mas o estoque atual é de apenas {qty}.\n\n" +
                        $"Por favor, ajuste a quantidade.",
                        "Estoque Limitado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation
                    );
                    return;
                }

                // Atualiza se já existe
                if (found)
                {
                    if (cn.State == ConnectionState.Closed)
                        cn.Open();

                    cm = new SqlCommand("UPDATE tblCar SET qty = qty + @qty WHERE id = @id", cn);
                    cm.Parameters.AddWithValue("@qty", novaQtd);
                    cm.Parameters.AddWithValue("@id", id);
                    cm.ExecuteNonQuery();

                    // Atualiza interface POS
                    txtSearch.SelectionStart = 0;
                    txtSearch.SelectionStart = txtSearch.Text.Length;
                    LoadCar();
                    //this.Dispose();
                }
                else
                {
                    // Insere novo item
                    if (cn.State == ConnectionState.Closed)
                        cn.Open();

                    cm = new SqlCommand("INSERT INTO tblCar (transno, pcode, price, qty, sdate, disc, total, funcionario) " +
                                        "VALUES (@transno, @pcode, @price, @qty, @sdate, @disc, @total, @funcionario)", cn);

                    cm.Parameters.AddWithValue("@transno", lblTransno.Text);
                    cm.Parameters.AddWithValue("@pcode", _pcode);
                    cm.Parameters.AddWithValue("@price", _price);
                    cm.Parameters.AddWithValue("@qty", novaQtd);
                    cm.Parameters.AddWithValue("@sdate", DateTime.Now);
                    cm.Parameters.AddWithValue("@disc", 0); // Sem desconto
                    cm.Parameters.AddWithValue("@total", _price * novaQtd);
                    cm.Parameters.AddWithValue("@funcionario", lblName.Text ?? "Desconhecido");

                    cm.ExecuteNonQuery();
                }

                // Atualiza interface POS (corrigido)
                txtSearch.SelectionStart = 0;
                txtSearch.SelectionStart = txtSearch.Text.Length;
                LoadCar();

                // Só depois que tudo foi feito, fecha o formulário
                //this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }



        public void LoadCar()
            {
                try
            {
                Boolean hasrecord = false;
                dataGridView1.Rows.Clear();
                int i = 0;
                double totalBruto = 0;  // Total SEM desconto (price * qty)
                double totalDesconto = 0; // Soma dos descontos

                if (cn.State == ConnectionState.Open) cn.Close();
                cn.Open();

                cm = new SqlCommand("SELECT c.id, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.total FROM tblCar AS c INNER JOIN tblProduct AS p ON c.pcode = p.pcode WHERE transno LIKE '" + lblTransno.Text + "' and status like 'Pending'", cn);
                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    i++;
                    double preco = double.Parse(dr["price"].ToString());
                    int qty = int.Parse(dr["qty"].ToString());
                    double disc = double.Parse(dr["disc"].ToString());
                     
                    hasrecord = true;

                    totalBruto += preco * qty;     // Soma o total BRUTO
                    totalDesconto += disc;         // Soma os descontos

                    Image imgAdd = Properties.Resources.icons8_plus_25__1_;      // ícone que você adicionou no projeto
                    Image imgRemove = Properties.Resources.icons8_subtract_25; // outro ícone

                    // Adiciona linha no DataGridView (mantendo sua estrutura original)
                    dataGridView1.Rows.Add(
                        i,
                        dr["id"].ToString(),
                        dr["pcode"].ToString(),
                        dr["pdesc"].ToString(),
                        preco.ToString("N2"),
                        qty.ToString(),
                        disc.ToString("N2"),
                        (preco * qty - disc).ToString("N2"), // Exibe total líquido na grid
                        imgAdd,
                        imgRemove
                    );
                }
                AtualizarEstadoUI();

                dr.Close();
                cn.Close();

                // Atualiza labels (corrigido)
                lblSub.Text = totalBruto.ToString("N2") + " Kzs"; // Exibe total BRUTO formatado
                lblDiscount.Text = totalDesconto != 0 ? $"-{totalDesconto:N2} Kzs" : "0.00 Kzs"; // Exibe desconto formatado
                GetCartTotal(); // Atualiza totais finais
                if (hasrecord == true) { btnSettle.Enabled = true;  btnDiscount.Enabled = true; btnCancel.Enabled = true; } else { btnSettle.Enabled = false; btnDiscount.Enabled = false; btnCancel.Enabled = false; }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (cn.State == ConnectionState.Open) cn.Close();
            }
        }

        public void NotifyCriticalItems()
        {
            try
            {
                // 🔸 Consulta 1: Total de itens críticos
                string totalCriticos;
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM vwCriticalItems", cn))
                {
                    cn.Open();
                    totalCriticos = cmd.ExecuteScalar()?.ToString() ?? "0";
                    cn.Close();
                }

                // 🔸 Consulta 2: Lista dos itens críticos
                StringBuilder listaCriticos = new StringBuilder();
                int contador = 0;

                cn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM vwCriticalItems", cn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contador++;
                        string produto = reader["pdesc"].ToString();
                        string codigo = reader["pcode"].ToString();
                        string estoque = reader["qty"].ToString();

                        listaCriticos.AppendLine($"{contador}. {produto} ({codigo}) - Estoque: {estoque}");
                    }
                }
                cn.Close();

                // 🔊 Toca som de aviso (Windows beep)
                System.Media.SystemSounds.Exclamation.Play();

                // 🔔 Exibe popup persistente com botão de fechar
                PopupNotifier popup = new PopupNotifier
                {
                    Image = Properties.Resources.icons8_critical_64, // ícone de alerta
                    TitleText = $"⚠ {totalCriticos} ITENS CRÍTICOS ENCONTRADOS",
                    ContentText = listaCriticos.ToString(),

                    // Estilo do popup
                    TitleColor = Color.DarkRed,
                    ContentColor = Color.Black,
                    TitleFont = new Font("Segoe UI", 10, FontStyle.Bold),
                    ContentFont = new Font("Segoe UI", 9),
                    BodyColor = Color.WhiteSmoke,
                    BorderColor = Color.DarkRed,

                    // ⚠️ Mantém o popup aberto até o usuário clicar
                    Delay = int.MaxValue,
                    ShowCloseButton = true,

                    // Animação suave
                    AnimationInterval = 10,
                    AnimationDuration = 500
                };

                popup.Popup(); // Exibe
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Erro ao gerar notificação de estoque crítico:\n" + ex.Message,
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void frmPOS_Load(object sender, EventArgs e)
        {
            AtualizarEstadoUI();

            _loySet = _loySvc.GetSettings();   // lê as regras atuais do banco


            LoadCustomers();

            lblName.Text = SessaoUsuario.Nome;
            lblRole.Text = SessaoUsuario.Cargo;
            NotifyCriticalItems();

            // ListBox dentro do painel, ocupando tudo
            lstSugClientes = new ListBox
            {
                Visible = false,              // começa escondida
                IntegralHeight = false,       // evita cortar itens
                Dock = DockStyle.Fill,        // ocupa 100% do painel
                Font = txtCustumerSearch.Font // mesma fonte da textbox
            };

            // adiciona a listbox DENTRO do painel de sugestões
            pnlSugArea.Controls.Add(lstSugClientes);
            pnlSugArea.BringToFront();

            // eventos da listbox
            lstSugClientes.Click += (s, ev) => SelecionarClienteDaLista();
            lstSugClientes.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.Enter) { ev.Handled = true; SelecionarClienteDaLista(); }
                else if (ev.KeyCode == Keys.Escape) { lstSugClientes.Visible = false; }
            };

            // Timer de debounce (espera parar de digitar)
            timerPesquisa = new Timer { Interval = 500 };
            timerPesquisa.Tick += timerPesquisa_Tick;

            // eventos da textbox
            txtCustumerSearch.TextChanged += txtCustumerSearch_TextChanged;
            txtCustumerSearch.KeyDown += txtCustumerSearch_KeyDown;
            txtCustumerSearch.LostFocus += (s, ev) =>
            {
                // se perdeu foco e a lista não está focada, esconde
                BeginInvoke(new Action(() =>
                {
                    if (!lstSugClientes.Focused) lstSugClientes.Visible = false;
                }));
            };




        }




        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (lblTransno.Text == "00000000000000") { return; }
            frmLookUp frm = new frmLookUp(this);
            frm.LoadRecords();
            frm.ShowDialog();
        }





        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Obtém o nome da coluna clicada no DataGridView
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            // Verifica se a célula clicada pertence à coluna "delete"
            if (colName == "delete")
            {
                string produto = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString(); // Nome do produto

                // Pergunta se o usuário tem certeza que deseja remover o item
                DialogResult result = MessageBox.Show(
                    $"Tem certeza que deseja remover o produto \"{produto}\" do carrinho?",
                    stitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        cn.Open();

                        SqlCommand cmd = new SqlCommand("DELETE FROM tblCar WHERE id = @id", cn);
                        cmd.Parameters.AddWithValue("@id", dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());

                        cmd.ExecuteNonQuery();

                        MessageBox.Show(
                            $"O produto \"{produto}\" foi removido com sucesso!",
                            stitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ocorreu um erro ao tentar remover o produto \"{produto}\":\n{ex.Message}",
                            stitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                    finally
                    {
                        if (cn.State == ConnectionState.Open)
                            cn.Close();

                        LoadCar(); // Atualiza o DataGridView
                        CheckCarrinhoVazioRecreditar();
                    }
                }
            }

            else if (colName == "colAdd")
            {
                int estoqueDisponivel = 0;
                int qtdAtual = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString());

                using (SqlCommand cmd = new SqlCommand("SELECT SUM(qty) FROM tblproduct WHERE pcode = @pcode", cn))
                {
                    cmd.Parameters.AddWithValue("@pcode", dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());
                    cn.Open();
                    estoqueDisponivel = Convert.ToInt32(cmd.ExecuteScalar());
                    cn.Close();
                }

                if (qtdAtual < estoqueDisponivel)
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE tblcar SET qty = qty + 1 WHERE transno = @transno AND pcode = @pcode", cn))
                    {
                        cmd.Parameters.AddWithValue("@transno", lblTransno.Text);
                        cmd.Parameters.AddWithValue("@pcode", dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }
                    LoadCar(); // Atualiza carrinho
                }
                else
                {
                    string produto = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString(); // assumindo que a coluna 3 tem a descrição
                    MessageBox.Show(
                        $"O produto \"{produto}\" atingiu o limite disponível em estoque.\nVerifique o inventário ou ajuste a quantidade.",
                        "Estoque Esgotado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                }
            }

            else if (colName == "colRemove")
            {
                int qtdAtual = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString());

                if (qtdAtual > 1)
                {
                    // Decrementa a quantidade em 1
                    using (SqlCommand cmd = new SqlCommand("UPDATE tblcar SET qty = qty - 1 WHERE transno = @transno AND pcode = @pcode", cn))
                    {
                        cmd.Parameters.AddWithValue("@transno", lblTransno.Text);
                        cmd.Parameters.AddWithValue("@pcode", dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());

                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }
                }
                else
                {
                    // Confirma a remoção total do item do carrinho
                    string produto = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString(); // descrição
                    DialogResult result = MessageBox.Show(
                        $"Deseja remover completamente o produto \"{produto}\" do carrinho?",
                        "Remover Item",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM tblcar WHERE transno = @transno AND pcode = @pcode", cn))
                        {
                            cmd.Parameters.AddWithValue("@transno", lblTransno.Text);
                            cmd.Parameters.AddWithValue("@pcode", dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());

                            cn.Open();
                            cmd.ExecuteNonQuery();
                            cn.Close();
                        }
                    }
                }

                LoadCar(); // Atualiza carrinho
            }



        }


        private void GetCartTotal()
        {
            try
            {
                double subTotal = double.Parse(lblSub.Text.Replace("Kzs", "").Trim());
                double discount = double.Parse(lblDiscount.Text.Replace("Kzs", "").Replace("-", "").Trim());
                double total = subTotal - discount;

                // GARANTE o Kzs
                lblTotal.Text = total.ToString("N2") + " Kzs";
                lblDisplayTotal.Text = total.ToString("N2") + " Kzs";  // <- se estiver usando outro label visível

                lblDiscount.Text = discount != 0 ? $"-{discount:N2} Kzs" : "0.00 Kzs";
                lblVat.Text = "0.00 Kzs";
                lblVatable.Text = "0.00 Kzs";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao calcular totais: " + ex.Message);
            }
        }




        private void btnDiscount_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int id = int.Parse(dataGridView1.CurrentRow.Cells[1].Value.ToString()); // Alterado de Cells[0] para Cells[1]
                double preco = double.Parse(dataGridView1.CurrentRow.Cells[4].Value.ToString());

                // Cria o objeto item
                ItemVenda item = new ItemVenda
                {
                    Id = id,
                    PrecoOriginal = preco
                };

                // Passa para o novo formulário
                frmDiscount frm = new frmDiscount(this, item);
                frm.ShowDialog();
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int i = dataGridView1.CurrentRow.Index;

            if (dataGridView1.Rows[i].Cells.Count > 3)
            {
                price = dataGridView1.Rows[i].Cells[3].Value?.ToString() ?? "0.00";
            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");  // "tt" mostra AM/PM
            lblDate1.Text = DateTime.Now.ToLongDateString();
        }

        private void btnSettle_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0) return;

            ReloadLoyaltySettingsSafe();  // <<< pega o que foi salvo agora há pouco
            ApplyLoyaltyRedeem();         // tenta aplicar o desconto por pontos
            GetCartTotal();

            using (var frm = new frmSettle(this))
            {
                frm.txtSale.Text = lblDisplayTotal.Text;
                var result = frm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ResetarEstadoDaVenda();
                    LoadCar();
                    txtCustumerSearch.Enabled = true;
                    cmbCustomer.Enabled = true;
                    if (lstSugClientes != null) lstSugClientes.Visible = false;
                    AtualizarEstadoUI();
                }
            }
        }

        private void btnSale_Click(object sender, EventArgs e)
        {
            frmSoldItem frm = new frmSoldItem(this);
            
            
            frm.ShowDialog();
            
        }

        private void frmPOS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1) btnNew_Click(sender, e);
            else if (e.KeyCode == Keys.F2) btnSearch_Click(sender, e);
            else if (e.KeyCode == Keys.F3) btnDiscount_Click(sender, e);
            else if (e.KeyCode == Keys.F4) btnSettle_Click(sender, e);
            else if (e.KeyCode == Keys.F5) btnCancel_Click(sender, e);
            else if (e.KeyCode == Keys.F6) btnSale_Click(sender, e);
            else if (e.KeyCode == Keys.F7) btnChangePass_Click(sender, e);
            else if (e.KeyCode == Keys.F10) this.Close(); // <- AGORA SIM, só no F10
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Mensagem clara e profissional de confirmação
            DialogResult confirmacao = MessageBox.Show(
                "Desejas realmente remover todos os itens do carrinho?\nEssa ação não poderá ser desfeita.",
                "Confirmação de Remoção",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacao == DialogResult.Yes)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM tblcar WHERE transno = @transno", cn))
                    {
                        cmd.Parameters.AddWithValue("@transno", lblTransno.Text);

                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }

                    // Mensagem pós-remoção mais polida
                    MessageBox.Show(
                        "O carrinho foi esvaziado com sucesso!",
                        "Carrinho Limpo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                 

                    // devolve pontos se havia desconto aplicado
                    RecreditarPontosSePreciso("");

                    // reseta estado para próxima venda
                    ResetarEstadoDaVenda();
                    LoadCar(); // Atualiza o DataGridView
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ocorreu um erro ao tentar limpar o carrinho:\n" + ex.Message,
                        "Erro de Sistema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                finally
                {
                    if (cn.State == ConnectionState.Open)
                        cn.Close();
                }
            }
        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            frmChangePassword frm = new frmChangePassword();
            frm.ShowDialog();
        }

        private void cmbCustomer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // 🚫 trava troca de cliente se já aplicou desconto por pontos
            if (clienteFixado)
            {
                MessageBox.Show(
                    "Não é possível trocar o cliente: já existe desconto por pontos aplicado nesta venda.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (cmbCustomer.SelectedValue == null)
            {
                lblCustomerPoints.Text = "Pontos: 0";
                return;
            }

            int id = (int)cmbCustomer.SelectedValue;

            var repo = new CustomerRepository();
            var cliente = repo.GetById(id);

            lblCustomerPoints.Text = cliente != null
                ? $"Pontos: {cliente.Points}"
                : "Pontos: 0";
        }

        public void AtualizarPontosCliente()
        {
            if (cmbCustomer.SelectedValue == null) return;

            int idCliente = (int)cmbCustomer.SelectedValue;

            // Busca novamente o cliente no banco com os pontos atualizados
            var repo = new CustomerRepository();
            var clienteAtualizado = repo.GetAll().FirstOrDefault(c => c.CustomerId == idCliente);

            lblCustomerPoints.Text = clienteAtualizado != null
                ? $"Pontos: {clienteAtualizado.Points}"
                : "Pontos: 0";
        }


        private void btnNewCustomer_Click(object sender, EventArgs e)
        {
            // Abre o formulário de cadastro/listagem de clientes
            using (var frm = new frmCustomer())
            {
                frm.ShowDialog();
            }
            // Recarrega a lista no combo para incluir o novo cliente
            LoadCustomers();
        }

        /// <summary>
        /// Busca todos os clientes e popula o combo.
        /// </summary>

        private void LoadCustomers()
        {
            var lista = new CustomerRepository().GetAll();
            lista.Insert(0, new CustomerModels { CustomerId = 0, Name = "SEM CLIENTE", Points = 0 });

            var listaFormatada = lista.Select(c => new
            {
                c.CustomerId,
                c.Points,
                Display = $"{c.Name} ({c.CustomerId})"
            }).ToList();

            cmbCustomer.DataSource = listaFormatada;
            cmbCustomer.DisplayMember = "Display";
            cmbCustomer.ValueMember = "CustomerId";
            cmbCustomer.SelectedIndex = 0;

            // autocomplete (opcional)
            cmbCustomer.DropDownStyle = ComboBoxStyle.DropDown;
            cmbCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbCustomer.AutoCompleteSource = AutoCompleteSource.ListItems;
        }



        private void cmbCustomer_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

      



        private void lblDisplayTotal_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void txtCustumerSearch_TextChanged(object sender, EventArgs e)
        {
            timerPesquisa.Stop();
            timerPesquisa.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timerPesquisa.Stop();
            CarregarSugestoesClientes(txtCustumerSearch.Text.Trim());
        }

        private void txtCustumerSearch_KeyDown(object sender, KeyEventArgs e)
        {
            // seta para BAIXO: entra na lista
            if (e.KeyCode == Keys.Down && lstSugClientes.Visible && lstSugClientes.Items.Count > 0)
            {
                lstSugClientes.Focus();
                if (lstSugClientes.SelectedIndex < 0)
                    lstSugClientes.SelectedIndex = 0;
                e.Handled = true;
            }
            // ENTER: escolhe o primeiro da lista
            else if (e.KeyCode == Keys.Enter && lstSugClientes.Visible && lstSugClientes.Items.Count > 0)
            {
                if (lstSugClientes.SelectedIndex < 0)
                    lstSugClientes.SelectedIndex = 0;

                SelecionarClienteDaLista();
                e.Handled = true;
            }
            // ESC: esconde a lista
            else if (e.KeyCode == Keys.Escape)
            {
                lstSugClientes.Visible = false;
                e.Handled = true;
            }
        }


        // 👇 Busca no repositório e preenche a listinha (ListBox).
        //   Se não achar nada, esconde.
        private void CarregarSugestoesClientes(string termo)
        {
            try
            {
                // se tá vazio, esconde e sai
                if (string.IsNullOrWhiteSpace(termo))
                {
                    lstSugClientes.Visible = false;
                    lstSugClientes.DataSource = null;
                    return;
                }

                // busca no banco por nome/telefone/email/ID (até 30 itens)
                var resultados = _custRepo.Search(termo, top: 30);

                if (resultados.Count == 0)
                {
                    lstSugClientes.Visible = false;
                    lstSugClientes.DataSource = null;
                    return;
                }

                // monta o texto que aparece: "Nome (ID)  •  Telefone"
                var dados = resultados.Select(c => new
                {
                    c.CustomerId,
                    c.Points,
                    Display = $"{c.Name} ({c.CustomerId})" +
                              (string.IsNullOrEmpty(c.Phone) ? "" : $"  •  {c.Phone}")
                }).ToList();

                // joga no ListBox
                lstSugClientes.DataSource = dados;
                lstSugClientes.DisplayMember = "Display";     // o que aparece
                lstSugClientes.ValueMember = "CustomerId";  // o “valor escondido” (ID)
                lstSugClientes.Visible = true;

                // garante posição e tamanho (se o layout mudou)
                lstSugClientes.Left = txtCustumerSearch.Left;
                lstSugClientes.Top = txtCustumerSearch.Bottom;
                lstSugClientes.Width = txtCustumerSearch.Width;
            }
            catch (Exception ex)
            {
                // se algo der errado, não trava a app
                lstSugClientes.Visible = false;
                MessageBox.Show("Erro ao pesquisar clientes: " + ex.Message, "Erro",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // 👇 Pega o item escolhido na lista e coloca no ComboBox (cmbCustomer).
        //   Também atualiza o label de pontos e volta o foco pro textbox.
        private void SelecionarClienteDaLista()
        {
            if (lstSugClientes == null || lstSugClientes.SelectedItem == null)
                return;

            int idSelecionado = 0;
            if (lstSugClientes.SelectedValue != null)
                idSelecionado = Convert.ToInt32(lstSugClientes.SelectedValue);

            if (idSelecionado > 0)
            {
                // seleciona no ComboBox oficial dos clientes
                cmbCustomer.SelectedValue = idSelecionado;

                // atualiza label de pontos (seu método já existente)
                AtualizarPontosCliente();
            }

            // esconde a lista e volta para o textbox (pronto pra próxima busca)
            lstSugClientes.Visible = false;
            txtCustumerSearch.Focus();
            txtCustumerSearch.SelectAll();
        }

        private void timerPesquisa_Tick(object sender, EventArgs e)
        {
            timerPesquisa.Stop();
            CarregarSugestoesClientes(txtCustumerSearch.Text.Trim());
        }

        public int GetSelectedCustomerIdSafe()
        {
            if (cmbCustomer?.SelectedValue == null) return 0;
            if (cmbCustomer.SelectedValue is int i) return i;
            int.TryParse(cmbCustomer.SelectedValue.ToString(), out int id);
            return id;
        }


        private List<lojaCanuma.Models.CartLine> BuildCartFromGrid()
        {
            var list = new List<lojaCanuma.Models.CartLine>();
            using (var cnx = new SqlConnection(dbcon.MyConnection()))
            {
                cnx.Open();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string pcode = row.Cells[2].Value?.ToString();
                    if (string.IsNullOrEmpty(pcode)) continue;

                    // Colunas da grid: 4=price, 5=qty (pelo seu AddToCart/LoadCar)
                    if (!decimal.TryParse(row.Cells[4].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                        price = 0m;
                    if (!int.TryParse(row.Cells[5].Value?.ToString(), out var qty))
                        qty = 0;

                    decimal cost = 0m;
                    using (var cmd = new SqlCommand("SELECT cost_price FROM tblProduct WHERE pcode=@p", cnx))
                    {
                        cmd.Parameters.AddWithValue("@p", pcode);
                        var obj = cmd.ExecuteScalar();
                        cost = obj == null || obj == DBNull.Value ? 0m : Convert.ToDecimal(obj, CultureInfo.InvariantCulture);
                    }

                    list.Add(new lojaCanuma.Models.CartLine { PCode = pcode, Price = price, Qty = qty, Cost = cost });
                }
            }
            return list;
        }


        // helper p/ ler "1.234,56 Kzs" ou "-1.000 Kzs"
        private static decimal ParseKzs(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt)) return 0m;
            var limp = txt.Replace("Kzs", "").Replace("-", "").Trim();
            decimal.TryParse(limp, NumberStyles.Any, CultureInfo.CurrentCulture, out var v);
            return v;
        }

        // recarrega settings do BD com segurança
        private void ReloadLoyaltySettingsSafe()
        {
            try
            {
                _loySet = _loySvc.GetSettings();
            }
            catch
            {
                if (_loySet == null)
                    _loySet = new lojaCanuma.Models.LoyaltySettings();
            }
        }


        private void ApplyLoyaltyRedeem()
        {
            try
            {
                // 0) pega config atual e sai se já houve resgate nesta venda
                if (descontoAplicado) return;
                ReloadLoyaltySettingsSafe();

                // 1) programa/promo precisam estar ligados
                if (_loySet == null || !_loySet.IsEnabled)
                {
                    MessageBox.Show("Programa de Pontos está desativado.", "Resgate de pontos",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (!_loySet.IsPromoEnabled)
                {
                    MessageBox.Show("Resgates estão desativados (promoção não ativa).",
                                    "Resgate de pontos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2) precisa ter cliente selecionado
                int customerId = GetSelectedCustomerIdSafe();
                if (customerId <= 0)
                {
                    MessageBox.Show("Selecione um cliente para usar pontos.", "Resgate de pontos",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 3) elegibilidade (mínimo de pontos + compra recente)
                var elig = _loySvc.CheckEligibilityToRedeem(customerId, _loySet);
                if (!elig.ok)
                {
                    MessageBox.Show(elig.motivo, "Resgate de pontos",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 4) calcula desconto seguro respeitando teto % e margem mínima (usa cost_price)
                var cart = BuildCartFromGrid();
                var calc = _loySvc.ComputeSafeRedeem(_loySet, cart);
                if (!calc.aplicar)
                {
                    MessageBox.Show("Resgate bloqueado: " + calc.motivoBloqueio,
                                    "Resgate de pontos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 5) aplica na UI (desconto por RECIBO – uma vez)
                decimal subAtual = ParseKzs(lblSub.Text);
                decimal descAtual = ParseKzs(lblDiscount.Text);
                decimal novoDesc = descAtual + calc.descontoKz;
                decimal novoTotal = Math.Max(0m, subAtual - novoDesc);

                lblDiscount.Text = (novoDesc > 0 ? "-" : "") + $"{novoDesc:N2} Kzs";
                lblTotal.Text = $"{novoTotal:N2} Kzs";
                lblDisplayTotal.Text = lblTotal.Text;

                // 6) debita os pontos (quantidade configurada por resgate)
                _loySvc.RedeemPoints(customerId, _loySet.RedeemUnitPoints, lblTransno.Text);

                // 7) flags/travas e refresh de UI/pontos do cliente
                pontosUsadosNaCompra = _loySet.RedeemUnitPoints;
                descontoAplicado = true;
                clienteFixado = true;
                pontosJaRecreditados = false;
                clienteIdNoDesconto = customerId;
                AtualizarEstadoUI();
                AtualizarPontosCliente();

                MessageBox.Show(
                    $"Desconto de {calc.descontoKz:N2} Kz aplicado. " +
                    $"{_loySet.RedeemUnitPoints} ponto(s) debitado(s).",
                    "Resgate de pontos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erro de BD ao aplicar resgate:\n" + ex.Message,
                                "Resgate de pontos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao aplicar resgate:\n" + ex.Message,
                                "Resgate de pontos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // em frmPOS
      

        private void frmPOS_Activated(object sender, EventArgs e)
        {
            ReloadLoyaltySettingsSafe();
        }
    }
}
