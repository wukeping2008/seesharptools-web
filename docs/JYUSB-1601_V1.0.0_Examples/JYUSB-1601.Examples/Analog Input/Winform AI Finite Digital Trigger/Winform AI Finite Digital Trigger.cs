using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 Single channel finite mode data acquisition (digital trigger)
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and ChannelID
/// 2. Input sample rate, SamplesToAcquire, and input voltage range limits
/// 3. Input the trigger parameter
/// 4. Press the Start button to start finite data acquisition
/// </summary>

namespace Winform_AI_Finite_Digital_Trigger
{
    public partial class MainForm : Form
    {
        #region private Fields
        /// <summary>
        /// aiTask
        /// </summary>
        private JYUSB1601AITask aiTask;

        /// <summary>
        ///  the Buffer of data acquisition by the aiTask
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

        private double[] AIRange = new double[] { 10, 5, 2.5};
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
        private void MainForm_Load(object sender, System.EventArgs e)
        {
            comboBox_SoltNumber.SelectedIndex = 0;
            for (int i = 0; i < AIRange.Length; i++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", AIRange[i]));
            }
            comboBox_inputRange.SelectedIndex = 0;
            //Call the enumeration of AIDigitalTriggerEdge in the driver as a menu
            comboBox_sampleclocksource.Items.AddRange(Enum.GetNames(typeof(AISampleClockSource)));
            comboBox_sampleclocksource.SelectedIndex = 0;
            comboBox_triggerEdge.Items.AddRange(Enum.GetNames(typeof(AIDigitalTriggerEdge)));
            comboBox_triggerEdge.SelectedIndex = 0;

            comboBox_triggerSource.Items.AddRange(Enum.GetNames(typeof(AIDigitalTriggerSource)));
            comboBox_triggerSource.SelectedIndex = 0;

            comboBox_channelNumber.Items.Clear();
           
            for (int i = 0; i < 16; i++)
            {
                comboBox_channelNumber.Items.Add(i);
            }
                   
            comboBox_channelNumber.SelectedIndex = 0;

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
        /// Set the enable condition for configuring the  pretriggersamples
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       

        /// <summary>
        /// Start data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {

                //New aiTask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_SoltNumber.SelectedIndex.ToString());

                //AddChannel
                aiTask.AddChannel(comboBox_channelNumber.SelectedIndex, lowRange, highRange);

                //Basic parameter configuration
                aiTask.Mode = AIMode.Finite;
                aiTask.SamplesToAcquire = (int)numericUpDown_samples.Value;
                aiTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                if (comboBox_sampleclocksource.SelectedIndex == 1)
                {
                    aiTask.SampleClock.Source = AISampleClockSource.External;
                    aiTask.SampleClock.External.ExpectedRate = (double)numericUpDown_sampleRate.Value;

                }
                aiTask.Trigger.Type = AITriggerType.Digital;
                
                aiTask.Trigger.Digital.Edge = (AIDigitalTriggerEdge)Enum.Parse(typeof(AIDigitalTriggerEdge), comboBox_triggerEdge.Text, true);
                aiTask.Trigger.Digital.Source = (AIDigitalTriggerSource)Enum.Parse(typeof(AIDigitalTriggerSource), comboBox_triggerSource.Text, true);

                try
                {
                    //Start
                    aiTask.Start();
                }
                catch (Exception ex)
                {
                    //Drive error message display
                   MessageBox.Show(ex.Message);
                   return;
                }

                readValue = new double[(int)numericUpDown_samples.Value];

                //Enable timer, disable parameter configuration button and start button, display status
                timer_FetchData.Enabled = true;
                groupBox_genParam.Enabled = false;
                groupBox_TrigParam.Enabled = false;
                button_start.Enabled = false;
                button_stop.Enabled = true;
                toolStripStatusLabel.Text = "Waiting to receive the trigger signal...";

            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = "aiTask start failed";
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
        }

        /// <summary>
        /// Timer, refresh every 10ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, System.EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                //Read data and display if the local buffer data is enough
                if (aiTask.AvailableSamples >= (ulong)readValue.Length)
                {
                    aiTask.ReadData(ref readValue);
                    toolStripStatusLabel.Text = "Reading Data...";
                    easyChartX_readData.Plot(readValue);


                    try
                    {
                        //Determine if the task exists
                        if (aiTask != null) 
                        {
                            //Stop Task 
                            aiTask.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        //Drive error message display
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    //Clear the channel that was added last time
                    aiTask.Channels.Clear();
                   
                    //Disable timer, enable Start button and parameter configuration button
                    timer_FetchData.Enabled = false;
                    groupBox_genParam.Enabled = true;
                    groupBox_TrigParam.Enabled = true;
                    button_start.Enabled = true;
                    button_stop.Enabled = false;

                }
                else
                {
                    timer_FetchData.Enabled = true;
                }
            }

            catch (Exception ex)
            {
                timer_FetchData.Enabled = false;
                toolStripStatusLabel.Text = "Failed to read data";
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
        }

        /// <summary>
        /// Stop aiTask data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                //Determine if the task exists
                if (aiTask != null) 
                {
                    //Stop Task 
                    aiTask.Stop();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //Disable timer, enable Start button and parameter configuration button
            timer_FetchData.Enabled = false;
            groupBox_genParam.Enabled = true;
            groupBox_TrigParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
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
                    //Stop Task 
                    aiTask.Stop();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
                return;
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
