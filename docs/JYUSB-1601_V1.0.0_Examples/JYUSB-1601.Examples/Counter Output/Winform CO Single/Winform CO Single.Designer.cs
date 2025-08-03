namespace Winform_CO_Single
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.button_Start = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox_paraConfig = new System.Windows.Forms.GroupBox();
            this.comboBox_SlotNumber = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_IdleState = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBox_counterNumber = new System.Windows.Forms.ComboBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.label2 = new System.Windows.Forms.Label();
            this.timer_FetchData = new System.Windows.Forms.Timer(this.components);
            this.button_stop = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox_PulsePara = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDown_PulseCount = new System.Windows.Forms.NumericUpDown();
            this.label_LowLevel = new System.Windows.Forms.Label();
            this.numericUpDown_lowPulseWidth = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_highPulseWidth = new System.Windows.Forms.NumericUpDown();
            this.label_HighLevel = new System.Windows.Forms.Label();
            this.comboBox_pulseType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_TransferedPulses = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox_paraConfig.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox_PulsePara.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PulseCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_lowPulseWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_highPulseWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Start
            // 
            this.button_Start.Font = new System.Drawing.Font("宋体", 10.5F);
            this.button_Start.Location = new System.Drawing.Point(135, 51);
            this.button_Start.Name = "button_Start";
            this.button_Start.Size = new System.Drawing.Size(92, 43);
            this.button_Start.TabIndex = 88;
            this.button_Start.Tag = "ParaConfig";
            this.button_Start.Text = "start";
            this.button_Start.UseVisualStyleBackColor = true;
            this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label1.Font = new System.Drawing.Font("宋体", 23F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(-108, -105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(376, 31);
            this.label1.TabIndex = 86;
            this.label1.Text = "JYUSB61902连续脉冲生成";
            // 
            // groupBox_paraConfig
            // 
            this.groupBox_paraConfig.Controls.Add(this.comboBox_SlotNumber);
            this.groupBox_paraConfig.Controls.Add(this.label9);
            this.groupBox_paraConfig.Controls.Add(this.comboBox_IdleState);
            this.groupBox_paraConfig.Controls.Add(this.label10);
            this.groupBox_paraConfig.Controls.Add(this.label8);
            this.groupBox_paraConfig.Controls.Add(this.comboBox_counterNumber);
            this.groupBox_paraConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_paraConfig.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_paraConfig.Location = new System.Drawing.Point(0, 0);
            this.groupBox_paraConfig.Name = "groupBox_paraConfig";
            this.groupBox_paraConfig.Size = new System.Drawing.Size(606, 106);
            this.groupBox_paraConfig.TabIndex = 92;
            this.groupBox_paraConfig.TabStop = false;
            this.groupBox_paraConfig.Text = "Basic Param Configuration";
            // 
            // comboBox_SlotNumber
            // 
            this.comboBox_SlotNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SlotNumber.Font = new System.Drawing.Font("宋体", 10.5F);
            this.comboBox_SlotNumber.FormattingEnabled = true;
            this.comboBox_SlotNumber.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18"});
            this.comboBox_SlotNumber.Location = new System.Drawing.Point(135, 22);
            this.comboBox_SlotNumber.Name = "comboBox_SlotNumber";
            this.comboBox_SlotNumber.Size = new System.Drawing.Size(114, 22);
            this.comboBox_SlotNumber.TabIndex = 180;
            this.comboBox_SlotNumber.Tag = "ParaConfig";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(10, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 14);
            this.label9.TabIndex = 68;
            this.label9.Text = "Counter ID";
            // 
            // comboBox_IdleState
            // 
            this.comboBox_IdleState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_IdleState.FormattingEnabled = true;
            this.comboBox_IdleState.Location = new System.Drawing.Point(355, 23);
            this.comboBox_IdleState.Name = "comboBox_IdleState";
            this.comboBox_IdleState.Size = new System.Drawing.Size(123, 22);
            this.comboBox_IdleState.TabIndex = 79;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(10, 23);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(91, 14);
            this.label10.TabIndex = 66;
            this.label10.Text = "Slot Number ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(259, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 14);
            this.label8.TabIndex = 78;
            this.label8.Text = "Idle State";
            // 
            // comboBox_counterNumber
            // 
            this.comboBox_counterNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_counterNumber.FormattingEnabled = true;
            this.comboBox_counterNumber.Location = new System.Drawing.Point(135, 52);
            this.comboBox_counterNumber.Name = "comboBox_counterNumber";
            this.comboBox_counterNumber.Size = new System.Drawing.Size(114, 22);
            this.comboBox_counterNumber.TabIndex = 67;
            this.comboBox_counterNumber.Tag = "ParaConfig";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(606, 80);
            this.splitter1.TabIndex = 94;
            this.splitter1.TabStop = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoEllipsis = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label2.Font = new System.Drawing.Font("宋体", 23F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(582, 31);
            this.label2.TabIndex = 95;
            this.label2.Text = "JYUSB1601 Single Pulse Generation";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer_FetchData
            // 
            this.timer_FetchData.Tick += new System.EventHandler(this.timer_FetchData_Tick);
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.5F);
            this.button_stop.Location = new System.Drawing.Point(342, 51);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(87, 43);
            this.button_stop.TabIndex = 112;
            this.button_stop.Tag = "ParaConfig";
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_Start);
            this.groupBox1.Controls.Add(this.button_stop);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(606, 164);
            this.groupBox1.TabIndex = 113;
            this.groupBox1.TabStop = false;
            // 
            // groupBox_PulsePara
            // 
            this.groupBox_PulsePara.Controls.Add(this.label7);
            this.groupBox_PulsePara.Controls.Add(this.numericUpDown_PulseCount);
            this.groupBox_PulsePara.Controls.Add(this.label_LowLevel);
            this.groupBox_PulsePara.Controls.Add(this.numericUpDown_lowPulseWidth);
            this.groupBox_PulsePara.Controls.Add(this.numericUpDown_highPulseWidth);
            this.groupBox_PulsePara.Controls.Add(this.label_HighLevel);
            this.groupBox_PulsePara.Controls.Add(this.comboBox_pulseType);
            this.groupBox_PulsePara.Controls.Add(this.label3);
            this.groupBox_PulsePara.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_PulsePara.Location = new System.Drawing.Point(0, 0);
            this.groupBox_PulsePara.Name = "groupBox_PulsePara";
            this.groupBox_PulsePara.Size = new System.Drawing.Size(606, 136);
            this.groupBox_PulsePara.TabIndex = 94;
            this.groupBox_PulsePara.TabStop = false;
            this.groupBox_PulsePara.Text = "Pulse Parameter";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label7.Location = new System.Drawing.Point(12, 79);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(119, 14);
            this.label7.TabIndex = 87;
            this.label7.Text = "Number of Pulses";
            // 
            // numericUpDown_PulseCount
            // 
            this.numericUpDown_PulseCount.Location = new System.Drawing.Point(135, 75);
            this.numericUpDown_PulseCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.numericUpDown_PulseCount.Maximum = new decimal(new int[] {
            -2,
            0,
            0,
            0});
            this.numericUpDown_PulseCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_PulseCount.Name = "numericUpDown_PulseCount";
            this.numericUpDown_PulseCount.Size = new System.Drawing.Size(159, 23);
            this.numericUpDown_PulseCount.TabIndex = 86;
            this.numericUpDown_PulseCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label_LowLevel
            // 
            this.label_LowLevel.AutoSize = true;
            this.label_LowLevel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_LowLevel.Location = new System.Drawing.Point(315, 59);
            this.label_LowLevel.Name = "label_LowLevel";
            this.label_LowLevel.Size = new System.Drawing.Size(126, 14);
            this.label_LowLevel.TabIndex = 85;
            this.label_LowLevel.Text = "Low Level Time(s)";
            // 
            // numericUpDown_lowPulseWidth
            // 
            this.numericUpDown_lowPulseWidth.DecimalPlaces = 3;
            this.numericUpDown_lowPulseWidth.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDown_lowPulseWidth.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
            this.numericUpDown_lowPulseWidth.Location = new System.Drawing.Point(315, 80);
            this.numericUpDown_lowPulseWidth.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_lowPulseWidth.Name = "numericUpDown_lowPulseWidth";
            this.numericUpDown_lowPulseWidth.Size = new System.Drawing.Size(161, 21);
            this.numericUpDown_lowPulseWidth.TabIndex = 84;
            this.numericUpDown_lowPulseWidth.Tag = "ParaConfig";
            this.numericUpDown_lowPulseWidth.Value = new decimal(new int[] {
            5,
            0,
            0,
            262144});
            // 
            // numericUpDown_highPulseWidth
            // 
            this.numericUpDown_highPulseWidth.DecimalPlaces = 3;
            this.numericUpDown_highPulseWidth.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDown_highPulseWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.numericUpDown_highPulseWidth.Location = new System.Drawing.Point(315, 31);
            this.numericUpDown_highPulseWidth.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_highPulseWidth.Name = "numericUpDown_highPulseWidth";
            this.numericUpDown_highPulseWidth.Size = new System.Drawing.Size(161, 21);
            this.numericUpDown_highPulseWidth.TabIndex = 83;
            this.numericUpDown_highPulseWidth.Tag = "ParaConfig";
            this.numericUpDown_highPulseWidth.Value = new decimal(new int[] {
            5,
            0,
            0,
            262144});
            // 
            // label_HighLevel
            // 
            this.label_HighLevel.AutoSize = true;
            this.label_HighLevel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_HighLevel.Location = new System.Drawing.Point(315, 12);
            this.label_HighLevel.Name = "label_HighLevel";
            this.label_HighLevel.Size = new System.Drawing.Size(133, 14);
            this.label_HighLevel.TabIndex = 82;
            this.label_HighLevel.Text = "High Level Time(s)";
            // 
            // comboBox_pulseType
            // 
            this.comboBox_pulseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_pulseType.FormattingEnabled = true;
            this.comboBox_pulseType.Location = new System.Drawing.Point(135, 30);
            this.comboBox_pulseType.Name = "comboBox_pulseType";
            this.comboBox_pulseType.Size = new System.Drawing.Size(159, 22);
            this.comboBox_pulseType.TabIndex = 81;
            this.comboBox_pulseType.SelectedIndexChanged += new System.EventHandler(this.comboBox_pulseType_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 14);
            this.label3.TabIndex = 80;
            this.label3.Text = "OutputPulse Type";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 80);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Size = new System.Drawing.Size(606, 441);
            this.splitContainer1.SplitterDistance = 556;
            this.splitContainer1.TabIndex = 123;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox_paraConfig);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(606, 433);
            this.splitContainer2.SplitterDistance = 106;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.groupBox_PulsePara);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer3.Size = new System.Drawing.Size(606, 303);
            this.splitContainer3.SplitterDistance = 135;
            this.splitContainer3.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_TransferedPulses});
            this.statusStrip1.Location = new System.Drawing.Point(0, 499);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(606, 22);
            this.statusStrip1.TabIndex = 124;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_TransferedPulses
            // 
            this.toolStripStatusLabel_TransferedPulses.Name = "toolStripStatusLabel_TransferedPulses";
            this.toolStripStatusLabel_TransferedPulses.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 521);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 Single Pulse Generation";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox_paraConfig.ResumeLayout(false);
            this.groupBox_paraConfig.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox_PulsePara.ResumeLayout(false);
            this.groupBox_PulsePara.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PulseCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_lowPulseWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_highPulseWidth)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_Start;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox_paraConfig;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBox_counterNumber;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer_FetchData;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox_PulsePara;
        private System.Windows.Forms.Label label_LowLevel;
        private System.Windows.Forms.NumericUpDown numericUpDown_lowPulseWidth;
        private System.Windows.Forms.NumericUpDown numericUpDown_highPulseWidth;
        private System.Windows.Forms.Label label_HighLevel;
        private System.Windows.Forms.ComboBox comboBox_pulseType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_IdleState;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ComboBox comboBox_SlotNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDown_PulseCount;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_TransferedPulses;
    }
}

