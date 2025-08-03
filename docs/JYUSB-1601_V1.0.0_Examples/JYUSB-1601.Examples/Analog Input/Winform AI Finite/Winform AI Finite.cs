using System;
using System.Windows.Forms;
using JYUSB1601;
using System.Collections.Generic;

/// <summary>
/// JYUSB1601 Single channel finite mode data acquisition
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and ChannelID
/// 2. Input sample rate, SamplesToAcquire, and input voltage range limits
/// 3. Press the Start button to start finite data acquisition
/// </summary>

namespace Winform_AI_Finite
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
            comboBox_SoltNumber.SelectedIndex = 0;
            comboBox_sampleclocksource.Items.AddRange(Enum.GetNames(typeof(AISampleClockSource)));
            comboBox_sampleclocksource.SelectedIndex = 0;
            for (int j = 0; j < JYRange.Length; j++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", JYRange[j]));
            }
            comboBox_inputRange.SelectedIndex = 0;
            Channels.Items.Clear();
            for (int i = 0; i < 16; i++)
            {
                Channels.Items.Add(string.Format("channel_{0}", i));

            }
            Channels.SetItemCheckState(0, CheckState.Checked);

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

        /// <summary>
        /// Start aiTask data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            //addchannel
            List<int> CheckedChannels = new List<int>();
            for (int i = 0; i < Channels.Items.Count; i++)
            {
                if (Channels.GetItemChecked(i))
                {
                    CheckedChannels.Add(i);
                }
            }
            easyChartX_readData.Clear();
            easyChartX_readData.Series.Clear();
            for (int i = 0; i < CheckedChannels.Count; i++)
            {
                easyChartX_readData.Series.Add(new SeeSharpTools.JY.GUI.EasyChartXSeries());
                easyChartX_readData.Series[i].Name = string.Format("Ch{0}", CheckedChannels[i]);
            }

            try
            {
                //New aiTask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_SoltNumber.SelectedIndex.ToString());
                //AddChannel
                for (int i = 0; i < CheckedChannels.Count; i++)
                {
                    aiTask.AddChannel(CheckedChannels[i], lowRange, highRange);
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
                try
                {
                    //Start data acquisition
                    aiTask.Start();
                }
                catch (Exception ex)
                {
                    //Drive error message display
                   MessageBox.Show(ex.Message);
                   return;
                }

                readValue = new double[(int)numericUpDown_samples.Value, aiTask.Channels.Count];

                //Enable timer, disable start button and parameter configuration button
                time_FetchData.Enabled = true;
                button_start.Enabled = false;
                button_stop.Enabled = true;

            }
            catch (Exception ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        ///Timer, check the buffer readable points every 10ms, if enough, read the data and display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void time_FetchData_Tick(object sender, EventArgs e)
        {
            time_FetchData.Enabled = false;

            try
            {
                if (aiTask.AvailableSamples >= (ulong)readValue.GetLength(0))
                {
                    //Read  data
                    aiTask.ReadData(ref readValue, readValue.GetLength(0),-1);
                    toolStripStatusLabel.Text = "Reading Data...";
                    easyChartX_readData.Plot(readValue, 0,1, SeeSharpTools.JY.GUI.MajorOrder.Column);

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
                    time_FetchData.Enabled = false;
                    
                    button_start.Enabled = true;
                    button_stop.Enabled = false;
                }
                else
                {
                    time_FetchData.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
                toolStripStatusLabel.Text = "Failed to read data";
                return;
            }
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
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }

            //Disable timer and Stop buttons, enable parameter configuration button and start button
            time_FetchData.Enabled = false;
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


        #endregion

        #region  Methods
        #endregion


    }
}
