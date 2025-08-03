namespace SeeSharpExample.JY.JY5310
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
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.timer_FetchData = new System.Windows.Forms.Timer(this.components);
            this.button_stop = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.groupBox_genParam = new System.Windows.Forms.GroupBox();
            this.groupBox_channel = new System.Windows.Forms.GroupBox();
            this.checkBox_selectchannel = new System.Windows.Forms.CheckBox();
            this.checkedListBox_portChoose = new System.Windows.Forms.CheckedListBox();
            this.groupBox_button = new System.Windows.Forms.GroupBox();
            this.groupBox_param = new System.Windows.Forms.GroupBox();
            this.comboBox_boardNumber = new System.Windows.Forms.ComboBox();
            this.comboBox_inputRange = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView_waveConfigure = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.groupBox_genParam.SuspendLayout();
            this.groupBox_channel.SuspendLayout();
            this.groupBox_button.SuspendLayout();
            this.groupBox_param.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_waveConfigure)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 619);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1153, 22);
            this.statusStrip1.TabIndex = 82;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1153, 80);
            this.splitter1.TabIndex = 80;
            this.splitter1.TabStop = false;
            // 
            // timer_FetchData
            // 
            this.timer_FetchData.Interval = 10;
            this.timer_FetchData.Tick += new System.EventHandler(this.timer_FetchData_Tick);
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(167, 36);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(98, 30);
            this.button_stop.TabIndex = 78;
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Location = new System.Drawing.Point(7, 36);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(119, 30);
            this.button_start.TabIndex = 77;
            this.button_start.Tag = "ParaConfig";
            this.button_start.Text = "Start";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // groupBox_genParam
            // 
            this.groupBox_genParam.Controls.Add(this.groupBox_channel);
            this.groupBox_genParam.Controls.Add(this.groupBox_button);
            this.groupBox_genParam.Controls.Add(this.groupBox_param);
            this.groupBox_genParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_genParam.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_genParam.Location = new System.Drawing.Point(0, 0);
            this.groupBox_genParam.Name = "groupBox_genParam";
            this.groupBox_genParam.Size = new System.Drawing.Size(486, 527);
            this.groupBox_genParam.TabIndex = 84;
            this.groupBox_genParam.TabStop = false;
            this.groupBox_genParam.Text = "Basic Param Configuration";
            // 
            // groupBox_channel
            // 
            this.groupBox_channel.Controls.Add(this.checkBox_selectchannel);
            this.groupBox_channel.Controls.Add(this.checkedListBox_portChoose);
            this.groupBox_channel.Location = new System.Drawing.Point(8, 19);
            this.groupBox_channel.Name = "groupBox_channel";
            this.groupBox_channel.Size = new System.Drawing.Size(151, 354);
            this.groupBox_channel.TabIndex = 198;
            this.groupBox_channel.TabStop = false;
            // 
            // checkBox_selectchannel
            // 
            this.checkBox_selectchannel.AutoSize = true;
            this.checkBox_selectchannel.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBox_selectchannel.Location = new System.Drawing.Point(11, 314);
            this.checkBox_selectchannel.Name = "checkBox_selectchannel";
            this.checkBox_selectchannel.Size = new System.Drawing.Size(128, 23);
            this.checkBox_selectchannel.TabIndex = 195;
            this.checkBox_selectchannel.Text = "select all";
            this.checkBox_selectchannel.UseVisualStyleBackColor = true;
            this.checkBox_selectchannel.CheckedChanged += new System.EventHandler(this.checkBox_selectchannel_CheckedChanged);
            // 
            // checkedListBox_portChoose
            // 
            this.checkedListBox_portChoose.CheckOnClick = true;
            this.checkedListBox_portChoose.FormattingEnabled = true;
            this.checkedListBox_portChoose.Items.AddRange(new object[] {
            "channel_0",
            "channel_1",
            "channel_2",
            "channel_3",
            "channel_4",
            "channel_5",
            "channel_6",
            "channel_7",
            "channel_8",
            "channel_9",
            "channel_10",
            "channel_11",
            "channel_12",
            "channel_13",
            "channel_14",
            "channel_15",
            "channel_16",
            "channel_17",
            "channel_18",
            "channel_19",
            "channel_20",
            "channel_21",
            "channel_22",
            "channel_23",
            "channel_24",
            "channel_25",
            "channel_26",
            "channel_27",
            "channel_28",
            "channel_29",
            "channel_30",
            "channel_31"});
            this.checkedListBox_portChoose.Location = new System.Drawing.Point(11, 11);
            this.checkedListBox_portChoose.Name = "checkedListBox_portChoose";
            this.checkedListBox_portChoose.Size = new System.Drawing.Size(118, 274);
            this.checkedListBox_portChoose.TabIndex = 194;
            // 
            // groupBox_button
            // 
            this.groupBox_button.Controls.Add(this.button_start);
            this.groupBox_button.Controls.Add(this.button_stop);
            this.groupBox_button.Location = new System.Drawing.Point(188, 275);
            this.groupBox_button.Name = "groupBox_button";
            this.groupBox_button.Size = new System.Drawing.Size(271, 99);
            this.groupBox_button.TabIndex = 197;
            this.groupBox_button.TabStop = false;
            // 
            // groupBox_param
            // 
            this.groupBox_param.Controls.Add(this.comboBox_boardNumber);
            this.groupBox_param.Controls.Add(this.comboBox_inputRange);
            this.groupBox_param.Controls.Add(this.label8);
            this.groupBox_param.Controls.Add(this.label7);
            this.groupBox_param.Location = new System.Drawing.Point(188, 19);
            this.groupBox_param.Name = "groupBox_param";
            this.groupBox_param.Size = new System.Drawing.Size(274, 250);
            this.groupBox_param.TabIndex = 196;
            this.groupBox_param.TabStop = false;
            // 
            // comboBox_boardNumber
            // 
            this.comboBox_boardNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_boardNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_boardNumber.FormattingEnabled = true;
            this.comboBox_boardNumber.Items.AddRange(new object[] {
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
            this.comboBox_boardNumber.Location = new System.Drawing.Point(146, 21);
            this.comboBox_boardNumber.Name = "comboBox_boardNumber";
            this.comboBox_boardNumber.Size = new System.Drawing.Size(117, 20);
            this.comboBox_boardNumber.TabIndex = 78;
            this.comboBox_boardNumber.Tag = "ParaConfig";
            // 
            // comboBox_inputRange
            // 
            this.comboBox_inputRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_inputRange.FormattingEnabled = true;
            this.comboBox_inputRange.Location = new System.Drawing.Point(146, 62);
            this.comboBox_inputRange.Name = "comboBox_inputRange";
            this.comboBox_inputRange.Size = new System.Drawing.Size(117, 22);
            this.comboBox_inputRange.TabIndex = 77;
            this.comboBox_inputRange.SelectedIndexChanged += new System.EventHandler(this.comboBox_inputRange_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(6, 70);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(84, 14);
            this.label8.TabIndex = 76;
            this.label8.Text = "Input Range";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(6, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 14);
            this.label7.TabIndex = 64;
            this.label7.Text = "Slot Number ";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.dataGridView_waveConfigure);
            this.panel1.Location = new System.Drawing.Point(12, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(629, 530);
            this.panel1.TabIndex = 85;
            // 
            // dataGridView_waveConfigure
            // 
            this.dataGridView_waveConfigure.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_waveConfigure.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.dataGridView_waveConfigure.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_waveConfigure.Name = "dataGridView_waveConfigure";
            this.dataGridView_waveConfigure.RowTemplate.Height = 23;
            this.dataGridView_waveConfigure.Size = new System.Drawing.Size(672, 383);
            this.dataGridView_waveConfigure.TabIndex = 0;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "ChannelNumber";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.HeaderText = "ReadValue";
            this.Column2.Name = "Column2";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.groupBox_genParam);
            this.panel2.Location = new System.Drawing.Point(663, 89);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(486, 527);
            this.panel2.TabIndex = 86;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label1.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(187, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(861, 34);
            this.label1.TabIndex = 88;
            this.label1.Text = "JYUSB1601 MultiChannel Single Data Acquisition ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1153, 641);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitter1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 MultiChannel Single Data Acquisition ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox_genParam.ResumeLayout(false);
            this.groupBox_channel.ResumeLayout(false);
            this.groupBox_channel.PerformLayout();
            this.groupBox_button.ResumeLayout(false);
            this.groupBox_param.ResumeLayout(false);
            this.groupBox_param.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_waveConfigure)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Timer timer_FetchData;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.GroupBox groupBox_genParam;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox checkedListBox_portChoose;
        private System.Windows.Forms.CheckBox checkBox_selectchannel;
        private System.Windows.Forms.GroupBox groupBox_channel;
        private System.Windows.Forms.GroupBox groupBox_button;
        private System.Windows.Forms.GroupBox groupBox_param;
        private System.Windows.Forms.ComboBox comboBox_inputRange;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBox_boardNumber;
        private System.Windows.Forms.DataGridView dataGridView_waveConfigure;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
    }
}

