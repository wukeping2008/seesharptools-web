namespace SeeSharpExample.JY.JYUSB1601
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
            SeeSharpTools.JY.GUI.EasyChartXSeries easyChartXSeries1 = new SeeSharpTools.JY.GUI.EasyChartXSeries();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.timer_FetchData = new System.Windows.Forms.Timer(this.components);
            this.button_start = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.button_stop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox_param = new System.Windows.Forms.GroupBox();
            this.numericUpDown_readLength = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_totalSamples = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_PreviewSamplesPerChannels = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown_channelCount = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_OpenFile = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.easyChartX_playbackData = new SeeSharpTools.JY.GUI.EasyChartX();
            this.label11 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.groupBox_param.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_readLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_totalSamples)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PreviewSamplesPerChannels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_channelCount)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer_FetchData
            // 
            this.timer_FetchData.Tick += new System.EventHandler(this.timer_FetchData_Tick);
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Location = new System.Drawing.Point(39, 328);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(156, 33);
            this.button_start.TabIndex = 94;
            this.button_start.Text = "Start Playback";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1021, 80);
            this.splitter1.TabIndex = 91;
            this.splitter1.TabStop = false;
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(39, 363);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(156, 33);
            this.button_stop.TabIndex = 95;
            this.button_stop.Text = "Stop Playback";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 511);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1021, 22);
            this.statusStrip1.TabIndex = 96;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // groupBox_param
            // 
            this.groupBox_param.Controls.Add(this.numericUpDown_readLength);
            this.groupBox_param.Controls.Add(this.label3);
            this.groupBox_param.Controls.Add(this.numericUpDown_totalSamples);
            this.groupBox_param.Controls.Add(this.label1);
            this.groupBox_param.Controls.Add(this.numericUpDown_PreviewSamplesPerChannels);
            this.groupBox_param.Controls.Add(this.label5);
            this.groupBox_param.Controls.Add(this.numericUpDown_channelCount);
            this.groupBox_param.Controls.Add(this.label2);
            this.groupBox_param.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_param.Location = new System.Drawing.Point(3, 3);
            this.groupBox_param.Name = "groupBox_param";
            this.groupBox_param.Size = new System.Drawing.Size(226, 266);
            this.groupBox_param.TabIndex = 97;
            this.groupBox_param.TabStop = false;
            this.groupBox_param.Text = "Basic parameter configuration";
            // 
            // numericUpDown_readLength
            // 
            this.numericUpDown_readLength.Location = new System.Drawing.Point(9, 202);
            this.numericUpDown_readLength.Maximum = new decimal(new int[] {
            1233977344,
            465661,
            0,
            0});
            this.numericUpDown_readLength.Name = "numericUpDown_readLength";
            this.numericUpDown_readLength.Size = new System.Drawing.Size(120, 23);
            this.numericUpDown_readLength.TabIndex = 14;
            this.numericUpDown_readLength.ThousandsSeparator = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 185);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 14);
            this.label3.TabIndex = 13;
            this.label3.Text = "Data Readed Per Channel";
            // 
            // numericUpDown_totalSamples
            // 
            this.numericUpDown_totalSamples.Location = new System.Drawing.Point(9, 159);
            this.numericUpDown_totalSamples.Maximum = new decimal(new int[] {
            1233977344,
            465661,
            0,
            0});
            this.numericUpDown_totalSamples.Name = "numericUpDown_totalSamples";
            this.numericUpDown_totalSamples.Size = new System.Drawing.Size(120, 23);
            this.numericUpDown_totalSamples.TabIndex = 14;
            this.numericUpDown_totalSamples.ThousandsSeparator = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 142);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(196, 14);
            this.label1.TabIndex = 13;
            this.label1.Text = "Total Samples  Per Channel ";
            // 
            // numericUpDown_PreviewSamplesPerChannels
            // 
            this.numericUpDown_PreviewSamplesPerChannels.Location = new System.Drawing.Point(9, 116);
            this.numericUpDown_PreviewSamplesPerChannels.Maximum = new decimal(new int[] {
            2000000,
            0,
            0,
            0});
            this.numericUpDown_PreviewSamplesPerChannels.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown_PreviewSamplesPerChannels.Name = "numericUpDown_PreviewSamplesPerChannels";
            this.numericUpDown_PreviewSamplesPerChannels.Size = new System.Drawing.Size(120, 23);
            this.numericUpDown_PreviewSamplesPerChannels.TabIndex = 14;
            this.numericUpDown_PreviewSamplesPerChannels.ThousandsSeparator = true;
            this.numericUpDown_PreviewSamplesPerChannels.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(203, 14);
            this.label5.TabIndex = 13;
            this.label5.Text = "Playback Samples Per Channel";
            // 
            // numericUpDown_channelCount
            // 
            this.numericUpDown_channelCount.Location = new System.Drawing.Point(9, 53);
            this.numericUpDown_channelCount.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numericUpDown_channelCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_channelCount.Name = "numericUpDown_channelCount";
            this.numericUpDown_channelCount.Size = new System.Drawing.Size(120, 23);
            this.numericUpDown_channelCount.TabIndex = 4;
            this.numericUpDown_channelCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 14);
            this.label2.TabIndex = 3;
            this.label2.Text = "Number of Channels";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.groupBox_param);
            this.panel1.Controls.Add(this.button_OpenFile);
            this.panel1.Controls.Add(this.button_start);
            this.panel1.Controls.Add(this.button_stop);
            this.panel1.Location = new System.Drawing.Point(781, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 422);
            this.panel1.TabIndex = 98;
            // 
            // button_OpenFile
            // 
            this.button_OpenFile.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_OpenFile.Location = new System.Drawing.Point(39, 293);
            this.button_OpenFile.Name = "button_OpenFile";
            this.button_OpenFile.Size = new System.Drawing.Size(156, 31);
            this.button_OpenFile.TabIndex = 94;
            this.button_OpenFile.Text = "Open File";
            this.button_OpenFile.UseVisualStyleBackColor = true;
            this.button_OpenFile.Click += new System.EventHandler(this.button_OpenFile_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.easyChartX_playbackData);
            this.panel2.Location = new System.Drawing.Point(12, 86);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(763, 422);
            this.panel2.TabIndex = 99;
            // 
            // easyChartX_playbackData
            // 
            this.easyChartX_playbackData.AutoClear = true;
            this.easyChartX_playbackData.AxisX.AutoScale = true;
            this.easyChartX_playbackData.AxisX.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByWholeNumbers;
            this.easyChartX_playbackData.AxisX.AutoZoomReset = false;
            this.easyChartX_playbackData.AxisX.Color = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX.InitWithScaleView = false;
            this.easyChartX_playbackData.AxisX.IsLogarithmic = false;
            this.easyChartX_playbackData.AxisX.LabelAngle = 0;
            this.easyChartX_playbackData.AxisX.LabelEnabled = true;
            this.easyChartX_playbackData.AxisX.LabelFormat = null;
            this.easyChartX_playbackData.AxisX.LogarithmBase = 10D;
            this.easyChartX_playbackData.AxisX.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_playbackData.AxisX.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX.MajorGridCount = -1;
            this.easyChartX_playbackData.AxisX.MajorGridEnabled = true;
            this.easyChartX_playbackData.AxisX.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_playbackData.AxisX.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_playbackData.AxisX.Maximum = 1000D;
            this.easyChartX_playbackData.AxisX.MinGridCountPerPixel = 0.004D;
            this.easyChartX_playbackData.AxisX.Minimum = 0D;
            this.easyChartX_playbackData.AxisX.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX.MinorGridEnabled = false;
            this.easyChartX_playbackData.AxisX.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_playbackData.AxisX.ShowLogarithmicLines = false;
            this.easyChartX_playbackData.AxisX.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX.TickWidth = 1F;
            this.easyChartX_playbackData.AxisX.Title = "";
            this.easyChartX_playbackData.AxisX.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_playbackData.AxisX.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_playbackData.AxisX.ViewMaximum = 1000D;
            this.easyChartX_playbackData.AxisX.ViewMinimum = 0D;
            this.easyChartX_playbackData.AxisX2.AutoScale = true;
            this.easyChartX_playbackData.AxisX2.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByWholeNumbers;
            this.easyChartX_playbackData.AxisX2.AutoZoomReset = false;
            this.easyChartX_playbackData.AxisX2.Color = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX2.InitWithScaleView = false;
            this.easyChartX_playbackData.AxisX2.IsLogarithmic = false;
            this.easyChartX_playbackData.AxisX2.LabelAngle = 0;
            this.easyChartX_playbackData.AxisX2.LabelEnabled = true;
            this.easyChartX_playbackData.AxisX2.LabelFormat = null;
            this.easyChartX_playbackData.AxisX2.LogarithmBase = 10D;
            this.easyChartX_playbackData.AxisX2.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_playbackData.AxisX2.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX2.MajorGridCount = -1;
            this.easyChartX_playbackData.AxisX2.MajorGridEnabled = true;
            this.easyChartX_playbackData.AxisX2.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_playbackData.AxisX2.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_playbackData.AxisX2.Maximum = 1000D;
            this.easyChartX_playbackData.AxisX2.MinGridCountPerPixel = 0.004D;
            this.easyChartX_playbackData.AxisX2.Minimum = 0D;
            this.easyChartX_playbackData.AxisX2.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX2.MinorGridEnabled = false;
            this.easyChartX_playbackData.AxisX2.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_playbackData.AxisX2.ShowLogarithmicLines = false;
            this.easyChartX_playbackData.AxisX2.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisX2.TickWidth = 1F;
            this.easyChartX_playbackData.AxisX2.Title = "";
            this.easyChartX_playbackData.AxisX2.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_playbackData.AxisX2.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_playbackData.AxisX2.ViewMaximum = 1000D;
            this.easyChartX_playbackData.AxisX2.ViewMinimum = 0D;
            this.easyChartX_playbackData.AxisY.AutoScale = true;
            this.easyChartX_playbackData.AxisY.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByGridCount;
            this.easyChartX_playbackData.AxisY.AutoZoomReset = false;
            this.easyChartX_playbackData.AxisY.Color = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY.InitWithScaleView = false;
            this.easyChartX_playbackData.AxisY.IsLogarithmic = false;
            this.easyChartX_playbackData.AxisY.LabelAngle = 0;
            this.easyChartX_playbackData.AxisY.LabelEnabled = true;
            this.easyChartX_playbackData.AxisY.LabelFormat = null;
            this.easyChartX_playbackData.AxisY.LogarithmBase = 10D;
            this.easyChartX_playbackData.AxisY.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_playbackData.AxisY.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY.MajorGridCount = 6;
            this.easyChartX_playbackData.AxisY.MajorGridEnabled = true;
            this.easyChartX_playbackData.AxisY.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_playbackData.AxisY.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_playbackData.AxisY.Maximum = 3.5D;
            this.easyChartX_playbackData.AxisY.MinGridCountPerPixel = 0.004D;
            this.easyChartX_playbackData.AxisY.Minimum = 0.5D;
            this.easyChartX_playbackData.AxisY.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY.MinorGridEnabled = false;
            this.easyChartX_playbackData.AxisY.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_playbackData.AxisY.ShowLogarithmicLines = false;
            this.easyChartX_playbackData.AxisY.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY.TickWidth = 1F;
            this.easyChartX_playbackData.AxisY.Title = "";
            this.easyChartX_playbackData.AxisY.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_playbackData.AxisY.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_playbackData.AxisY.ViewMaximum = 3.5D;
            this.easyChartX_playbackData.AxisY.ViewMinimum = 0.5D;
            this.easyChartX_playbackData.AxisY2.AutoScale = true;
            this.easyChartX_playbackData.AxisY2.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByGridCount;
            this.easyChartX_playbackData.AxisY2.AutoZoomReset = false;
            this.easyChartX_playbackData.AxisY2.Color = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY2.InitWithScaleView = false;
            this.easyChartX_playbackData.AxisY2.IsLogarithmic = false;
            this.easyChartX_playbackData.AxisY2.LabelAngle = 0;
            this.easyChartX_playbackData.AxisY2.LabelEnabled = true;
            this.easyChartX_playbackData.AxisY2.LabelFormat = null;
            this.easyChartX_playbackData.AxisY2.LogarithmBase = 10D;
            this.easyChartX_playbackData.AxisY2.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_playbackData.AxisY2.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY2.MajorGridCount = 6;
            this.easyChartX_playbackData.AxisY2.MajorGridEnabled = true;
            this.easyChartX_playbackData.AxisY2.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_playbackData.AxisY2.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_playbackData.AxisY2.Maximum = 3.5D;
            this.easyChartX_playbackData.AxisY2.MinGridCountPerPixel = 0.004D;
            this.easyChartX_playbackData.AxisY2.Minimum = 0.5D;
            this.easyChartX_playbackData.AxisY2.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY2.MinorGridEnabled = false;
            this.easyChartX_playbackData.AxisY2.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_playbackData.AxisY2.ShowLogarithmicLines = false;
            this.easyChartX_playbackData.AxisY2.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.AxisY2.TickWidth = 1F;
            this.easyChartX_playbackData.AxisY2.Title = "";
            this.easyChartX_playbackData.AxisY2.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_playbackData.AxisY2.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_playbackData.AxisY2.ViewMaximum = 3.5D;
            this.easyChartX_playbackData.AxisY2.ViewMinimum = 0.5D;
            this.easyChartX_playbackData.BackColor = System.Drawing.Color.White;
            this.easyChartX_playbackData.ChartAreaBackColor = System.Drawing.Color.Empty;
            this.easyChartX_playbackData.Cumulitive = false;
            this.easyChartX_playbackData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyChartX_playbackData.GradientStyle = SeeSharpTools.JY.GUI.EasyChartX.ChartGradientStyle.None;
            this.easyChartX_playbackData.LegendBackColor = System.Drawing.Color.Transparent;
            this.easyChartX_playbackData.LegendFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.easyChartX_playbackData.LegendForeColor = System.Drawing.Color.Black;
            this.easyChartX_playbackData.LegendVisible = true;
            easyChartXSeries1.Color = System.Drawing.Color.Red;
            easyChartXSeries1.Marker = SeeSharpTools.JY.GUI.EasyChartXSeries.MarkerType.None;
            easyChartXSeries1.Name = "Series1";
            easyChartXSeries1.Type = SeeSharpTools.JY.GUI.EasyChartXSeries.LineType.FastLine;
            easyChartXSeries1.Visible = true;
            easyChartXSeries1.Width = SeeSharpTools.JY.GUI.EasyChartXSeries.LineWidth.Thin;
            easyChartXSeries1.XPlotAxis = SeeSharpTools.JY.GUI.EasyChartXAxis.PlotAxis.Primary;
            easyChartXSeries1.YPlotAxis = SeeSharpTools.JY.GUI.EasyChartXAxis.PlotAxis.Primary;
            this.easyChartX_playbackData.LineSeries.Add(easyChartXSeries1);
            this.easyChartX_playbackData.Location = new System.Drawing.Point(0, 0);
            this.easyChartX_playbackData.Margin = new System.Windows.Forms.Padding(4);
            this.easyChartX_playbackData.Miscellaneous.CheckInfinity = false;
            this.easyChartX_playbackData.Miscellaneous.CheckNaN = false;
            this.easyChartX_playbackData.Miscellaneous.CheckNegtiveOrZero = false;
            this.easyChartX_playbackData.Miscellaneous.DataStorage = SeeSharpTools.JY.GUI.DataStorageType.Clone;
            this.easyChartX_playbackData.Miscellaneous.DirectionChartCount = 3;
            this.easyChartX_playbackData.Miscellaneous.Fitting = SeeSharpTools.JY.GUI.EasyChartX.FitType.Range;
            this.easyChartX_playbackData.Miscellaneous.MarkerSize = 5;
            this.easyChartX_playbackData.Miscellaneous.MaxSeriesCount = 32;
            this.easyChartX_playbackData.Miscellaneous.MaxSeriesPointCount = 4000;
            this.easyChartX_playbackData.Miscellaneous.ShowFunctionMenu = true;
            this.easyChartX_playbackData.Miscellaneous.SplitLayoutColumnInterval = 0F;
            this.easyChartX_playbackData.Miscellaneous.SplitLayoutDirection = SeeSharpTools.JY.GUI.EasyChartXUtility.LayoutDirection.LeftToRight;
            this.easyChartX_playbackData.Miscellaneous.SplitLayoutRowInterval = 0F;
            this.easyChartX_playbackData.Miscellaneous.SplitViewAutoLayout = true;
            this.easyChartX_playbackData.Name = "easyChartX_playbackData";
            this.easyChartX_playbackData.SeriesCount = 1;
            this.easyChartX_playbackData.Size = new System.Drawing.Size(763, 422);
            this.easyChartX_playbackData.SplitView = false;
            this.easyChartX_playbackData.TabIndex = 0;
            this.easyChartX_playbackData.XCursor.AutoInterval = true;
            this.easyChartX_playbackData.XCursor.Color = System.Drawing.Color.DeepSkyBlue;
            this.easyChartX_playbackData.XCursor.Interval = 0.001D;
            this.easyChartX_playbackData.XCursor.Mode = SeeSharpTools.JY.GUI.EasyChartXCursor.CursorMode.Zoom;
            this.easyChartX_playbackData.XCursor.SelectionColor = System.Drawing.Color.LightGray;
            this.easyChartX_playbackData.XCursor.Value = double.NaN;
            this.easyChartX_playbackData.YCursor.AutoInterval = true;
            this.easyChartX_playbackData.YCursor.Color = System.Drawing.Color.DeepSkyBlue;
            this.easyChartX_playbackData.YCursor.Interval = 0.001D;
            this.easyChartX_playbackData.YCursor.Mode = SeeSharpTools.JY.GUI.EasyChartXCursor.CursorMode.Disabled;
            this.easyChartX_playbackData.YCursor.SelectionColor = System.Drawing.Color.LightGray;
            this.easyChartX_playbackData.YCursor.Value = double.NaN;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoEllipsis = true;
            this.label11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label11.Font = new System.Drawing.Font("宋体", 23F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(148, 27);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(721, 31);
            this.label11.TabIndex = 93;
            this.label11.Text = "JYUSB1601 Data Playback";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1021, 533);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitter1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 Data Playback";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox_param.ResumeLayout(false);
            this.groupBox_param.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_readLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_totalSamples)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PreviewSamplesPerChannels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_channelCount)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer_FetchData;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBox_param;
        private System.Windows.Forms.NumericUpDown numericUpDown_PreviewSamplesPerChannels;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown_channelCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private SeeSharpTools.JY.GUI.EasyChartX easyChartX_playbackData;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericUpDown_totalSamples;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OpenFile;
        private System.Windows.Forms.NumericUpDown numericUpDown_readLength;
        private System.Windows.Forms.Label label3;
    }
}

