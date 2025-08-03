using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.ArrayUtility;

/// <summary>
/// JYUSB1601 multi-channel continuous acquisition
/// Author：JYTEK
/// Modified date：2023.6.6
/// Driver version： V0.0.3
/// Installation package:  SeeSharpTools.JY.GUI 1.4.7
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and the number of channels
/// 2. Input sample rate and input voltage range limit
/// 3. Press the start button start for continuous data collection
/// </summary>

namespace SeeSharpExample.JY.JYUSB1601
{
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        ///  AITask
        /// </summary>
        private JYUSB1601AITask aiTask;

        /// <summary>
        /// the Buffer of data acquisition by the AITask
        /// </summary>
        double[,] readValue;

        /// <summary>
        /// the Buffer of data after readValue transpose, the capacity is the same as readValue
        /// </summary>
        double[,] displayValue;

        /// input low limit
        /// </summary>
        private double lowRange;

        /// <summary>
        /// input high limit
        /// </summary>
        /// <returns></returns>
        private double highRange;

        private int channelCount;

        private double[] JYRange = new double[] { 10, 5, 2.5};


        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handler

        /// <summary>
        /// Set the default index of comboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox_boardNumber.SelectedIndex = 0;
         
            groupBox_genParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            comboBox_sampleclocksource.Items.AddRange(Enum.GetNames(typeof(AISampleClockSource)));
            comboBox_sampleclocksource.SelectedIndex = 0;
            for (int j = 0; j < JYRange.Length; j++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", JYRange[j]));
            }
            comboBox_inputRange.SelectedIndex = 0;
            checkedListBox_portChoose.Items.Clear();
            channelCount = 16;

            for (int i = 0; i < channelCount; i++)
            {
                checkedListBox_portChoose.Items.Add(string.Format("channel_{0}", i), false);
            }
            checkedListBox_portChoose.SetItemCheckState(0, CheckState.Checked);
        }

        /// <summary>
        /// select all channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_selectchannel_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox_selectchannel.Checked)
            {
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    checkedListBox_portChoose.SetItemCheckState(i, CheckState.Checked);
                }
            }
            else
            {
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    checkedListBox_portChoose.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
        }

       

        /// <summary>
        /// select input limit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_inputRange_SelectedIndexChanged(object sender, EventArgs e)
        {
                switch (comboBox_inputRange.SelectedIndex)
                {
                case 0:
                    lowRange = -10;
                    highRange = 10;
                    break;
                case 1:
                    lowRange = -5;
                    highRange = 5;
                    break;
                case 2:
                    lowRange = -2.5;
                    highRange = 2.5;
                    break;
                default:
                    lowRange = -10;
                    highRange = 10;
                    break;
            }
        }

        /// <summary>
        /// Start data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //New AITask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_boardNumber.SelectedIndex.ToString());

                //AddChannel
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    if (checkedListBox_portChoose.GetItemChecked(i))
                    {
                        aiTask.AddChannel(i,lowRange, highRange);
                    }
                }
                //Basic parameter configuration
                aiTask.Mode = AIMode.Continuous;
                aiTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                if (comboBox_sampleclocksource.SelectedIndex == 1)
                {
                    aiTask.SampleClock.Source = AISampleClockSource.External;
                    aiTask.SampleClock.External.ExpectedRate = (double)numericUpDown_sampleRate.Value;

                }
                try
                {
                    //Start data acquisition
                    aiTask.Start();
                }

                catch (JYDriverException ex)
                {
                    toolStripStatusLabel.Text = "AITask start failed";
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }

                readValue = new double[(int)numericUpDown_samples.Value, aiTask.Channels.Count];
                displayValue = new double[readValue.GetLength(1), readValue.GetLength(0)];

                //Enable timer, disable parameter configuration button and start button, display status
                timer_FetchData.Enabled = true;
                groupBox_channel.Enabled = false;
                groupBox_param.Enabled = false;
                //groupBox_genParam.Enabled = false;
                button_start.Enabled = false;
                button_stop.Enabled = true;
                toolStripStatusLabel.Text = "Start data acquisition";
            }
            catch(Exception ex)
            {
                toolStripStatusLabel.Text = "AITask start failed";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

        }

        /// <summary>
        /// Stop AITask data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                textBox_AvailableSamples.Text = "0";
                //Determine if the task exists
                if (aiTask != null) 
                {
                    //stop
                    aiTask.Stop();
                    //Clear the channel that was added last time
                    aiTask.Channels.Clear();
                }
            }
            catch (JYDriverException ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
            aiTask.Channels.Clear();
            //Disable timer, enable parameter configuration button and start button, display status
            timer_FetchData.Enabled = false;
            groupBox_channel.Enabled = true;
            groupBox_param.Enabled = true;
            //groupBox_genParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            toolStripStatusLabel.Text = "Stop AITask data acquisition";
        }

        /// <summary>
        /// Timer, read and display data every 10ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                textBox_AvailableSamples.Text = aiTask.AvailableSamples.ToString();
                //Read data and display if the local buffer data is sufficient
                if (aiTask.AvailableSamples >= (ulong)readValue.GetLength(0))
                {
                    //Read data stored in readValue
                    aiTask.ReadData(ref readValue, readValue.GetLength(0), -1);

                    toolStripStatusLabel.Text = "Reading data...";
                    easyChartX_readData.Plot(readValue,0,1, SeeSharpTools.JY.GUI.MajorOrder.Column);
                }
            }
            catch (JYDriverException ex)
            {
                timer_FetchData.Enabled = false;
                toolStripStatusLabel.Text = "Failed to read data";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //Enable timer, continue to check whether buffer data is enough
            timer_FetchData.Enabled = true;
        }

        /// <summary>
        /// close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //Determine if the task exists
                if (aiTask != null) 
                {
                    //stop
                    aiTask.Stop();
                }
            }
            catch (JYDriverException ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
                return;
            }
            timer_FetchData.Enabled = false;
        }

        private void comboBox_sampleclocksource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_sampleclocksource.SelectedIndex == 0)
            {
                label_SampleRate.Text = "Sampling Rate(Sa/s)";
            }
            else
            {
                label_SampleRate.Text = "External Clock Rate(Sa/s)";
            }
        }
        #endregion

        #region Methods
        #endregion

        
    }
}
