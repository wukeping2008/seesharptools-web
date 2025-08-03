using System;
using System.Windows.Forms;
using JYUSB1601;
using System.Collections.Generic;

/// <summary>
/// JYUSB1601 Single channel finite mode data acquisition (software trigger)
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and ChannelID
/// 2. Input sample rate, SamplesToAcquire, and input voltage range limits
/// 3. Press the Start button and the Send Soft Trigger button to start finite data acquisition.
/// </summary>

namespace Winform_AI_Finite_Soft_Trigger
{
    public partial class MainForm : Form
    {
        #region Private Fields

        /// <summary>
        /// aiTask
        /// </summary>
        private JYUSB1601AITask aiTask;

        /// <summary>
        ///  the Buffer of data acquisition by the aiTask
        /// </summary>
        private double[,] readValue;

        /// input low limit
        /// </summary>
        private double lowRange;

        /// <summary>
        /// input high limit
        /// </summary>
        /// <returns></returns>
        private double highRange;

        /// <summary>
        ///  retriggercount that have been completed
        /// </summary>
        private int finishedSendTriggerCount;

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
        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox_SlotNumber.SelectedIndex = 0;
            for (int i = 0; i < AIRange.Length; i++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", AIRange[i]));
            }
            comboBox_inputRange.SelectedIndex = 0;
            comboBox_sampleclocksource.Items.AddRange(Enum.GetNames(typeof(AISampleClockSource)));
            comboBox_sampleclocksource.SelectedIndex = 0;
            Channels.Items.Clear();
            
            for (int i = 0; i < 16; i++)
            {
                Channels.Items.Add(string.Format("Channel_{0}", i));
            }
           
            Channels.SetItemChecked(0, true);
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
                case 3:
                    lowRange = -1.25;
                    highRange = 1.25;
                    break;
                case 4:
                    lowRange = -0.625;
                    highRange = 0.625;
                    break;
                default:
                    lowRange = -10;
                    highRange = 10;
                    break;
            }
        }

        /// <summary>
        /// Start aiTask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //New aiTask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_SlotNumber.SelectedIndex.ToString());
                List<int> CheckedChannels = new List<int>();
                //AddChannel
                for (int i = 0; i < Channels.Items.Count; i++)
                {
                    if (Channels.GetItemChecked(i))
                    {
                        CheckedChannels.Add(i);
                    }
                }
                for (int i = 0; i < CheckedChannels.Count; i++)
                {
                    aiTask.AddChannel(CheckedChannels[i], lowRange, highRange);
                }
                easyChartX_readData.Clear();
                easyChartX_readData.Series.Clear();
                for (int i = 0; i < CheckedChannels.Count; i++)
                {
                    easyChartX_readData.Series.Add(new SeeSharpTools.JY.GUI.EasyChartXSeries());
                    easyChartX_readData.Series[i].Name = string.Format("Ch{0}", CheckedChannels[i]);
                }
                //Basic parameter configuration
                aiTask.Mode = AIMode.Finite;
                aiTask.SamplesToAcquire = (int)numericUpDown_samples.Value;
                aiTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                if (comboBox_sampleclocksource.SelectedIndex == 1)
                {
                    aiTask.SampleClock.Source = AISampleClockSource.External;
                    aiTask.SampleClock.External.ExpectedRate = (double)numericUpDown_sampleRate.Value;

                }
                //Trigger parameter configuration
                aiTask.Trigger.Type = AITriggerType.Soft;
              
                try
                {
                    //Start data acquisition
                    aiTask.Start();
                }

                catch (JYDriverException ex)
                {
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }

                readValue = new double[(int)numericUpDown_samples.Value, aiTask.Channels.Count];

                //Enable timer, disable start button and parameter configuration button
                toolStripStatusLabel.Text = "Waiting to receive the trigger signal...";
                timer_FetchData.Enabled = true;
                button_start.Enabled = false;
                button_sendSoftTrigger.Enabled = true;
                button_stop.Enabled = true;
                groupBox_anaInParam.Enabled = false;

            }
            catch (JYDriverException ex)
            {
                toolStripStatusLabel.Text = "aiTask start failed";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Send Soft Trigger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_sendSoftTrigger_Click(object sender, EventArgs e)
        {
            aiTask.SendSoftwareTrigger();

            timer_FetchData.Enabled = true;
            button_start.Enabled = false;
            button_sendSoftTrigger.Enabled = true;
            button_stop.Enabled = true;
      
            button_sendSoftTrigger.Enabled = false;
            
        }

        /// <summary>
        /// Stop data acquisition
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
            catch (JYDriverException ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //Disable timer and Stop button, enable parameter configuration and start button
            timer_FetchData.Enabled = false;
            button_start.Enabled = true;
            button_sendSoftTrigger.Enabled = false;
            button_stop.Enabled = false;
            groupBox_anaInParam.Enabled = true;
        }

        /// <summary>
        ///Timer, check the buffer readable points every 10ms, if enough, read the data and display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void time_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                //Read data and display if the local buffer data is enough
                if (aiTask.AvailableSamples >= (ulong)readValue.GetLength(0))
                {
                    aiTask.ReadData(ref readValue);
                    easyChartX_readData.Plot(readValue,0,1,SeeSharpTools.JY.GUI.MajorOrder.Column);

                    try
                    {
                        //Determine if the task exists
                        if (aiTask != null)
                        {
                            //Stop Task 
                            aiTask.Stop();
                        }
                    }
                    catch (JYDriverException ex)
                    {
                        //Drive error message display
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    //Clear the channel that was added last time
                    aiTask.Channels.Clear();

                    //Disable timer, enable Start button and parameter configuration button
                    timer_FetchData.Enabled = false;
                    button_start.Enabled = true;
                    button_stop.Enabled = false;
                    groupBox_anaInParam.Enabled = true;
                   
                }
                else
                {
                    timer_FetchData.Enabled = true;
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
            catch (JYDriverException ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
        }


        #endregion

        #region  Methods
        #endregion

       

        private void checkBox_selectchannel_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_selectchannel.Checked == true)
            {
                for (int i = 0; i < Channels.Items.Count; i++)
                {
                    Channels.SetItemChecked(i, true);
                }
            }
            else
            {
                for (int i = 0; i < Channels.Items.Count; i++)
                {
                    Channels.SetItemChecked(i, false);
                }
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
    }
}
