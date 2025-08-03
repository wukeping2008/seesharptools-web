using System;
using System.Windows.Forms;
using JYUSB1601;

///<summary>
/// JYUSB1601 single channel continuous data acquisition (digital trigger)
/// Author：JYTEK
/// Modified date：2023.6.6
/// Driver version： V0.0.3
/// Installation package:  SeeSharpTools.JY.GUI 1.4.7
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and channel number
/// 2. Input sample rate and input voltage range limit
/// 3. Input numeric trigger parameters, including trigger source and trigger edge
/// 4. Press the start button to start continuous data collection
/// </summary>
/// 
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

        private double[] JYRange = new double[] { 10, 5, 2.5 };

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

            //Call the enumeration of AIDigitalTriggerEdge in the driver as a menu
            comboBox_triggerEdge.Items.AddRange(Enum.GetNames(typeof(AIDigitalTriggerEdge)));
            comboBox_triggerEdge.SelectedIndex = 0;
            comboBox_sampleclocksource.Items.AddRange(Enum.GetNames(typeof(AISampleClockSource)));
            comboBox_sampleclocksource.SelectedIndex = 0;

            comboBox_triggerSource.Items.AddRange(Enum.GetNames(typeof(AIDigitalTriggerSource)));
            comboBox_triggerSource.SelectedIndex = 0;

            groupBox_ParamConfig.Enabled = true;
            groupBox_TrigParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            for (int j = 0; j < JYRange.Length; j++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", JYRange[j]));
            }
            comboBox_inputRange.SelectedIndex = 0;
        }

        /// <summary>
        /// Set CardID Basic Param
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_cardID_SelectedIndexChanged(object sender, EventArgs e)
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

                //Addchannel  
                aiTask.AddChannel(comboBox_channelNumber.SelectedIndex, lowRange, highRange);

                //Basic parameter configuration
                aiTask.Mode = AIMode.Continuous;
                aiTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                if (comboBox_sampleclocksource.SelectedIndex == 1)
                {
                    aiTask.SampleClock.Source = AISampleClockSource.External;
                    aiTask.SampleClock.External.ExpectedRate = (double)numericUpDown_sampleRate.Value;

                }
                //Trigger parameter configuration
                aiTask.Trigger.Type = AITriggerType.Digital;
                aiTask.Trigger.Digital.Edge = (AIDigitalTriggerEdge)Enum.Parse(typeof(AIDigitalTriggerEdge), comboBox_triggerEdge.Text, true);

                aiTask.Trigger.Digital.Source = (AIDigitalTriggerSource)Enum.Parse(typeof(AIDigitalTriggerSource), comboBox_triggerSource.Text, true);

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
                groupBox_ParamConfig.Enabled = false;
                groupBox_TrigParam.Enabled = false;
                button_start.Enabled = false;
                button_stop.Enabled = true;
                toolStripStatusLabel.Text = "Wait for trigger signal...";
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
            groupBox_ParamConfig.Enabled = true;
            groupBox_TrigParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            toolStripStatusLabel.Text = "Stop AITask data acquisition";
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
                if (aiTask.AvailableSamples>= (ulong)readValue.Length)
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
                timer_FetchData.Enabled = false;
                toolStripStatusLabel.Text = "Failed to read data";
                //Drive error message display
               MessageBox.Show(ex.Message);return;
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
            }
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
    }
}
