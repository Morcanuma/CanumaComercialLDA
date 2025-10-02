namespace lojaCanuma
{
    partial class frmLoyaltyAndPromos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkPromo = new System.Windows.Forms.CheckBox();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpGanhos = new System.Windows.Forms.GroupBox();
            this.nudActivityDays = new System.Windows.Forms.NumericUpDown();
            this.lblActivityDays = new System.Windows.Forms.Label();
            this.nudExpireDays = new System.Windows.Forms.NumericUpDown();
            this.lblExpireDays = new System.Windows.Forms.Label();
            this.nudKzPerPoint = new System.Windows.Forms.NumericUpDown();
            this.lblKzPerPoint = new System.Windows.Forms.Label();
            this.grpResgate = new System.Windows.Forms.GroupBox();
            this.nudMaxPct = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.nudPtsPerRedeem = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nudKzPerRedeem = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nudMinPtsRedeem = new System.Windows.Forms.NumericUpDown();
            this.lblMinPts = new System.Windows.Forms.Label();
            this.grpProtecao = new System.Windows.Forms.GroupBox();
            this.lblAutoPauseHint = new System.Windows.Forms.Label();
            this.btnAutoPauseTest = new System.Windows.Forms.Button();
            this.nudAutoPauseDays = new System.Windows.Forms.NumericUpDown();
            this.labess = new System.Windows.Forms.Label();
            this.nudMinMargin = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.grpTeste = new System.Windows.Forms.GroupBox();
            this.txtMotivo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtResultado = new System.Windows.Forms.TextBox();
            this.lblResultado = new System.Windows.Forms.Label();
            this.btnSimularCarrinho = new System.Windows.Forms.Button();
            this.grpMetas = new System.Windows.Forms.GroupBox();
            this.btnSalvarMetas = new System.Windows.Forms.Button();
            this.nudMargemMinPorVenda = new System.Windows.Forms.NumericUpDown();
            this.nudMetaMargemLiquida = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.nudMetaFaturamento = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnSalvarConfig = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpGanhos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudActivityDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExpireDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKzPerPoint)).BeginInit();
            this.grpResgate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxPct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPtsPerRedeem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKzPerRedeem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinPtsRedeem)).BeginInit();
            this.grpProtecao.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoPauseDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinMargin)).BeginInit();
            this.grpTeste.SuspendLayout();
            this.grpMetas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMargemMinPorVenda)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetaMargemLiquida)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetaFaturamento)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkPromo);
            this.groupBox1.Controls.Add(this.chkEnable);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1055, 61);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configurações de Pontos & Promoções";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // chkPromo
            // 
            this.chkPromo.AutoSize = true;
            this.chkPromo.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPromo.Location = new System.Drawing.Point(670, 28);
            this.chkPromo.Name = "chkPromo";
            this.chkPromo.Size = new System.Drawing.Size(266, 24);
            this.chkPromo.TabIndex = 3;
            this.chkPromo.Text = "Permitir Resgates (Promoção ativa)";
            this.chkPromo.UseVisualStyleBackColor = true;
            this.chkPromo.CheckedChanged += new System.EventHandler(this.chkPromo_CheckedChanged);
            // 
            // chkEnable
            // 
            this.chkEnable.AutoSize = true;
            this.chkEnable.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkEnable.Location = new System.Drawing.Point(344, 26);
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.Size = new System.Drawing.Size(212, 24);
            this.chkEnable.TabIndex = 2;
            this.chkEnable.Text = "Ativar Programa de Pontos";
            this.chkEnable.UseVisualStyleBackColor = true;
            this.chkEnable.CheckedChanged += new System.EventHandler(this.chkEnable_CheckedChanged_1);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DodgerBlue;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1089, 34);
            this.panel1.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox1.Image = global::lojaCanuma.Properties.Resources.icons8_x_25;
            this.pictureBox1.Location = new System.Drawing.Point(1057, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 34);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(33, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "Pontos E  Promoções";
            // 
            // grpGanhos
            // 
            this.grpGanhos.Controls.Add(this.nudActivityDays);
            this.grpGanhos.Controls.Add(this.lblActivityDays);
            this.grpGanhos.Controls.Add(this.nudExpireDays);
            this.grpGanhos.Controls.Add(this.lblExpireDays);
            this.grpGanhos.Controls.Add(this.nudKzPerPoint);
            this.grpGanhos.Controls.Add(this.lblKzPerPoint);
            this.grpGanhos.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpGanhos.Location = new System.Drawing.Point(12, 138);
            this.grpGanhos.Name = "grpGanhos";
            this.grpGanhos.Size = new System.Drawing.Size(1055, 93);
            this.grpGanhos.TabIndex = 4;
            this.grpGanhos.TabStop = false;
            this.grpGanhos.Text = "Regras de ganhos";
            // 
            // nudActivityDays
            // 
            this.nudActivityDays.Location = new System.Drawing.Point(940, 28);
            this.nudActivityDays.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudActivityDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudActivityDays.Name = "nudActivityDays";
            this.nudActivityDays.Size = new System.Drawing.Size(75, 29);
            this.nudActivityDays.TabIndex = 5;
            this.nudActivityDays.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // lblActivityDays
            // 
            this.lblActivityDays.AutoSize = true;
            this.lblActivityDays.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActivityDays.Location = new System.Drawing.Point(674, 31);
            this.lblActivityDays.Name = "lblActivityDays";
            this.lblActivityDays.Size = new System.Drawing.Size(240, 20);
            this.lblActivityDays.TabIndex = 4;
            this.lblActivityDays.Text = "Compra recente obrigatória (dias)";
            // 
            // nudExpireDays
            // 
            this.nudExpireDays.Location = new System.Drawing.Point(516, 31);
            this.nudExpireDays.Maximum = new decimal(new int[] {
            3650,
            0,
            0,
            0});
            this.nudExpireDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudExpireDays.Name = "nudExpireDays";
            this.nudExpireDays.Size = new System.Drawing.Size(88, 29);
            this.nudExpireDays.TabIndex = 3;
            this.nudExpireDays.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lblExpireDays
            // 
            this.lblExpireDays.AutoSize = true;
            this.lblExpireDays.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExpireDays.Location = new System.Drawing.Point(303, 37);
            this.lblExpireDays.Name = "lblExpireDays";
            this.lblExpireDays.Size = new System.Drawing.Size(196, 20);
            this.lblExpireDays.TabIndex = 2;
            this.lblExpireDays.Text = " Validade dos pontos (dias):";
            // 
            // nudKzPerPoint
            // 
            this.nudKzPerPoint.DecimalPlaces = 2;
            this.nudKzPerPoint.Location = new System.Drawing.Point(154, 37);
            this.nudKzPerPoint.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.nudKzPerPoint.Name = "nudKzPerPoint";
            this.nudKzPerPoint.Size = new System.Drawing.Size(97, 29);
            this.nudKzPerPoint.TabIndex = 1;
            this.nudKzPerPoint.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // lblKzPerPoint
            // 
            this.lblKzPerPoint.AutoSize = true;
            this.lblKzPerPoint.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKzPerPoint.Location = new System.Drawing.Point(26, 40);
            this.lblKzPerPoint.Name = "lblKzPerPoint";
            this.lblKzPerPoint.Size = new System.Drawing.Size(112, 20);
            this.lblKzPerPoint.TabIndex = 0;
            this.lblKzPerPoint.Text = "Kz por 1 ponto:";
            // 
            // grpResgate
            // 
            this.grpResgate.Controls.Add(this.nudMaxPct);
            this.grpResgate.Controls.Add(this.label4);
            this.grpResgate.Controls.Add(this.nudPtsPerRedeem);
            this.grpResgate.Controls.Add(this.label2);
            this.grpResgate.Controls.Add(this.nudKzPerRedeem);
            this.grpResgate.Controls.Add(this.label3);
            this.grpResgate.Controls.Add(this.nudMinPtsRedeem);
            this.grpResgate.Controls.Add(this.lblMinPts);
            this.grpResgate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpResgate.Location = new System.Drawing.Point(12, 265);
            this.grpResgate.Name = "grpResgate";
            this.grpResgate.Size = new System.Drawing.Size(1055, 137);
            this.grpResgate.TabIndex = 6;
            this.grpResgate.TabStop = false;
            this.grpResgate.Text = "Resgate (desconto por pontos)";
            // 
            // nudMaxPct
            // 
            this.nudMaxPct.DecimalPlaces = 2;
            this.nudMaxPct.Location = new System.Drawing.Point(742, 85);
            this.nudMaxPct.Name = "nudMaxPct";
            this.nudMaxPct.Size = new System.Drawing.Size(88, 29);
            this.nudMaxPct.TabIndex = 7;
            this.nudMaxPct.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(472, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(244, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Desconto máximo (% do subtotal):";
            // 
            // nudPtsPerRedeem
            // 
            this.nudPtsPerRedeem.Location = new System.Drawing.Point(284, 88);
            this.nudPtsPerRedeem.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudPtsPerRedeem.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPtsPerRedeem.Name = "nudPtsPerRedeem";
            this.nudPtsPerRedeem.Size = new System.Drawing.Size(97, 29);
            this.nudPtsPerRedeem.TabIndex = 5;
            this.nudPtsPerRedeem.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(26, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Ao resgatar usa (pontos):";
            // 
            // nudKzPerRedeem
            // 
            this.nudKzPerRedeem.DecimalPlaces = 2;
            this.nudKzPerRedeem.Location = new System.Drawing.Point(742, 37);
            this.nudKzPerRedeem.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.nudKzPerRedeem.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudKzPerRedeem.Name = "nudKzPerRedeem";
            this.nudKzPerRedeem.Size = new System.Drawing.Size(88, 29);
            this.nudKzPerRedeem.TabIndex = 3;
            this.nudKzPerRedeem.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(472, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = " Concede (Kz):";
            // 
            // nudMinPtsRedeem
            // 
            this.nudMinPtsRedeem.Location = new System.Drawing.Point(284, 37);
            this.nudMinPtsRedeem.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudMinPtsRedeem.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMinPtsRedeem.Name = "nudMinPtsRedeem";
            this.nudMinPtsRedeem.Size = new System.Drawing.Size(97, 29);
            this.nudMinPtsRedeem.TabIndex = 1;
            this.nudMinPtsRedeem.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblMinPts
            // 
            this.lblMinPts.AutoSize = true;
            this.lblMinPts.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMinPts.Location = new System.Drawing.Point(26, 40);
            this.lblMinPts.Name = "lblMinPts";
            this.lblMinPts.Size = new System.Drawing.Size(234, 20);
            this.lblMinPts.TabIndex = 0;
            this.lblMinPts.Text = "Mínimo de pontos para resgatar:";
            // 
            // grpProtecao
            // 
            this.grpProtecao.Controls.Add(this.lblAutoPauseHint);
            this.grpProtecao.Controls.Add(this.btnAutoPauseTest);
            this.grpProtecao.Controls.Add(this.nudAutoPauseDays);
            this.grpProtecao.Controls.Add(this.labess);
            this.grpProtecao.Controls.Add(this.nudMinMargin);
            this.grpProtecao.Controls.Add(this.label8);
            this.grpProtecao.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpProtecao.Location = new System.Drawing.Point(12, 433);
            this.grpProtecao.Name = "grpProtecao";
            this.grpProtecao.Size = new System.Drawing.Size(1050, 173);
            this.grpProtecao.TabIndex = 8;
            this.grpProtecao.TabStop = false;
            this.grpProtecao.Text = "Proteção de Lucro";
            // 
            // lblAutoPauseHint
            // 
            this.lblAutoPauseHint.AutoSize = true;
            this.lblAutoPauseHint.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAutoPauseHint.ForeColor = System.Drawing.Color.Gray;
            this.lblAutoPauseHint.Location = new System.Drawing.Point(357, 113);
            this.lblAutoPauseHint.Name = "lblAutoPauseHint";
            this.lblAutoPauseHint.Size = new System.Drawing.Size(221, 20);
            this.lblAutoPauseHint.TabIndex = 12;
            this.lblAutoPauseHint.Text = "(resultado/apontamentos aqui)";
            // 
            // btnAutoPauseTest
            // 
            this.btnAutoPauseTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.btnAutoPauseTest.FlatAppearance.BorderSize = 0;
            this.btnAutoPauseTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAutoPauseTest.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoPauseTest.ForeColor = System.Drawing.Color.White;
            this.btnAutoPauseTest.Location = new System.Drawing.Point(30, 77);
            this.btnAutoPauseTest.Name = "btnAutoPauseTest";
            this.btnAutoPauseTest.Size = new System.Drawing.Size(236, 33);
            this.btnAutoPauseTest.TabIndex = 11;
            this.btnAutoPauseTest.Text = "Testar auto-pause (ult. 7 dias)";
            this.btnAutoPauseTest.UseVisualStyleBackColor = false;
            this.btnAutoPauseTest.Click += new System.EventHandler(this.btnAutoPauseTest_Click_1);
            // 
            // nudAutoPauseDays
            // 
            this.nudAutoPauseDays.Location = new System.Drawing.Point(876, 37);
            this.nudAutoPauseDays.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudAutoPauseDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudAutoPauseDays.Name = "nudAutoPauseDays";
            this.nudAutoPauseDays.Size = new System.Drawing.Size(97, 29);
            this.nudAutoPauseDays.TabIndex = 5;
            this.nudAutoPauseDays.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // labess
            // 
            this.labess.AutoSize = true;
            this.labess.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labess.Location = new System.Drawing.Point(538, 40);
            this.labess.Name = "labess";
            this.labess.Size = new System.Drawing.Size(315, 20);
            this.labess.TabIndex = 4;
            this.labess.Text = "Auto-pauser se margem < mínima por (dias):";
            // 
            // nudMinMargin
            // 
            this.nudMinMargin.DecimalPlaces = 2;
            this.nudMinMargin.Location = new System.Drawing.Point(379, 37);
            this.nudMinMargin.Name = "nudMinMargin";
            this.nudMinMargin.Size = new System.Drawing.Size(97, 29);
            this.nudMinMargin.TabIndex = 1;
            this.nudMinMargin.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(26, 40);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(302, 20);
            this.label8.TabIndex = 0;
            this.label8.Text = "Margem bruta mínima após descontos (%):";
            // 
            // grpTeste
            // 
            this.grpTeste.Controls.Add(this.txtMotivo);
            this.grpTeste.Controls.Add(this.label7);
            this.grpTeste.Controls.Add(this.txtResultado);
            this.grpTeste.Controls.Add(this.lblResultado);
            this.grpTeste.Controls.Add(this.btnSimularCarrinho);
            this.grpTeste.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpTeste.Location = new System.Drawing.Point(12, 766);
            this.grpTeste.Name = "grpTeste";
            this.grpTeste.Size = new System.Drawing.Size(1055, 124);
            this.grpTeste.TabIndex = 9;
            this.grpTeste.TabStop = false;
            this.grpTeste.Text = "Teste / Simulação";
            // 
            // txtMotivo
            // 
            this.txtMotivo.Location = new System.Drawing.Point(759, 37);
            this.txtMotivo.Multiline = true;
            this.txtMotivo.Name = "txtMotivo";
            this.txtMotivo.ReadOnly = true;
            this.txtMotivo.Size = new System.Drawing.Size(256, 67);
            this.txtMotivo.TabIndex = 18;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(654, 58);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 20);
            this.label7.TabIndex = 17;
            this.label7.Text = "Motivo:";
            // 
            // txtResultado
            // 
            this.txtResultado.Location = new System.Drawing.Point(379, 37);
            this.txtResultado.Multiline = true;
            this.txtResultado.Name = "txtResultado";
            this.txtResultado.ReadOnly = true;
            this.txtResultado.Size = new System.Drawing.Size(256, 67);
            this.txtResultado.TabIndex = 16;
            // 
            // lblResultado
            // 
            this.lblResultado.AutoSize = true;
            this.lblResultado.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultado.Location = new System.Drawing.Point(274, 58);
            this.lblResultado.Name = "lblResultado";
            this.lblResultado.Size = new System.Drawing.Size(80, 20);
            this.lblResultado.TabIndex = 15;
            this.lblResultado.Text = "Resultado:";
            // 
            // btnSimularCarrinho
            // 
            this.btnSimularCarrinho.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.btnSimularCarrinho.FlatAppearance.BorderSize = 0;
            this.btnSimularCarrinho.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSimularCarrinho.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSimularCarrinho.ForeColor = System.Drawing.Color.White;
            this.btnSimularCarrinho.Location = new System.Drawing.Point(30, 51);
            this.btnSimularCarrinho.Name = "btnSimularCarrinho";
            this.btnSimularCarrinho.Size = new System.Drawing.Size(199, 33);
            this.btnSimularCarrinho.TabIndex = 15;
            this.btnSimularCarrinho.Text = "Simular carrinho…";
            this.btnSimularCarrinho.UseVisualStyleBackColor = false;
            this.btnSimularCarrinho.Click += new System.EventHandler(this.btnSimularCarrinho_Click_1);
            // 
            // grpMetas
            // 
            this.grpMetas.Controls.Add(this.btnSalvarMetas);
            this.grpMetas.Controls.Add(this.nudMargemMinPorVenda);
            this.grpMetas.Controls.Add(this.nudMetaMargemLiquida);
            this.grpMetas.Controls.Add(this.label13);
            this.grpMetas.Controls.Add(this.label14);
            this.grpMetas.Controls.Add(this.nudMetaFaturamento);
            this.grpMetas.Controls.Add(this.label15);
            this.grpMetas.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpMetas.Location = new System.Drawing.Point(7, 623);
            this.grpMetas.Name = "grpMetas";
            this.grpMetas.Size = new System.Drawing.Size(1055, 137);
            this.grpMetas.TabIndex = 10;
            this.grpMetas.TabStop = false;
            this.grpMetas.Text = " Metas do Negócio";
            // 
            // btnSalvarMetas
            // 
            this.btnSalvarMetas.BackColor = System.Drawing.Color.Green;
            this.btnSalvarMetas.FlatAppearance.BorderSize = 0;
            this.btnSalvarMetas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalvarMetas.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalvarMetas.ForeColor = System.Drawing.Color.White;
            this.btnSalvarMetas.Location = new System.Drawing.Point(703, 98);
            this.btnSalvarMetas.Name = "btnSalvarMetas";
            this.btnSalvarMetas.Size = new System.Drawing.Size(132, 33);
            this.btnSalvarMetas.TabIndex = 13;
            this.btnSalvarMetas.Text = "Salvar Metas";
            this.btnSalvarMetas.UseVisualStyleBackColor = false;
            this.btnSalvarMetas.Click += new System.EventHandler(this.btnSalvarMetas_Click_1);
            // 
            // nudMargemMinPorVenda
            // 
            this.nudMargemMinPorVenda.DecimalPlaces = 2;
            this.nudMargemMinPorVenda.Location = new System.Drawing.Point(726, 37);
            this.nudMargemMinPorVenda.Name = "nudMargemMinPorVenda";
            this.nudMargemMinPorVenda.Size = new System.Drawing.Size(97, 29);
            this.nudMargemMinPorVenda.TabIndex = 14;
            this.nudMargemMinPorVenda.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // nudMetaMargemLiquida
            // 
            this.nudMetaMargemLiquida.DecimalPlaces = 2;
            this.nudMetaMargemLiquida.Location = new System.Drawing.Point(284, 85);
            this.nudMetaMargemLiquida.Name = "nudMetaMargemLiquida";
            this.nudMetaMargemLiquida.Size = new System.Drawing.Size(97, 29);
            this.nudMetaMargemLiquida.TabIndex = 13;
            this.nudMetaMargemLiquida.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(26, 88);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(208, 20);
            this.label13.TabIndex = 4;
            this.label13.Text = "Meta de margem líquida (%):";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(472, 40);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(227, 20);
            this.label14.TabIndex = 2;
            this.label14.Text = "Margem mínima por venda (%):";
            // 
            // nudMetaFaturamento
            // 
            this.nudMetaFaturamento.DecimalPlaces = 2;
            this.nudMetaFaturamento.Location = new System.Drawing.Point(284, 37);
            this.nudMetaFaturamento.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudMetaFaturamento.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMetaFaturamento.Name = "nudMetaFaturamento";
            this.nudMetaFaturamento.Size = new System.Drawing.Size(97, 29);
            this.nudMetaFaturamento.TabIndex = 1;
            this.nudMetaFaturamento.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(26, 40);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(240, 20);
            this.label15.TabIndex = 0;
            this.label15.Text = "Meta mensal de faturamento (Kz):";
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(920, 884);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(140, 33);
            this.btnCancelar.TabIndex = 18;
            this.btnCancelar.Text = "Fechar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click_1);
            // 
            // btnSalvarConfig
            // 
            this.btnSalvarConfig.BackColor = System.Drawing.Color.Green;
            this.btnSalvarConfig.FlatAppearance.BorderSize = 0;
            this.btnSalvarConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalvarConfig.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalvarConfig.ForeColor = System.Drawing.Color.White;
            this.btnSalvarConfig.Location = new System.Drawing.Point(696, 884);
            this.btnSalvarConfig.Name = "btnSalvarConfig";
            this.btnSalvarConfig.Size = new System.Drawing.Size(206, 33);
            this.btnSalvarConfig.TabIndex = 17;
            this.btnSalvarConfig.Text = " Salvar Configurações";
            this.btnSalvarConfig.UseVisualStyleBackColor = false;
            this.btnSalvarConfig.Click += new System.EventHandler(this.btnSalvarConfig_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Location = new System.Drawing.Point(7, 910);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 29);
            this.panel2.TabIndex = 19;
            // 
            // frmLoyaltyAndPromos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1106, 605);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnSalvarConfig);
            this.Controls.Add(this.grpMetas);
            this.Controls.Add(this.grpTeste);
            this.Controls.Add(this.grpProtecao);
            this.Controls.Add(this.grpResgate);
            this.Controls.Add(this.grpGanhos);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmLoyaltyAndPromos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.frmLoyaltyAndPromos_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpGanhos.ResumeLayout(false);
            this.grpGanhos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudActivityDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExpireDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKzPerPoint)).EndInit();
            this.grpResgate.ResumeLayout(false);
            this.grpResgate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxPct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPtsPerRedeem)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKzPerRedeem)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinPtsRedeem)).EndInit();
            this.grpProtecao.ResumeLayout(false);
            this.grpProtecao.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoPauseDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinMargin)).EndInit();
            this.grpTeste.ResumeLayout(false);
            this.grpTeste.PerformLayout();
            this.grpMetas.ResumeLayout(false);
            this.grpMetas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMargemMinPorVenda)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetaMargemLiquida)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMetaFaturamento)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpGanhos;
        private System.Windows.Forms.CheckBox chkPromo;
        private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.NumericUpDown nudKzPerPoint;
        private System.Windows.Forms.Label lblKzPerPoint;
        private System.Windows.Forms.NumericUpDown nudExpireDays;
        private System.Windows.Forms.Label lblExpireDays;
        private System.Windows.Forms.Label lblActivityDays;
        private System.Windows.Forms.NumericUpDown nudActivityDays;
        private System.Windows.Forms.GroupBox grpResgate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudKzPerRedeem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudMinPtsRedeem;
        private System.Windows.Forms.Label lblMinPts;
        private System.Windows.Forms.NumericUpDown nudPtsPerRedeem;
        private System.Windows.Forms.NumericUpDown nudMaxPct;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox grpProtecao;
        private System.Windows.Forms.NumericUpDown nudAutoPauseDays;
        private System.Windows.Forms.Label labess;
        private System.Windows.Forms.NumericUpDown nudMinMargin;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.Button btnAutoPauseTest;
        private System.Windows.Forms.Label lblAutoPauseHint;
        private System.Windows.Forms.GroupBox grpTeste;
        private System.Windows.Forms.GroupBox grpMetas;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown nudMetaFaturamento;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown nudMargemMinPorVenda;
        private System.Windows.Forms.NumericUpDown nudMetaMargemLiquida;
        public System.Windows.Forms.Button btnSalvarMetas;
        private System.Windows.Forms.TextBox txtMotivo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtResultado;
        private System.Windows.Forms.Label lblResultado;
        public System.Windows.Forms.Button btnSimularCarrinho;
        public System.Windows.Forms.Button btnCancelar;
        public System.Windows.Forms.Button btnSalvarConfig;
        private System.Windows.Forms.Panel panel2;
    }
}