namespace Winform_DO_SinglePoint
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox_ParamConfig = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.comboBox_SoltNumber = new System.Windows.Forms.ComboBox();
            this.button_start = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.groupBox_port0 = new System.Windows.Forms.GroupBox();
            this.industrySwitch7 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.button_stop = new System.Windows.Forms.Button();
            this.industrySwitch15 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch5 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch13 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch6 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch14 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch1 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch9 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch4 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch3 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch12 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch2 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch11 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch0 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch10 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.industrySwitch8 = new SeeSharpTools.JY.GUI.IndustrySwitch();
            this.label34 = new System.Windows.Forms.Label();
            this.checkedListBox_lineChoose = new System.Windows.Forms.CheckedListBox();
            this.groupBox_do = new System.Windows.Forms.GroupBox();
            this.groupBox_ParamConfig.SuspendLayout();
            this.groupBox_port0.SuspendLayout();
            this.groupBox_do.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1187, 80);
            this.splitter1.TabIndex = 81;
            this.splitter1.TabStop = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoEllipsis = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label2.Font = new System.Drawing.Font("SimSun", 20F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(42, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1119, 31);
            this.label2.TabIndex = 82;
            this.label2.Text = "JYUSB1601 Single Digital Output";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox_ParamConfig
            // 
            this.groupBox_ParamConfig.Controls.Add(this.label13);
            this.groupBox_ParamConfig.Controls.Add(this.comboBox_SoltNumber);
            this.groupBox_ParamConfig.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_ParamConfig.Location = new System.Drawing.Point(12, 109);
            this.groupBox_ParamConfig.Name = "groupBox_ParamConfig";
            this.groupBox_ParamConfig.Size = new System.Drawing.Size(237, 112);
            this.groupBox_ParamConfig.TabIndex = 83;
            this.groupBox_ParamConfig.TabStop = false;
            this.groupBox_ParamConfig.Text = "Basic Param Configuration";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(6, 65);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(91, 14);
            this.label13.TabIndex = 66;
            this.label13.Text = "Slot Number ";
            // 
            // comboBox_SoltNumber
            // 
            this.comboBox_SoltNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SoltNumber.FormattingEnabled = true;
            this.comboBox_SoltNumber.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7"});
            this.comboBox_SoltNumber.Location = new System.Drawing.Point(107, 62);
            this.comboBox_SoltNumber.Name = "comboBox_SoltNumber";
            this.comboBox_SoltNumber.Size = new System.Drawing.Size(91, 22);
            this.comboBox_SoltNumber.TabIndex = 65;
            this.comboBox_SoltNumber.Tag = "ParaConfig";
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Location = new System.Drawing.Point(306, 420);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(88, 55);
            this.button_start.TabIndex = 85;
            this.button_start.Tag = "ParaConfig";
            this.button_start.Text = "Start";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(10, 54);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(112, 14);
            this.label20.TabIndex = 2;
            this.label20.Text = "LowLevel(false)";
            // 
            // groupBox_port0
            // 
            this.groupBox_port0.Controls.Add(this.groupBox_do);
            this.groupBox_port0.Controls.Add(this.button_stop);
            this.groupBox_port0.Controls.Add(this.button_start);
            this.groupBox_port0.Controls.Add(this.label34);
            this.groupBox_port0.Controls.Add(this.label20);
            this.groupBox_port0.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_port0.Location = new System.Drawing.Point(277, 97);
            this.groupBox_port0.Name = "groupBox_port0";
            this.groupBox_port0.Size = new System.Drawing.Size(1117, 555);
            this.groupBox_port0.TabIndex = 93;
            this.groupBox_port0.TabStop = false;
            this.groupBox_port0.Text = "Line(0~7)";
            // 
            // industrySwitch7
            // 
            this.industrySwitch7.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch7.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch7.Location = new System.Drawing.Point(740, 9);
            this.industrySwitch7.Name = "industrySwitch7";
            this.industrySwitch7.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch7.OffText = "OFF";
            this.industrySwitch7.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch7.OnText = "ON";
            this.industrySwitch7.ShowText = false;
            this.industrySwitch7.Size = new System.Drawing.Size(94, 101);
            this.industrySwitch7.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch7.TabIndex = 3;
            this.industrySwitch7.Value = false;
            this.industrySwitch7.ValueChanged += new System.EventHandler(this.industrySwitch7_ValueChanged);
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(464, 420);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(88, 55);
            this.button_stop.TabIndex = 107;
            this.button_stop.Tag = "ParaConfig";
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // industrySwitch15
            // 
            this.industrySwitch15.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch15.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch15.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch15.Location = new System.Drawing.Point(743, 145);
            this.industrySwitch15.Name = "industrySwitch15";
            this.industrySwitch15.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch15.OffText = "OFF";
            this.industrySwitch15.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch15.OnText = "ON";
            this.industrySwitch15.ShowText = false;
            this.industrySwitch15.Size = new System.Drawing.Size(94, 100);
            this.industrySwitch15.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch15.TabIndex = 3;
            this.industrySwitch15.Value = false;
            this.industrySwitch15.ValueChanged += new System.EventHandler(this.industrySwitch15_ValueChanged);
            // 
            // industrySwitch5
            // 
            this.industrySwitch5.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch5.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch5.Location = new System.Drawing.Point(533, 9);
            this.industrySwitch5.Name = "industrySwitch5";
            this.industrySwitch5.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch5.OffText = "OFF";
            this.industrySwitch5.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch5.OnText = "ON";
            this.industrySwitch5.ShowText = false;
            this.industrySwitch5.Size = new System.Drawing.Size(85, 101);
            this.industrySwitch5.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch5.TabIndex = 3;
            this.industrySwitch5.Value = false;
            this.industrySwitch5.ValueChanged += new System.EventHandler(this.industrySwitch5_ValueChanged);
            // 
            // industrySwitch13
            // 
            this.industrySwitch13.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch13.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch13.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch13.Location = new System.Drawing.Point(559, 145);
            this.industrySwitch13.Name = "industrySwitch13";
            this.industrySwitch13.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch13.OffText = "OFF";
            this.industrySwitch13.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch13.OnText = "ON";
            this.industrySwitch13.ShowText = false;
            this.industrySwitch13.Size = new System.Drawing.Size(85, 100);
            this.industrySwitch13.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch13.TabIndex = 3;
            this.industrySwitch13.Value = false;
            this.industrySwitch13.ValueChanged += new System.EventHandler(this.industrySwitch13_ValueChanged);
            // 
            // industrySwitch6
            // 
            this.industrySwitch6.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch6.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch6.Location = new System.Drawing.Point(636, 9);
            this.industrySwitch6.Name = "industrySwitch6";
            this.industrySwitch6.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch6.OffText = "OFF";
            this.industrySwitch6.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch6.OnText = "ON";
            this.industrySwitch6.ShowText = false;
            this.industrySwitch6.Size = new System.Drawing.Size(98, 101);
            this.industrySwitch6.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch6.TabIndex = 3;
            this.industrySwitch6.Value = false;
            this.industrySwitch6.ValueChanged += new System.EventHandler(this.industrySwitch6_ValueChanged);
            // 
            // industrySwitch14
            // 
            this.industrySwitch14.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch14.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch14.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch14.Location = new System.Drawing.Point(650, 145);
            this.industrySwitch14.Name = "industrySwitch14";
            this.industrySwitch14.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch14.OffText = "OFF";
            this.industrySwitch14.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch14.OnText = "ON";
            this.industrySwitch14.ShowText = false;
            this.industrySwitch14.Size = new System.Drawing.Size(84, 100);
            this.industrySwitch14.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch14.TabIndex = 3;
            this.industrySwitch14.Value = false;
            this.industrySwitch14.ValueChanged += new System.EventHandler(this.industrySwitch14_ValueChanged);
            // 
            // industrySwitch1
            // 
            this.industrySwitch1.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch1.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch1.Location = new System.Drawing.Point(137, 9);
            this.industrySwitch1.Name = "industrySwitch1";
            this.industrySwitch1.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch1.OffText = "OFF";
            this.industrySwitch1.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch1.OnText = "ON";
            this.industrySwitch1.ShowText = false;
            this.industrySwitch1.Size = new System.Drawing.Size(86, 101);
            this.industrySwitch1.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch1.TabIndex = 3;
            this.industrySwitch1.Value = false;
            this.industrySwitch1.ValueChanged += new System.EventHandler(this.industrySwitch1_ValueChanged);
            // 
            // industrySwitch9
            // 
            this.industrySwitch9.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch9.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch9.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch9.Location = new System.Drawing.Point(137, 145);
            this.industrySwitch9.Name = "industrySwitch9";
            this.industrySwitch9.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch9.OffText = "OFF";
            this.industrySwitch9.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch9.OnText = "ON";
            this.industrySwitch9.ShowText = false;
            this.industrySwitch9.Size = new System.Drawing.Size(86, 100);
            this.industrySwitch9.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch9.TabIndex = 3;
            this.industrySwitch9.Value = false;
            this.industrySwitch9.ValueChanged += new System.EventHandler(this.industrySwitch9_ValueChanged);
            // 
            // industrySwitch4
            // 
            this.industrySwitch4.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch4.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch4.Location = new System.Drawing.Point(438, 9);
            this.industrySwitch4.Name = "industrySwitch4";
            this.industrySwitch4.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch4.OffText = "OFF";
            this.industrySwitch4.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch4.OnText = "ON";
            this.industrySwitch4.ShowText = false;
            this.industrySwitch4.Size = new System.Drawing.Size(89, 101);
            this.industrySwitch4.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch4.TabIndex = 3;
            this.industrySwitch4.Value = false;
            this.industrySwitch4.ValueChanged += new System.EventHandler(this.industrySwitch4_ValueChanged);
            // 
            // industrySwitch3
            // 
            this.industrySwitch3.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch3.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch3.Location = new System.Drawing.Point(334, 9);
            this.industrySwitch3.Name = "industrySwitch3";
            this.industrySwitch3.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch3.OffText = "OFF";
            this.industrySwitch3.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch3.OnText = "ON";
            this.industrySwitch3.ShowText = false;
            this.industrySwitch3.Size = new System.Drawing.Size(89, 101);
            this.industrySwitch3.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch3.TabIndex = 3;
            this.industrySwitch3.Value = false;
            this.industrySwitch3.ValueChanged += new System.EventHandler(this.industrySwitch3_ValueChanged);
            // 
            // industrySwitch12
            // 
            this.industrySwitch12.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch12.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch12.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch12.Location = new System.Drawing.Point(440, 145);
            this.industrySwitch12.Name = "industrySwitch12";
            this.industrySwitch12.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch12.OffText = "OFF";
            this.industrySwitch12.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch12.OnText = "ON";
            this.industrySwitch12.ShowText = false;
            this.industrySwitch12.Size = new System.Drawing.Size(98, 100);
            this.industrySwitch12.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch12.TabIndex = 3;
            this.industrySwitch12.Value = false;
            this.industrySwitch12.ValueChanged += new System.EventHandler(this.industrySwitch12_ValueChanged);
            // 
            // industrySwitch2
            // 
            this.industrySwitch2.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch2.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch2.Location = new System.Drawing.Point(239, 9);
            this.industrySwitch2.Name = "industrySwitch2";
            this.industrySwitch2.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch2.OffText = "OFF";
            this.industrySwitch2.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch2.OnText = "ON";
            this.industrySwitch2.ShowText = false;
            this.industrySwitch2.Size = new System.Drawing.Size(89, 101);
            this.industrySwitch2.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch2.TabIndex = 3;
            this.industrySwitch2.Value = false;
            this.industrySwitch2.ValueChanged += new System.EventHandler(this.industrySwitch2_ValueChanged);
            // 
            // industrySwitch11
            // 
            this.industrySwitch11.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch11.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch11.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch11.Location = new System.Drawing.Point(334, 145);
            this.industrySwitch11.Name = "industrySwitch11";
            this.industrySwitch11.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch11.OffText = "OFF";
            this.industrySwitch11.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch11.OnText = "ON";
            this.industrySwitch11.ShowText = false;
            this.industrySwitch11.Size = new System.Drawing.Size(91, 100);
            this.industrySwitch11.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch11.TabIndex = 3;
            this.industrySwitch11.Value = false;
            this.industrySwitch11.ValueChanged += new System.EventHandler(this.industrySwitch11_ValueChanged);
            // 
            // industrySwitch0
            // 
            this.industrySwitch0.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch0.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch0.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch0.Location = new System.Drawing.Point(33, 9);
            this.industrySwitch0.Name = "industrySwitch0";
            this.industrySwitch0.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch0.OffText = "OFF";
            this.industrySwitch0.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch0.OnText = "ON";
            this.industrySwitch0.ShowText = false;
            this.industrySwitch0.Size = new System.Drawing.Size(86, 101);
            this.industrySwitch0.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch0.TabIndex = 3;
            this.industrySwitch0.Value = false;
            this.industrySwitch0.ValueChanged += new System.EventHandler(this.industrySwitch0_ValueChanged);
            // 
            // industrySwitch10
            // 
            this.industrySwitch10.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch10.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch10.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch10.Location = new System.Drawing.Point(236, 145);
            this.industrySwitch10.Name = "industrySwitch10";
            this.industrySwitch10.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch10.OffText = "OFF";
            this.industrySwitch10.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch10.OnText = "ON";
            this.industrySwitch10.ShowText = false;
            this.industrySwitch10.Size = new System.Drawing.Size(92, 100);
            this.industrySwitch10.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch10.TabIndex = 3;
            this.industrySwitch10.Value = false;
            this.industrySwitch10.ValueChanged += new System.EventHandler(this.industrySwitch10_ValueChanged);
            // 
            // industrySwitch8
            // 
            this.industrySwitch8.BackColor = System.Drawing.Color.Transparent;
            this.industrySwitch8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.industrySwitch8.Interacton = SeeSharpTools.JY.GUI.IndustrySwitch.InteractionStyle.SwitchWhenPressed;
            this.industrySwitch8.Location = new System.Drawing.Point(33, 145);
            this.industrySwitch8.Name = "industrySwitch8";
            this.industrySwitch8.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch8.OffText = "OFF";
            this.industrySwitch8.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(180)))), ((int)(((byte)(205)))));
            this.industrySwitch8.OnText = "ON";
            this.industrySwitch8.ShowText = false;
            this.industrySwitch8.Size = new System.Drawing.Size(86, 100);
            this.industrySwitch8.Style = SeeSharpTools.JY.GUI.IndustrySwitch.SwitchStyles.Vertical;
            this.industrySwitch8.TabIndex = 3;
            this.industrySwitch8.Value = false;
            this.industrySwitch8.ValueChanged += new System.EventHandler(this.industrySwitch8_ValueChanged);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(10, 29);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(112, 14);
            this.label34.TabIndex = 1;
            this.label34.Text = "HighLevel(true)";
            // 
            // checkedListBox_lineChoose
            // 
            this.checkedListBox_lineChoose.CheckOnClick = true;
            this.checkedListBox_lineChoose.FormattingEnabled = true;
            this.checkedListBox_lineChoose.Location = new System.Drawing.Point(21, 229);
            this.checkedListBox_lineChoose.Name = "checkedListBox_lineChoose";
            this.checkedListBox_lineChoose.Size = new System.Drawing.Size(228, 356);
            this.checkedListBox_lineChoose.TabIndex = 195;
            // 
            // groupBox_do
            // 
            this.groupBox_do.Controls.Add(this.industrySwitch7);
            this.groupBox_do.Controls.Add(this.industrySwitch15);
            this.groupBox_do.Controls.Add(this.industrySwitch5);
            this.groupBox_do.Controls.Add(this.industrySwitch13);
            this.groupBox_do.Controls.Add(this.industrySwitch6);
            this.groupBox_do.Controls.Add(this.industrySwitch14);
            this.groupBox_do.Controls.Add(this.industrySwitch1);
            this.groupBox_do.Controls.Add(this.industrySwitch9);
            this.groupBox_do.Controls.Add(this.industrySwitch4);
            this.groupBox_do.Controls.Add(this.industrySwitch3);
            this.groupBox_do.Controls.Add(this.industrySwitch12);
            this.groupBox_do.Controls.Add(this.industrySwitch2);
            this.groupBox_do.Controls.Add(this.industrySwitch11);
            this.groupBox_do.Controls.Add(this.industrySwitch0);
            this.groupBox_do.Controls.Add(this.industrySwitch10);
            this.groupBox_do.Controls.Add(this.industrySwitch8);
            this.groupBox_do.Location = new System.Drawing.Point(14, 79);
            this.groupBox_do.Name = "groupBox_do";
            this.groupBox_do.Size = new System.Drawing.Size(869, 304);
            this.groupBox_do.TabIndex = 108;
            this.groupBox_do.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1187, 664);
            this.Controls.Add(this.checkedListBox_lineChoose);
            this.Controls.Add(this.groupBox_port0);
            this.Controls.Add(this.groupBox_ParamConfig);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.splitter1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 Single Digital Output";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox_ParamConfig.ResumeLayout(false);
            this.groupBox_ParamConfig.PerformLayout();
            this.groupBox_port0.ResumeLayout(false);
            this.groupBox_port0.PerformLayout();
            this.groupBox_do.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox_ParamConfig;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox comboBox_SoltNumber;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.GroupBox groupBox_port0;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Button button_stop;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch7;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch15;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch5;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch13;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch6;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch14;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch1;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch9;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch4;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch3;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch12;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch2;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch11;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch0;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch10;
        private SeeSharpTools.JY.GUI.IndustrySwitch industrySwitch8;
        private System.Windows.Forms.CheckedListBox checkedListBox_lineChoose;
        private System.Windows.Forms.GroupBox groupBox_do;
    }
}

