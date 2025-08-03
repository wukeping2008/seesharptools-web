namespace Winform_AO_Continuous_Wrapping_Digital_Trigger
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
            SeeSharpTools.JY.GUI.EasyChartXSeries easyChartXSeries1 = new SeeSharpTools.JY.GUI.EasyChartXSeries();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDown_waveformAmplitude = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBox_waveformType = new System.Windows.Forms.ComboBox();
            this.numericUpDown_waveformFrequency = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox_waveConfig = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox_genParam = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_samplesToUpdate = new System.Windows.Forms.NumericUpDown();
            this.comboBox_channelNumber = new System.Windows.Forms.ComboBox();
            this.comboBox_SoltNumber = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label_SampleRate = new System.Windows.Forms.Label();
            this.label_Channel = new System.Windows.Forms.Label();
            this.numericUpDown_updateRate = new System.Windows.Forms.NumericUpDown();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.easyChartX_AO = new SeeSharpTools.JY.GUI.EasyChartX();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.groupBox_TrigParam = new System.Windows.Forms.GroupBox();
            this.comboBox_triggerEdge = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.comboBox_triggerSource = new System.Windows.Forms.ComboBox();
            this.label_triggerSource = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waveformAmplitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waveformFrequency)).BeginInit();
            this.groupBox_waveConfig.SuspendLayout();
            this.groupBox_genParam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_samplesToUpdate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_updateRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.groupBox_TrigParam.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(176, 29);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 14);
            this.label8.TabIndex = 77;
            this.label8.Text = "Wave Amplitude";
            // 
            // numericUpDown_waveformAmplitude
            // 
            this.numericUpDown_waveformAmplitude.Location = new System.Drawing.Point(178, 60);
            this.numericUpDown_waveformAmplitude.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_waveformAmplitude.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericUpDown_waveformAmplitude.Name = "numericUpDown_waveformAmplitude";
            this.numericUpDown_waveformAmplitude.Size = new System.Drawing.Size(151, 23);
            this.numericUpDown_waveformAmplitude.TabIndex = 76;
            this.numericUpDown_waveformAmplitude.Tag = "ParaConfig";
            this.numericUpDown_waveformAmplitude.ThousandsSeparator = true;
            this.numericUpDown_waveformAmplitude.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(363, 29);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(105, 14);
            this.label10.TabIndex = 75;
            this.label10.Text = "Wave Frequency";
            // 
            // comboBox_waveformType
            // 
            this.comboBox_waveformType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_waveformType.FormattingEnabled = true;
            this.comboBox_waveformType.Items.AddRange(new object[] {
            "SineWave",
            "SquareWave",
            "UniformWhiteNoise"});
            this.comboBox_waveformType.Location = new System.Drawing.Point(16, 61);
            this.comboBox_waveformType.Name = "comboBox_waveformType";
            this.comboBox_waveformType.Size = new System.Drawing.Size(142, 22);
            this.comboBox_waveformType.TabIndex = 74;
            this.comboBox_waveformType.Tag = "ParaConfig";
            // 
            // numericUpDown_waveformFrequency
            // 
            this.numericUpDown_waveformFrequency.Location = new System.Drawing.Point(365, 60);
            this.numericUpDown_waveformFrequency.Maximum = new decimal(new int[] {
            250000,
            0,
            0,
            0});
            this.numericUpDown_waveformFrequency.Name = "numericUpDown_waveformFrequency";
            this.numericUpDown_waveformFrequency.Size = new System.Drawing.Size(151, 23);
            this.numericUpDown_waveformFrequency.TabIndex = 72;
            this.numericUpDown_waveformFrequency.Tag = "ParaConfig";
            this.numericUpDown_waveformFrequency.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label2.Font = new System.Drawing.Font("SimSun", 23F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(878, 31);
            this.label2.TabIndex = 89;
            this.label2.Text = "JYUSB61902Single Channel 连续环绕 output（数字触发）";
            // 
            // groupBox_waveConfig
            // 
            this.groupBox_waveConfig.Controls.Add(this.label8);
            this.groupBox_waveConfig.Controls.Add(this.numericUpDown_waveformAmplitude);
            this.groupBox_waveConfig.Controls.Add(this.label9);
            this.groupBox_waveConfig.Controls.Add(this.label10);
            this.groupBox_waveConfig.Controls.Add(this.comboBox_waveformType);
            this.groupBox_waveConfig.Controls.Add(this.numericUpDown_waveformFrequency);
            this.groupBox_waveConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_waveConfig.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_waveConfig.Location = new System.Drawing.Point(0, 0);
            this.groupBox_waveConfig.Name = "groupBox_waveConfig";
            this.groupBox_waveConfig.Size = new System.Drawing.Size(691, 96);
            this.groupBox_waveConfig.TabIndex = 87;
            this.groupBox_waveConfig.TabStop = false;
            this.groupBox_waveConfig.Text = "Waveform Configuration";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(13, 29);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 14);
            this.label9.TabIndex = 73;
            this.label9.Text = "Wave Type";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1443, 80);
            this.splitter1.TabIndex = 90;
            this.splitter1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label1.Font = new System.Drawing.Font("SimSun", 23F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(147, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1068, 31);
            this.label1.TabIndex = 91;
            this.label1.Text = "JYUSB1601 Single Channel Continuous";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoEllipsis = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label4.Font = new System.Drawing.Font("SimSun", 23F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(260, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(843, 31);
            this.label4.TabIndex = 93;
            this.label4.Text = "Wrapping Output（Digital Trigger）";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox_genParam
            // 
            this.groupBox_genParam.Controls.Add(this.label3);
            this.groupBox_genParam.Controls.Add(this.numericUpDown_samplesToUpdate);
            this.groupBox_genParam.Controls.Add(this.comboBox_channelNumber);
            this.groupBox_genParam.Controls.Add(this.comboBox_SoltNumber);
            this.groupBox_genParam.Controls.Add(this.label7);
            this.groupBox_genParam.Controls.Add(this.label_SampleRate);
            this.groupBox_genParam.Controls.Add(this.label_Channel);
            this.groupBox_genParam.Controls.Add(this.numericUpDown_updateRate);
            this.groupBox_genParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_genParam.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_genParam.Location = new System.Drawing.Point(0, 0);
            this.groupBox_genParam.Name = "groupBox_genParam";
            this.groupBox_genParam.Size = new System.Drawing.Size(691, 132);
            this.groupBox_genParam.TabIndex = 92;
            this.groupBox_genParam.TabStop = false;
            this.groupBox_genParam.Text = "Basic Param Configuration";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(251, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 14);
            this.label3.TabIndex = 126;
            this.label3.Text = "Samples to Update";
            // 
            // numericUpDown_samplesToUpdate
            // 
            this.numericUpDown_samplesToUpdate.Location = new System.Drawing.Point(386, 99);
            this.numericUpDown_samplesToUpdate.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_samplesToUpdate.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDown_samplesToUpdate.Name = "numericUpDown_samplesToUpdate";
            this.numericUpDown_samplesToUpdate.Size = new System.Drawing.Size(132, 23);
            this.numericUpDown_samplesToUpdate.TabIndex = 127;
            this.numericUpDown_samplesToUpdate.Tag = "ParaConfig";
            this.numericUpDown_samplesToUpdate.ThousandsSeparator = true;
            this.numericUpDown_samplesToUpdate.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // comboBox_channelNumber
            // 
            this.comboBox_channelNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_channelNumber.FormattingEnabled = true;
            this.comboBox_channelNumber.Location = new System.Drawing.Point(108, 99);
            this.comboBox_channelNumber.Name = "comboBox_channelNumber";
            this.comboBox_channelNumber.Size = new System.Drawing.Size(119, 22);
            this.comboBox_channelNumber.TabIndex = 67;
            this.comboBox_channelNumber.Tag = "ParaConfig";
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
            this.comboBox_SoltNumber.Location = new System.Drawing.Point(108, 56);
            this.comboBox_SoltNumber.Name = "comboBox_SoltNumber";
            this.comboBox_SoltNumber.Size = new System.Drawing.Size(119, 22);
            this.comboBox_SoltNumber.TabIndex = 63;
            this.comboBox_SoltNumber.Tag = "ParaConfig";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(13, 57);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 14);
            this.label7.TabIndex = 64;
            this.label7.Text = "Slot Number ";
            // 
            // label_SampleRate
            // 
            this.label_SampleRate.AutoSize = true;
            this.label_SampleRate.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_SampleRate.Location = new System.Drawing.Point(251, 58);
            this.label_SampleRate.Name = "label_SampleRate";
            this.label_SampleRate.Size = new System.Drawing.Size(133, 14);
            this.label_SampleRate.TabIndex = 1;
            this.label_SampleRate.Text = "Update Rate(Sa/s) ";
            // 
            // label_Channel
            // 
            this.label_Channel.AutoSize = true;
            this.label_Channel.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Channel.Location = new System.Drawing.Point(13, 104);
            this.label_Channel.Name = "label_Channel";
            this.label_Channel.Size = new System.Drawing.Size(77, 14);
            this.label_Channel.TabIndex = 2;
            this.label_Channel.Text = "Channel ID";
            // 
            // numericUpDown_updateRate
            // 
            this.numericUpDown_updateRate.Location = new System.Drawing.Point(386, 56);
            this.numericUpDown_updateRate.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_updateRate.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDown_updateRate.Name = "numericUpDown_updateRate";
            this.numericUpDown_updateRate.Size = new System.Drawing.Size(128, 23);
            this.numericUpDown_updateRate.TabIndex = 5;
            this.numericUpDown_updateRate.Tag = "ParaConfig";
            this.numericUpDown_updateRate.ThousandsSeparator = true;
            this.numericUpDown_updateRate.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 94);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.easyChartX_AO);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1443, 518);
            this.splitContainer1.SplitterDistance = 780;
            this.splitContainer1.TabIndex = 95;
            // 
            // easyChartX_AO
            // 
            this.easyChartX_AO.AutoClear = true;
            this.easyChartX_AO.AxisX.AutoScale = true;
            this.easyChartX_AO.AxisX.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByWholeNumbers;
            this.easyChartX_AO.AxisX.AutoZoomReset = false;
            this.easyChartX_AO.AxisX.Color = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX.InitWithScaleView = false;
            this.easyChartX_AO.AxisX.IsLogarithmic = false;
            this.easyChartX_AO.AxisX.LabelAngle = 0;
            this.easyChartX_AO.AxisX.LabelEnabled = true;
            this.easyChartX_AO.AxisX.LabelFormat = null;
            this.easyChartX_AO.AxisX.LogarithmBase = 10D;
            this.easyChartX_AO.AxisX.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_AO.AxisX.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX.MajorGridCount = -1;
            this.easyChartX_AO.AxisX.MajorGridEnabled = true;
            this.easyChartX_AO.AxisX.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_AO.AxisX.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_AO.AxisX.Maximum = 1000D;
            this.easyChartX_AO.AxisX.MinGridCountPerPixel = 0.004D;
            this.easyChartX_AO.AxisX.Minimum = 0D;
            this.easyChartX_AO.AxisX.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX.MinorGridEnabled = false;
            this.easyChartX_AO.AxisX.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_AO.AxisX.ShowLogarithmicLines = false;
            this.easyChartX_AO.AxisX.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX.TickWidth = 1F;
            this.easyChartX_AO.AxisX.Title = "";
            this.easyChartX_AO.AxisX.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_AO.AxisX.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_AO.AxisX.ViewMaximum = 1000D;
            this.easyChartX_AO.AxisX.ViewMinimum = 0D;
            this.easyChartX_AO.AxisX2.AutoScale = true;
            this.easyChartX_AO.AxisX2.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByWholeNumbers;
            this.easyChartX_AO.AxisX2.AutoZoomReset = false;
            this.easyChartX_AO.AxisX2.Color = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX2.InitWithScaleView = false;
            this.easyChartX_AO.AxisX2.IsLogarithmic = false;
            this.easyChartX_AO.AxisX2.LabelAngle = 0;
            this.easyChartX_AO.AxisX2.LabelEnabled = true;
            this.easyChartX_AO.AxisX2.LabelFormat = null;
            this.easyChartX_AO.AxisX2.LogarithmBase = 10D;
            this.easyChartX_AO.AxisX2.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_AO.AxisX2.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX2.MajorGridCount = -1;
            this.easyChartX_AO.AxisX2.MajorGridEnabled = true;
            this.easyChartX_AO.AxisX2.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_AO.AxisX2.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_AO.AxisX2.Maximum = 1000D;
            this.easyChartX_AO.AxisX2.MinGridCountPerPixel = 0.004D;
            this.easyChartX_AO.AxisX2.Minimum = 0D;
            this.easyChartX_AO.AxisX2.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX2.MinorGridEnabled = false;
            this.easyChartX_AO.AxisX2.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_AO.AxisX2.ShowLogarithmicLines = false;
            this.easyChartX_AO.AxisX2.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisX2.TickWidth = 1F;
            this.easyChartX_AO.AxisX2.Title = "";
            this.easyChartX_AO.AxisX2.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_AO.AxisX2.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_AO.AxisX2.ViewMaximum = 1000D;
            this.easyChartX_AO.AxisX2.ViewMinimum = 0D;
            this.easyChartX_AO.AxisY.AutoScale = true;
            this.easyChartX_AO.AxisY.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByGridCount;
            this.easyChartX_AO.AxisY.AutoZoomReset = false;
            this.easyChartX_AO.AxisY.Color = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY.InitWithScaleView = false;
            this.easyChartX_AO.AxisY.IsLogarithmic = false;
            this.easyChartX_AO.AxisY.LabelAngle = 0;
            this.easyChartX_AO.AxisY.LabelEnabled = true;
            this.easyChartX_AO.AxisY.LabelFormat = null;
            this.easyChartX_AO.AxisY.LogarithmBase = 10D;
            this.easyChartX_AO.AxisY.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_AO.AxisY.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY.MajorGridCount = 6;
            this.easyChartX_AO.AxisY.MajorGridEnabled = true;
            this.easyChartX_AO.AxisY.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_AO.AxisY.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_AO.AxisY.Maximum = 3.5D;
            this.easyChartX_AO.AxisY.MinGridCountPerPixel = 0.004D;
            this.easyChartX_AO.AxisY.Minimum = 0.5D;
            this.easyChartX_AO.AxisY.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY.MinorGridEnabled = false;
            this.easyChartX_AO.AxisY.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_AO.AxisY.ShowLogarithmicLines = false;
            this.easyChartX_AO.AxisY.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY.TickWidth = 1F;
            this.easyChartX_AO.AxisY.Title = "";
            this.easyChartX_AO.AxisY.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_AO.AxisY.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_AO.AxisY.ViewMaximum = 3.5D;
            this.easyChartX_AO.AxisY.ViewMinimum = 0.5D;
            this.easyChartX_AO.AxisY2.AutoScale = true;
            this.easyChartX_AO.AxisY2.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByGridCount;
            this.easyChartX_AO.AxisY2.AutoZoomReset = false;
            this.easyChartX_AO.AxisY2.Color = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY2.InitWithScaleView = false;
            this.easyChartX_AO.AxisY2.IsLogarithmic = false;
            this.easyChartX_AO.AxisY2.LabelAngle = 0;
            this.easyChartX_AO.AxisY2.LabelEnabled = true;
            this.easyChartX_AO.AxisY2.LabelFormat = null;
            this.easyChartX_AO.AxisY2.LogarithmBase = 10D;
            this.easyChartX_AO.AxisY2.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_AO.AxisY2.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY2.MajorGridCount = 6;
            this.easyChartX_AO.AxisY2.MajorGridEnabled = true;
            this.easyChartX_AO.AxisY2.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_AO.AxisY2.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_AO.AxisY2.Maximum = 3.5D;
            this.easyChartX_AO.AxisY2.MinGridCountPerPixel = 0.004D;
            this.easyChartX_AO.AxisY2.Minimum = 0.5D;
            this.easyChartX_AO.AxisY2.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY2.MinorGridEnabled = false;
            this.easyChartX_AO.AxisY2.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_AO.AxisY2.ShowLogarithmicLines = false;
            this.easyChartX_AO.AxisY2.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_AO.AxisY2.TickWidth = 1F;
            this.easyChartX_AO.AxisY2.Title = "";
            this.easyChartX_AO.AxisY2.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_AO.AxisY2.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_AO.AxisY2.ViewMaximum = 3.5D;
            this.easyChartX_AO.AxisY2.ViewMinimum = 0.5D;
            this.easyChartX_AO.BackColor = System.Drawing.Color.White;
            this.easyChartX_AO.ChartAreaBackColor = System.Drawing.Color.Empty;
            this.easyChartX_AO.Cumulitive = false;
            this.easyChartX_AO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyChartX_AO.GradientStyle = SeeSharpTools.JY.GUI.EasyChartX.ChartGradientStyle.None;
            this.easyChartX_AO.LegendBackColor = System.Drawing.Color.Transparent;
            this.easyChartX_AO.LegendFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.easyChartX_AO.LegendForeColor = System.Drawing.Color.Black;
            this.easyChartX_AO.LegendVisible = true;
            easyChartXSeries1.Color = System.Drawing.Color.Red;
            easyChartXSeries1.Marker = SeeSharpTools.JY.GUI.EasyChartXSeries.MarkerType.None;
            easyChartXSeries1.Name = "Series1";
            easyChartXSeries1.Type = SeeSharpTools.JY.GUI.EasyChartXSeries.LineType.FastLine;
            easyChartXSeries1.Visible = true;
            easyChartXSeries1.Width = SeeSharpTools.JY.GUI.EasyChartXSeries.LineWidth.Thin;
            easyChartXSeries1.XPlotAxis = SeeSharpTools.JY.GUI.EasyChartXAxis.PlotAxis.Primary;
            easyChartXSeries1.YPlotAxis = SeeSharpTools.JY.GUI.EasyChartXAxis.PlotAxis.Primary;
            this.easyChartX_AO.LineSeries.Add(easyChartXSeries1);
            this.easyChartX_AO.Location = new System.Drawing.Point(0, 0);
            this.easyChartX_AO.Margin = new System.Windows.Forms.Padding(4);
            this.easyChartX_AO.Miscellaneous.CheckInfinity = false;
            this.easyChartX_AO.Miscellaneous.CheckNaN = false;
            this.easyChartX_AO.Miscellaneous.CheckNegtiveOrZero = false;
            this.easyChartX_AO.Miscellaneous.DataStorage = SeeSharpTools.JY.GUI.DataStorageType.Clone;
            this.easyChartX_AO.Miscellaneous.DirectionChartCount = 3;
            this.easyChartX_AO.Miscellaneous.Fitting = SeeSharpTools.JY.GUI.EasyChartX.FitType.Range;
            this.easyChartX_AO.Miscellaneous.MarkerSize = 7;
            this.easyChartX_AO.Miscellaneous.MaxSeriesCount = 32;
            this.easyChartX_AO.Miscellaneous.MaxSeriesPointCount = 4000;
            this.easyChartX_AO.Miscellaneous.ShowFunctionMenu = true;
            this.easyChartX_AO.Miscellaneous.SplitLayoutColumnInterval = 0F;
            this.easyChartX_AO.Miscellaneous.SplitLayoutDirection = SeeSharpTools.JY.GUI.EasyChartXUtility.LayoutDirection.LeftToRight;
            this.easyChartX_AO.Miscellaneous.SplitLayoutRowInterval = 0F;
            this.easyChartX_AO.Miscellaneous.SplitViewAutoLayout = true;
            this.easyChartX_AO.Name = "easyChartX_AO";
            this.easyChartX_AO.SeriesCount = 1;
            this.easyChartX_AO.Size = new System.Drawing.Size(780, 518);
            this.easyChartX_AO.SplitView = false;
            this.easyChartX_AO.TabIndex = 0;
            this.easyChartX_AO.XCursor.AutoInterval = true;
            this.easyChartX_AO.XCursor.Color = System.Drawing.Color.DeepSkyBlue;
            this.easyChartX_AO.XCursor.Interval = 0.001D;
            this.easyChartX_AO.XCursor.Mode = SeeSharpTools.JY.GUI.EasyChartXCursor.CursorMode.Zoom;
            this.easyChartX_AO.XCursor.SelectionColor = System.Drawing.Color.LightGray;
            this.easyChartX_AO.XCursor.Value = double.NaN;
            this.easyChartX_AO.YCursor.AutoInterval = true;
            this.easyChartX_AO.YCursor.Color = System.Drawing.Color.DeepSkyBlue;
            this.easyChartX_AO.YCursor.Interval = 0.001D;
            this.easyChartX_AO.YCursor.Mode = SeeSharpTools.JY.GUI.EasyChartXCursor.CursorMode.Disabled;
            this.easyChartX_AO.YCursor.SelectionColor = System.Drawing.Color.LightGray;
            this.easyChartX_AO.YCursor.Value = double.NaN;
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
            this.splitContainer2.Panel1.Controls.Add(this.groupBox_genParam);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(691, 481);
            this.splitContainer2.SplitterDistance = 132;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.groupBox_waveConfig);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(691, 345);
            this.splitContainer3.SplitterDistance = 96;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.groupBox_TrigParam);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.panel1);
            this.splitContainer4.Size = new System.Drawing.Size(691, 245);
            this.splitContainer4.SplitterDistance = 341;
            this.splitContainer4.TabIndex = 0;
            // 
            // groupBox_TrigParam
            // 
            this.groupBox_TrigParam.Controls.Add(this.label_triggerSource);
            this.groupBox_TrigParam.Controls.Add(this.comboBox_triggerSource);
            this.groupBox_TrigParam.Controls.Add(this.comboBox_triggerEdge);
            this.groupBox_TrigParam.Controls.Add(this.label5);
            this.groupBox_TrigParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_TrigParam.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_TrigParam.Location = new System.Drawing.Point(0, 0);
            this.groupBox_TrigParam.Name = "groupBox_TrigParam";
            this.groupBox_TrigParam.Size = new System.Drawing.Size(341, 245);
            this.groupBox_TrigParam.TabIndex = 88;
            this.groupBox_TrigParam.TabStop = false;
            this.groupBox_TrigParam.Text = "Trigger Param Configuration";
            // 
            // comboBox_triggerEdge
            // 
            this.comboBox_triggerEdge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_triggerEdge.FormattingEnabled = true;
            this.comboBox_triggerEdge.Location = new System.Drawing.Point(136, 44);
            this.comboBox_triggerEdge.Name = "comboBox_triggerEdge";
            this.comboBox_triggerEdge.Size = new System.Drawing.Size(161, 22);
            this.comboBox_triggerEdge.TabIndex = 70;
            this.comboBox_triggerEdge.Tag = "ParaConfig";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(19, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 14);
            this.label5.TabIndex = 69;
            this.label5.Text = "Trigger Edge";
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel1.Controls.Add(this.button_stop);
            this.panel1.Controls.Add(this.button_start);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(346, 245);
            this.panel1.TabIndex = 94;
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(170, 108);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(77, 47);
            this.button_stop.TabIndex = 86;
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Location = new System.Drawing.Point(20, 108);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(77, 47);
            this.button_start.TabIndex = 85;
            this.button_start.Tag = "ParaConfig";
            this.button_start.Text = "Start";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // comboBox_triggerSource
            // 
            this.comboBox_triggerSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_triggerSource.FormattingEnabled = true;
            this.comboBox_triggerSource.Location = new System.Drawing.Point(136, 92);
            this.comboBox_triggerSource.Name = "comboBox_triggerSource";
            this.comboBox_triggerSource.Size = new System.Drawing.Size(161, 22);
            this.comboBox_triggerSource.TabIndex = 71;
            this.comboBox_triggerSource.Tag = "ParaConfig";
            // 
            // label_triggerSource
            // 
            this.label_triggerSource.AutoSize = true;
            this.label_triggerSource.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_triggerSource.Location = new System.Drawing.Point(19, 95);
            this.label_triggerSource.Name = "label_triggerSource";
            this.label_triggerSource.Size = new System.Drawing.Size(105, 14);
            this.label_triggerSource.TabIndex = 72;
            this.label_triggerSource.Text = "Trigger Source";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1443, 618);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 Single Channel Continuous Wrapping Output（Digital Trigger）";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waveformAmplitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waveformFrequency)).EndInit();
            this.groupBox_waveConfig.ResumeLayout(false);
            this.groupBox_waveConfig.PerformLayout();
            this.groupBox_genParam.ResumeLayout(false);
            this.groupBox_genParam.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_samplesToUpdate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_updateRate)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
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
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.groupBox_TrigParam.ResumeLayout(false);
            this.groupBox_TrigParam.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDown_waveformAmplitude;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBox_waveformType;
        private System.Windows.Forms.NumericUpDown numericUpDown_waveformFrequency;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox_waveConfig;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox_genParam;
        private System.Windows.Forms.ComboBox comboBox_channelNumber;
        private System.Windows.Forms.ComboBox comboBox_SoltNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_SampleRate;
        private System.Windows.Forms.Label label_Channel;
        private System.Windows.Forms.NumericUpDown numericUpDown_updateRate;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private SeeSharpTools.JY.GUI.EasyChartX easyChartX_AO;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown_samplesToUpdate;
        private System.Windows.Forms.GroupBox groupBox_TrigParam;
        private System.Windows.Forms.ComboBox comboBox_triggerEdge;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label_triggerSource;
        private System.Windows.Forms.ComboBox comboBox_triggerSource;
    }
}

