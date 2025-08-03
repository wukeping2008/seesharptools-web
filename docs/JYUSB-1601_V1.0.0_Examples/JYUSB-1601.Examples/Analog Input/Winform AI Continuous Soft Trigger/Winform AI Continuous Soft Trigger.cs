using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single channel continuous data acquisition (software trigger)
/// Author：JYTEK
/// Modified date：2023.6.6
/// Driver version： V0.0.3
/// Installation package:  SeeSharpTools.JY.GUI 1.4.7
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and ChannelID and select AITerminal
/// 2. Input sample rate and input voltage range limit
/// 3. Send Soft Trigger
/// 4. Press the Start button and the Send Soft Trigger button start for continuous data acquisition
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
        private double[] readValue;

        /// input low limit
        /// </summary>
        private double lowRange;

        /// <summary>
        /// input high limit
        /// </summary>
        /// <returns></returns>
        private double highRange;

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
            comboBox_channelNumber.SelectedIndex = 0;
            comboBox_sampleclocksource.Items.AddRange(Enum.GetNames(typeof(AISampleClockSource)));
            comboBox_sampleclocksource.SelectedIndex = 0;
            for (int j = 0; j < JYRange.Length; j++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", JYRange[j]));
            }
            comboBox_inputRange.SelectedIndex = 0;
            groupBox_ParmConfig.Enabled = true;
            button_start.Enabled = true;
            button_sendSoftTrigger.Enabled = false;
            button_stop.Enabled = false;
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
        /// Start AITask data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //New AITask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_boardNumber.SelectedIndex.ToString());

                //Addchannel  
                aiTask.AddChannel(comboBox_channelNumber.SelectedIndex, lowRange, highRange);

                //Basic parameter configuration
                aiTask.Mode = AIMode.Continuous;
                if (comboBox_sampleclocksource.SelectedIndex == 1)
                {
                    aiTask.SampleClock.Source = AISampleClockSource.External;
                    aiTask.SampleClock.External.ExpectedRate = (double)numericUpDown_sampleRate.Value;

                }
                aiTask.Trigger.Type = AITriggerType.Soft;

                aiTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                
                try
                {
                    //Start 
                    aiTask.Start();
                }

                catch (JYDriverException ex)
                {
                    toolStripStatusLabel.Text = "AITask start failed";
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }

                readValue = new double[(int)numericUpDown_samples.Value];

                //Enable timer, disable parameter configuration button and start button, display status
                timer_FetchData.Enabled = true;
                groupBox_ParmConfig.Enabled = false;
                button_start.Enabled = false;
                button_sendSoftTrigger.Enabled = true;
                button_stop.Enabled = true;
                toolStripStatusLabel.Text = "Start data acquisition";
            }

            catch (JYDriverException ex)
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


            //Disable timer, enable parameter configuration button and start button, display status
            timer_FetchData.Enabled = false;
            groupBox_ParmConfig.Enabled = true;
            button_start.Enabled = true;
            button_sendSoftTrigger.Enabled = false;
            button_stop.Enabled = false;
            toolStripStatusLabel.Text = "Stop AITask data acquisitionTask";
        }

        /// <summary>
        /// Timer, refresh every 10ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                textBox_AvailableSamples.Text = aiTask.AvailableSamples.ToString();
                //Read data and display if the local buffer data is enough
                if (aiTask.AvailableSamples >= (ulong)readValue.Length)
                {
                    //Read data stored in readValue
                    aiTask.ReadData(ref readValue, readValue.Length, -1);
                    toolStripStatusLabel.Text = "Reading data...";
                    //Display data
                    easyChartX_readData.Plot(readValue);
                }
            }
            catch (JYDriverException ex)
            {
                toolStripStatusLabel.Text = "Failed to read data";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //Enable the timer and continue to check if the buffer data is enough
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
        }

        /// <summary>
        /// Send soft trigger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_sendSoftTrigger_Click(object sender, EventArgs e)
        {
            aiTask.SendSoftwareTrigger();
            button_start.Enabled = false;
            button_sendSoftTrigger.Enabled = false;
            button_stop.Enabled = true;
        }

        #endregion

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
    }
}

 