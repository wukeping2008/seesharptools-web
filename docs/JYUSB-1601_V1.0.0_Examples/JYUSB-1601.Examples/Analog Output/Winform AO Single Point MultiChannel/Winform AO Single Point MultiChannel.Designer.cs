namespace Winform_AO_Single_Point_MultiChannel
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
            this.label1 = new System.Windows.Forms.Label();
            this.button_start = new System.Windows.Forms.Button();
            this.dataGridView_waveConfigure = new System.Windows.Forms.DataGridView();
            this.groupBox_parameter = new System.Windows.Forms.GroupBox();
            this.checkBox_selectAll = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_BoardNumber = new System.Windows.Forms.ComboBox();
            this.button_update = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_waveConfigure)).BeginInit();
            this.groupBox_parameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(697, 72);
            this.splitter1.TabIndex = 76;
            this.splitter1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label1.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(4, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(673, 31);
            this.label1.TabIndex = 77;
            this.label1.Text = "JYUSB1601 MultiChannel Single Output";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_start
            // 
            this.button_start.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button_start.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Location = new System.Drawing.Point(19, 94);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(63, 31);
            this.button_start.TabIndex = 93;
            this.button_start.Tag = "ParaConfig";
            this.button_start.Text = "Start";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // dataGridView_waveConfigure
            // 
            this.dataGridView_waveConfigure.AllowUserToAddRows = false;
            this.dataGridView_waveConfigure.AllowUserToDeleteRows = false;
            this.dataGridView_waveConfigure.AllowUserToResizeRows = false;
            this.dataGridView_waveConfigure.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_waveConfigure.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_waveConfigure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_waveConfigure.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_waveConfigure.Name = "dataGridView_waveConfigure";
            this.dataGridView_waveConfigure.RowHeadersVisible = false;
            this.dataGridView_waveConfigure.RowTemplate.Height = 23;
            this.dataGridView_waveConfigure.Size = new System.Drawing.Size(414, 437);
            this.dataGridView_waveConfigure.TabIndex = 99;
            // 
            // groupBox_parameter
            // 
            this.groupBox_parameter.Controls.Add(this.checkBox_selectAll);
            this.groupBox_parameter.Controls.Add(this.label7);
            this.groupBox_parameter.Controls.Add(this.comboBox_BoardNumber);
            this.groupBox_parameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_parameter.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_parameter.Location = new System.Drawing.Point(0, 0);
            this.groupBox_parameter.Name = "groupBox_parameter";
            this.groupBox_parameter.Size = new System.Drawing.Size(279, 196);
            this.groupBox_parameter.TabIndex = 98;
            this.groupBox_parameter.TabStop = false;
            // 
            // checkBox_selectAll
            // 
            this.checkBox_selectAll.AutoSize = true;
            this.checkBox_selectAll.Location = new System.Drawing.Point(17, 86);
            this.checkBox_selectAll.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_selectAll.Name = "checkBox_selectAll";
            this.checkBox_selectAll.Size = new System.Drawing.Size(145, 18);
            this.checkBox_selectAll.TabIndex = 100;
            this.checkBox_selectAll.Text = "channel_allselect";
            this.checkBox_selectAll.UseVisualStyleBackColor = true;
            this.checkBox_selectAll.CheckedChanged += new System.EventHandler(this.checkBox_selectAll_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(17, 44);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 14);
            this.label7.TabIndex = 79;
            this.label7.Text = "Slot Number";
            // 
            // comboBox_BoardNumber
            // 
            this.comboBox_BoardNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_BoardNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_BoardNumber.FormattingEnabled = true;
            this.comboBox_BoardNumber.Items.AddRange(new object[] {
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
            this.comboBox_BoardNumber.Location = new System.Drawing.Point(123, 44);
            this.comboBox_BoardNumber.Name = "comboBox_BoardNumber";
            this.comboBox_BoardNumber.Size = new System.Drawing.Size(102, 20);
            this.comboBox_BoardNumber.TabIndex = 77;
            this.comboBox_BoardNumber.Tag = "ParaConfig";
            // 
            // button_update
            // 
            this.button_update.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button_update.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_update.Location = new System.Drawing.Point(106, 94);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(63, 31);
            this.button_update.TabIndex = 100;
            this.button_update.Tag = "ParaConfig";
            this.button_update.Text = "Update";
            this.button_update.UseVisualStyleBackColor = true;
            this.button_update.Click += new System.EventHandler(this.button_update_Click);
            // 
            // button_stop
            // 
            this.button_stop.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(193, 94);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(63, 31);
            this.button_stop.TabIndex = 101;
            this.button_stop.Tag = "ParaConfig";
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 72);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridView_waveConfigure);
            this.splitContainer1.Size = new System.Drawing.Size(697, 437);
            this.splitContainer1.SplitterDistance = 279;
            this.splitContainer1.TabIndex = 102;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox_parameter);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.button_start);
            this.splitContainer2.Panel2.Controls.Add(this.button_stop);
            this.splitContainer2.Panel2.Controls.Add(this.button_update);
            this.splitContainer2.Size = new System.Drawing.Size(279, 437);
            this.splitContainer2.SplitterDistance = 196;
            this.splitContainer2.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 509);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.splitter1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 MultiChannel Single Output";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_waveConfigure)).EndInit();
            this.groupBox_parameter.ResumeLayout(false);
            this.groupBox_parameter.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.DataGridView dataGridView_waveConfigure;
        private System.Windows.Forms.GroupBox groupBox_parameter;
        private System.Windows.Forms.CheckBox checkBox_selectAll;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_BoardNumber;
        private System.Windows.Forms.Button button_update;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}

