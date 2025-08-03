namespace SeeSharpExample.JY.JYUSB1601
{
    partial class AI_MultiCard_Sync
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
            this.components = new System.ComponentModel.Container();
            SeeSharpTools.JY.GUI.EasyChartXSeries easyChartXSeries1 = new SeeSharpTools.JY.GUI.EasyChartXSeries();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AI_MultiCard_Sync));
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.easyChartX_displayData = new SeeSharpTools.JY.GUI.EasyChartX();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDown_samples = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label_SampleRate = new System.Windows.Forms.Label();
            this.numericUpDown_sampleRate = new System.Windows.Forms.NumericUpDown();
            this.label_Channel = new System.Windows.Forms.Label();
            this.comboBox_channelNumber = new System.Windows.Forms.ComboBox();
            this.button_start = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBox_phasediff = new System.Windows.Forms.TextBox();
            this.textBox_timediff = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer_FetchData = new System.Windows.Forms.Timer(this.components);
            this.textBox_masterCardName = new System.Windows.Forms.TextBox();
            this.textBox_slaveCardName = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_samples)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_sampleRate)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1059, 80);
            this.splitter1.TabIndex = 72;
            this.splitter1.TabStop = false;
            // 
            // easyChartX_displayData
            // 
            this.easyChartX_displayData.AutoClear = true;
            this.easyChartX_displayData.AxisX.AutoScale = true;
            this.easyChartX_displayData.AxisX.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByWholeNumbers;
            this.easyChartX_displayData.AxisX.AutoZoomReset = false;
            this.easyChartX_displayData.AxisX.Color = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX.InitWithScaleView = false;
            this.easyChartX_displayData.AxisX.IsLogarithmic = false;
            this.easyChartX_displayData.AxisX.LabelAngle = 0;
            this.easyChartX_displayData.AxisX.LabelEnabled = true;
            this.easyChartX_displayData.AxisX.LabelFormat = null;
            this.easyChartX_displayData.AxisX.LogarithmBase = 10D;
            this.easyChartX_displayData.AxisX.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_displayData.AxisX.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX.MajorGridCount = -1;
            this.easyChartX_displayData.AxisX.MajorGridEnabled = true;
            this.easyChartX_displayData.AxisX.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_displayData.AxisX.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_displayData.AxisX.Maximum = 1000D;
            this.easyChartX_displayData.AxisX.MinGridCountPerPixel = 0.004D;
            this.easyChartX_displayData.AxisX.Minimum = 0D;
            this.easyChartX_displayData.AxisX.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX.MinorGridEnabled = false;
            this.easyChartX_displayData.AxisX.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_displayData.AxisX.ShowLogarithmicLines = false;
            this.easyChartX_displayData.AxisX.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX.TickWidth = 1F;
            this.easyChartX_displayData.AxisX.Title = "";
            this.easyChartX_displayData.AxisX.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_displayData.AxisX.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_displayData.AxisX.ViewMaximum = 1000D;
            this.easyChartX_displayData.AxisX.ViewMinimum = 0D;
            this.easyChartX_displayData.AxisX2.AutoScale = true;
            this.easyChartX_displayData.AxisX2.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByWholeNumbers;
            this.easyChartX_displayData.AxisX2.AutoZoomReset = false;
            this.easyChartX_displayData.AxisX2.Color = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX2.InitWithScaleView = false;
            this.easyChartX_displayData.AxisX2.IsLogarithmic = false;
            this.easyChartX_displayData.AxisX2.LabelAngle = 0;
            this.easyChartX_displayData.AxisX2.LabelEnabled = true;
            this.easyChartX_displayData.AxisX2.LabelFormat = null;
            this.easyChartX_displayData.AxisX2.LogarithmBase = 10D;
            this.easyChartX_displayData.AxisX2.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_displayData.AxisX2.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX2.MajorGridCount = -1;
            this.easyChartX_displayData.AxisX2.MajorGridEnabled = true;
            this.easyChartX_displayData.AxisX2.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_displayData.AxisX2.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_displayData.AxisX2.Maximum = 1000D;
            this.easyChartX_displayData.AxisX2.MinGridCountPerPixel = 0.004D;
            this.easyChartX_displayData.AxisX2.Minimum = 0D;
            this.easyChartX_displayData.AxisX2.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX2.MinorGridEnabled = false;
            this.easyChartX_displayData.AxisX2.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_displayData.AxisX2.ShowLogarithmicLines = false;
            this.easyChartX_displayData.AxisX2.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisX2.TickWidth = 1F;
            this.easyChartX_displayData.AxisX2.Title = "";
            this.easyChartX_displayData.AxisX2.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_displayData.AxisX2.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_displayData.AxisX2.ViewMaximum = 1000D;
            this.easyChartX_displayData.AxisX2.ViewMinimum = 0D;
            this.easyChartX_displayData.AxisY.AutoScale = true;
            this.easyChartX_displayData.AxisY.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByGridCount;
            this.easyChartX_displayData.AxisY.AutoZoomReset = false;
            this.easyChartX_displayData.AxisY.Color = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY.InitWithScaleView = false;
            this.easyChartX_displayData.AxisY.IsLogarithmic = false;
            this.easyChartX_displayData.AxisY.LabelAngle = 0;
            this.easyChartX_displayData.AxisY.LabelEnabled = true;
            this.easyChartX_displayData.AxisY.LabelFormat = null;
            this.easyChartX_displayData.AxisY.LogarithmBase = 10D;
            this.easyChartX_displayData.AxisY.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_displayData.AxisY.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY.MajorGridCount = 6;
            this.easyChartX_displayData.AxisY.MajorGridEnabled = true;
            this.easyChartX_displayData.AxisY.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_displayData.AxisY.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_displayData.AxisY.Maximum = 3.5D;
            this.easyChartX_displayData.AxisY.MinGridCountPerPixel = 0.004D;
            this.easyChartX_displayData.AxisY.Minimum = 0.5D;
            this.easyChartX_displayData.AxisY.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY.MinorGridEnabled = false;
            this.easyChartX_displayData.AxisY.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_displayData.AxisY.ShowLogarithmicLines = false;
            this.easyChartX_displayData.AxisY.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY.TickWidth = 1F;
            this.easyChartX_displayData.AxisY.Title = "";
            this.easyChartX_displayData.AxisY.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_displayData.AxisY.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_displayData.AxisY.ViewMaximum = 3.5D;
            this.easyChartX_displayData.AxisY.ViewMinimum = 0.5D;
            this.easyChartX_displayData.AxisY2.AutoScale = true;
            this.easyChartX_displayData.AxisY2.AutoScalingMode = SeeSharpTools.JY.GUI.EasyChartXAxis.AutoScaleMode.ByGridCount;
            this.easyChartX_displayData.AxisY2.AutoZoomReset = false;
            this.easyChartX_displayData.AxisY2.Color = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY2.InitWithScaleView = false;
            this.easyChartX_displayData.AxisY2.IsLogarithmic = false;
            this.easyChartX_displayData.AxisY2.LabelAngle = 0;
            this.easyChartX_displayData.AxisY2.LabelEnabled = true;
            this.easyChartX_displayData.AxisY2.LabelFormat = null;
            this.easyChartX_displayData.AxisY2.LogarithmBase = 10D;
            this.easyChartX_displayData.AxisY2.LogLabelStyle = SeeSharpTools.JY.GUI.EasyChartXAxis.LogarithmicLabelStyle.E2;
            this.easyChartX_displayData.AxisY2.MajorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY2.MajorGridCount = 6;
            this.easyChartX_displayData.AxisY2.MajorGridEnabled = true;
            this.easyChartX_displayData.AxisY2.MajorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.Dash;
            this.easyChartX_displayData.AxisY2.MaxGridCountPerPixel = 0.012D;
            this.easyChartX_displayData.AxisY2.Maximum = 3.5D;
            this.easyChartX_displayData.AxisY2.MinGridCountPerPixel = 0.004D;
            this.easyChartX_displayData.AxisY2.Minimum = 0.5D;
            this.easyChartX_displayData.AxisY2.MinorGridColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY2.MinorGridEnabled = false;
            this.easyChartX_displayData.AxisY2.MinorGridType = SeeSharpTools.JY.GUI.EasyChartXAxis.GridStyle.DashDot;
            this.easyChartX_displayData.AxisY2.ShowLogarithmicLines = false;
            this.easyChartX_displayData.AxisY2.TickLineColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.AxisY2.TickWidth = 1F;
            this.easyChartX_displayData.AxisY2.Title = "";
            this.easyChartX_displayData.AxisY2.TitleOrientation = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextOrientation.Auto;
            this.easyChartX_displayData.AxisY2.TitlePosition = SeeSharpTools.JY.GUI.EasyChartXAxis.AxisTextPosition.Center;
            this.easyChartX_displayData.AxisY2.ViewMaximum = 3.5D;
            this.easyChartX_displayData.AxisY2.ViewMinimum = 0.5D;
            this.easyChartX_displayData.BackColor = System.Drawing.Color.White;
            this.easyChartX_displayData.ChartAreaBackColor = System.Drawing.Color.Empty;
            this.easyChartX_displayData.Cumulitive = false;
            this.easyChartX_displayData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyChartX_displayData.GradientStyle = SeeSharpTools.JY.GUI.EasyChartX.ChartGradientStyle.None;
            this.easyChartX_displayData.LegendBackColor = System.Drawing.Color.Transparent;
            this.easyChartX_displayData.LegendFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.easyChartX_displayData.LegendForeColor = System.Drawing.Color.Black;
            this.easyChartX_displayData.LegendVisible = true;
            easyChartXSeries1.Color = System.Drawing.Color.Red;
            easyChartXSeries1.Marker = SeeSharpTools.JY.GUI.EasyChartXSeries.MarkerType.None;
            easyChartXSeries1.Name = "Series1";
            easyChartXSeries1.Type = SeeSharpTools.JY.GUI.EasyChartXSeries.LineType.FastLine;
            easyChartXSeries1.Visible = true;
            easyChartXSeries1.Width = SeeSharpTools.JY.GUI.EasyChartXSeries.LineWidth.Thin;
            easyChartXSeries1.XPlotAxis = SeeSharpTools.JY.GUI.EasyChartXAxis.PlotAxis.Primary;
            easyChartXSeries1.YPlotAxis = SeeSharpTools.JY.GUI.EasyChartXAxis.PlotAxis.Primary;
            this.easyChartX_displayData.LineSeries.Add(easyChartXSeries1);
            this.easyChartX_displayData.Location = new System.Drawing.Point(0, 0);
            this.easyChartX_displayData.Margin = new System.Windows.Forms.Padding(4);
            this.easyChartX_displayData.Miscellaneous.CheckInfinity = false;
            this.easyChartX_displayData.Miscellaneous.CheckNaN = false;
            this.easyChartX_displayData.Miscellaneous.CheckNegtiveOrZero = false;
            this.easyChartX_displayData.Miscellaneous.DataStorage = SeeSharpTools.JY.GUI.DataStorageType.Clone;
            this.easyChartX_displayData.Miscellaneous.DirectionChartCount = 3;
            this.easyChartX_displayData.Miscellaneous.Fitting = SeeSharpTools.JY.GUI.EasyChartX.FitType.Range;
            this.easyChartX_displayData.Miscellaneous.MarkerSize = 7;
            this.easyChartX_displayData.Miscellaneous.MaxSeriesCount = 32;
            this.easyChartX_displayData.Miscellaneous.MaxSeriesPointCount = 4000;
            this.easyChartX_displayData.Miscellaneous.ShowFunctionMenu = true;
            this.easyChartX_displayData.Miscellaneous.SplitLayoutColumnInterval = 0F;
            this.easyChartX_displayData.Miscellaneous.SplitLayoutDirection = SeeSharpTools.JY.GUI.EasyChartXUtility.LayoutDirection.LeftToRight;
            this.easyChartX_displayData.Miscellaneous.SplitLayoutRowInterval = 0F;
            this.easyChartX_displayData.Miscellaneous.SplitViewAutoLayout = true;
            this.easyChartX_displayData.Name = "easyChartX_displayData";
            this.easyChartX_displayData.SeriesCount = 0;
            this.easyChartX_displayData.Size = new System.Drawing.Size(739, 531);
            this.easyChartX_displayData.SplitView = false;
            this.easyChartX_displayData.TabIndex = 73;
            this.easyChartX_displayData.XCursor.AutoInterval = true;
            this.easyChartX_displayData.XCursor.Color = System.Drawing.Color.DeepSkyBlue;
            this.easyChartX_displayData.XCursor.Interval = 0.001D;
            this.easyChartX_displayData.XCursor.Mode = SeeSharpTools.JY.GUI.EasyChartXCursor.CursorMode.Zoom;
            this.easyChartX_displayData.XCursor.SelectionColor = System.Drawing.Color.LightGray;
            this.easyChartX_displayData.XCursor.Value = double.NaN;
            this.easyChartX_displayData.YCursor.AutoInterval = true;
            this.easyChartX_displayData.YCursor.Color = System.Drawing.Color.DeepSkyBlue;
            this.easyChartX_displayData.YCursor.Interval = 0.001D;
            this.easyChartX_displayData.YCursor.Mode = SeeSharpTools.JY.GUI.EasyChartXCursor.CursorMode.Disabled;
            this.easyChartX_displayData.YCursor.SelectionColor = System.Drawing.Color.LightGray;
            this.easyChartX_displayData.YCursor.Value = double.NaN;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_slaveCardName);
            this.groupBox1.Controls.Add(this.textBox_masterCardName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.numericUpDown_samples);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label_SampleRate);
            this.groupBox1.Controls.Add(this.numericUpDown_sampleRate);
            this.groupBox1.Controls.Add(this.label_Channel);
            this.groupBox1.Controls.Add(this.comboBox_channelNumber);
            this.groupBox1.Font = new System.Drawing.Font("SimSun", 10.05F);
            this.groupBox1.Location = new System.Drawing.Point(10, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(295, 228);
            this.groupBox1.TabIndex = 74;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Basic Param Configuration";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(6, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 14);
            this.label1.TabIndex = 145;
            this.label1.Text = "Slave Slot Number ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(6, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(140, 14);
            this.label7.TabIndex = 144;
            this.label7.Text = "Master Slot Number ";
            // 
            // numericUpDown_samples
            // 
            this.numericUpDown_samples.Location = new System.Drawing.Point(151, 167);
            this.numericUpDown_samples.Maximum = new decimal(new int[] {
            50000000,
            0,
            0,
            0});
            this.numericUpDown_samples.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_samples.Name = "numericUpDown_samples";
            this.numericUpDown_samples.Size = new System.Drawing.Size(130, 23);
            this.numericUpDown_samples.TabIndex = 74;
            this.numericUpDown_samples.Tag = "ParaConfig";
            this.numericUpDown_samples.ThousandsSeparator = true;
            this.numericUpDown_samples.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(6, 168);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 14);
            this.label3.TabIndex = 72;
            this.label3.Text = "Samples To Acquire";
            // 
            // label_SampleRate
            // 
            this.label_SampleRate.AutoSize = true;
            this.label_SampleRate.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_SampleRate.Location = new System.Drawing.Point(6, 136);
            this.label_SampleRate.Name = "label_SampleRate";
            this.label_SampleRate.Size = new System.Drawing.Size(140, 14);
            this.label_SampleRate.TabIndex = 70;
            this.label_SampleRate.Text = "Sample Rate (Sa/s) ";
            // 
            // numericUpDown_sampleRate
            // 
            this.numericUpDown_sampleRate.Location = new System.Drawing.Point(151, 134);
            this.numericUpDown_sampleRate.Maximum = new decimal(new int[] {
            250000,
            0,
            0,
            0});
            this.numericUpDown_sampleRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_sampleRate.Name = "numericUpDown_sampleRate";
            this.numericUpDown_sampleRate.Size = new System.Drawing.Size(130, 23);
            this.numericUpDown_sampleRate.TabIndex = 71;
            this.numericUpDown_sampleRate.Tag = "ParaConfig";
            this.numericUpDown_sampleRate.ThousandsSeparator = true;
            this.numericUpDown_sampleRate.Value = new decimal(new int[] {
            250000,
            0,
            0,
            0});
            // 
            // label_Channel
            // 
            this.label_Channel.AutoSize = true;
            this.label_Channel.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Channel.Location = new System.Drawing.Point(6, 104);
            this.label_Channel.Name = "label_Channel";
            this.label_Channel.Size = new System.Drawing.Size(77, 14);
            this.label_Channel.TabIndex = 68;
            this.label_Channel.Text = "Channel ID";
            // 
            // comboBox_channelNumber
            // 
            this.comboBox_channelNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_channelNumber.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_channelNumber.FormattingEnabled = true;
            this.comboBox_channelNumber.Items.AddRange(new object[] {
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
            "15"});
            this.comboBox_channelNumber.Location = new System.Drawing.Point(151, 102);
            this.comboBox_channelNumber.Name = "comboBox_channelNumber";
            this.comboBox_channelNumber.Size = new System.Drawing.Size(130, 20);
            this.comboBox_channelNumber.TabIndex = 67;
            this.comboBox_channelNumber.Tag = "ParaConfig";
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Location = new System.Drawing.Point(35, 39);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(103, 52);
            this.button_start.TabIndex = 69;
            this.button_start.Tag = "ParaConfig";
            this.button_start.Text = "Start";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(161, 39);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(102, 50);
            this.button_stop.TabIndex = 74;
            this.button_stop.Tag = "ParaConfig";
            this.button_stop.Text = "Stop";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoEllipsis = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(157)))), ((int)(((byte)(29)))), ((int)(((byte)(34)))));
            this.label5.Font = new System.Drawing.Font("SimSun", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(142, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(740, 27);
            this.label5.TabIndex = 81;
            this.label5.Text = "JYUSB1601 MultiCard Synchronization";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.easyChartX_displayData);
            this.panel1.Location = new System.Drawing.Point(0, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(739, 531);
            this.panel1.TabIndex = 83;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.textBox_phasediff);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Controls.Add(this.textBox_timediff);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(749, 80);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(310, 540);
            this.panel2.TabIndex = 84;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(16, 318);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(273, 14);
            this.label10.TabIndex = 209;
            this.label10.Text = "PhaseDiff=TimeDiff*SignalFrequency*360";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_stop);
            this.groupBox2.Controls.Add(this.button_start);
            this.groupBox2.Location = new System.Drawing.Point(18, 385);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(287, 114);
            this.groupBox2.TabIndex = 86;
            this.groupBox2.TabStop = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(15, 344);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(112, 14);
            this.label12.TabIndex = 208;
            this.label12.Text = "Phase Diff (°)";
            // 
            // textBox_phasediff
            // 
            this.textBox_phasediff.Location = new System.Drawing.Point(161, 337);
            this.textBox_phasediff.Name = "textBox_phasediff";
            this.textBox_phasediff.Size = new System.Drawing.Size(130, 21);
            this.textBox_phasediff.TabIndex = 207;
            // 
            // textBox_timediff
            // 
            this.textBox_timediff.Location = new System.Drawing.Point(161, 293);
            this.textBox_timediff.Name = "textBox_timediff";
            this.textBox_timediff.Size = new System.Drawing.Size(130, 21);
            this.textBox_timediff.TabIndex = 205;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(15, 290);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 14);
            this.label11.TabIndex = 206;
            this.label11.Text = "Time Diff(ns)";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 620);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1059, 22);
            this.statusStrip1.TabIndex = 82;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // timer_FetchData
            // 
            this.timer_FetchData.Tick += new System.EventHandler(this.timer_FetchData_Tick);
            // 
            // textBox_masterCardName
            // 
            this.textBox_masterCardName.Location = new System.Drawing.Point(151, 43);
            this.textBox_masterCardName.Name = "textBox_masterCardName";
            this.textBox_masterCardName.Size = new System.Drawing.Size(130, 23);
            this.textBox_masterCardName.TabIndex = 146;
            this.textBox_masterCardName.Text = "USBDev0";
            // 
            // textBox_slaveCardName
            // 
            this.textBox_slaveCardName.Location = new System.Drawing.Point(151, 72);
            this.textBox_slaveCardName.Name = "textBox_slaveCardName";
            this.textBox_slaveCardName.Size = new System.Drawing.Size(130, 23);
            this.textBox_slaveCardName.TabIndex = 146;
            this.textBox_slaveCardName.Text = "USBDev1";
            // 
            // AI_MultiCard_Sync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 642);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.splitter1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AI_MultiCard_Sync";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JYUSB1601 MultiCard Synchronization";
            this.Load += new System.EventHandler(this.AI_MultiCard_Sync_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_samples)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_sampleRate)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Splitter splitter1;
        private SeeSharpTools.JY.GUI.EasyChartX easyChartX_displayData;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox_channelNumber;
        private System.Windows.Forms.Label label_Channel;
        private System.Windows.Forms.Label label_SampleRate;
        private System.Windows.Forms.NumericUpDown numericUpDown_sampleRate;
        private System.Windows.Forms.NumericUpDown numericUpDown_samples;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Timer timer_FetchData;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBox_phasediff;
        private System.Windows.Forms.TextBox textBox_timediff;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_slaveCardName;
        private System.Windows.Forms.TextBox textBox_masterCardName;
    }
}

