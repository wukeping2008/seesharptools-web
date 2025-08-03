using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.DSP.Fundamental;

/// <summary>
/// JYUSB1601 Single Channel finite mode output (soft trigger)
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and ChannelID
/// 2. Input the output rate, SamplesToAcquire, and output voltage range limits.
/// 3. Input channel waveform information, including WaveType, amplitude and frequency
/// 4. Press the Start button and the Send Soft Trigger button to start the finite mode waveform output.
/// </summary>

namespace Winform_AO_Finite_Soft_Trigger
{
    public partial class MainForm : Form
    {
        #region Private Fields

        /// <summary>
        ///AO Task
        /// </summary>
        private JYUSB1601AOTask aoTask;

        /// <summary>
        /// the Buffer of data written by the aoTask
        /// </summary>
        private double[] writeValue;

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
            comboBox_waveformType.SelectedIndex = 0;
            comboBox_channelNumber.Items.Clear();
            for (int i = 0; i < 2; i++)
            {
                comboBox_channelNumber.Items.Add(i);
            }
            comboBox_channelNumber.SelectedIndex = 0;
        }

        /// <summary>
        /// Start aoTask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //new task based on the selected Solt Number
                aoTask = new JYUSB1601AOTask(comboBox_SoltNumber.SelectedIndex.ToString());

                //AddChannel
                aoTask.AddChannel(comboBox_channelNumber.SelectedIndex);

                //Basic parameter configuration
                aoTask.Mode = AOMode.Finite;
                aoTask.UpdateRate = (double)numericUpDown_updateRate.Value;
                aoTask.SamplesToUpdate = (int)numericUpDown_samplesToUpdate.Value;


                //Software trigger configuration
                aoTask.Trigger.Type = AOTriggerType.Soft;

                //Waveform generation
                waveGeneration();

                try
                {
                    //Write data to buffer
                    aoTask.WriteData(writeValue, -1);

                    easyChartX_AO.Plot(writeValue);
                }
                catch (Exception ex)
                {
                    //Display driver error message
                    MessageBox.Show(ex.Message);
                    return;
                }
                try
                {
                    //Start Task
                    aoTask.Start();
                }
                catch (Exception ex)
                {
                    //Display driver error message
                    MessageBox.Show(ex.Message);
                    return;
                }

                //Enable timer and Stop button, disable start button and parameter configuration button
                timer_FetchData.Enabled = true;
                groupBox_genParam.Enabled = false;
                groupBox_waveConfig.Enabled = false;
                button_start.Enabled = false;
                button_sendSoftTrigger.Enabled = true;
                button_stop.Enabled = false;

            }
            catch (Exception ex)
            {
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
            aoTask.SendSoftwareTrigger();
            button_start.Enabled = false;
            button_sendSoftTrigger.Enabled = false;
            button_stop.Enabled = true;
        }

        /// <summary>
        /// Stop output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                //Determine if the task exists
                if (aoTask != null) 
                {
                    //Stop Task 
                    aoTask.Stop();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //Clear the channel that was added last time
            aoTask.Channels.Clear();

            easyChartX_AO.Clear();

            //enable the Start button and parameter configuration button,disable the Stop button
            timer_FetchData.Enabled = false;
            groupBox_genParam.Enabled = true;
            groupBox_waveConfig.Enabled = true;
            button_start.Enabled = true;
            button_sendSoftTrigger.Enabled = false;
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
                if (aoTask != null) 
                {
                    //Stop Task 
                    aoTask.Stop();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Timer, query the Task state every 100ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            //Stop aoTask if the Task is completed
            if (aoTask.WaitUntilDone(10) == true)
            {
                try
                {
                    if (aoTask != null)
                    {
                        aoTask.Stop();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                //Clear channel
                aoTask.Channels.Clear();

                //Disable timer and Stop button, enable parameter configuration button and start button
                timer_FetchData.Enabled = false;
                groupBox_genParam.Enabled = true;
                groupBox_waveConfig.Enabled = true;
                button_start.Enabled = true;
                button_sendSoftTrigger.Enabled = false;
                button_stop.Enabled = false;
            }
            else
            {
                //Task is not completed, enable the timer, continue to query whether the task is completed
                timer_FetchData.Enabled = true;
            }
        }
        #endregion

        #region Methods
        private void waveGeneration()
        {
            //Waveform setting
            writeValue = new double[aoTask.SamplesToUpdate];
            double amplitude = (double)numericUpDown_waveformAmplitude.Value;
            double frequency = (double)numericUpDown_waveformFrequency.Value;
            switch (comboBox_waveformType.SelectedIndex)
            {
                case 0:
                    Generation.SineWave(ref writeValue, amplitude, 0, frequency, aoTask.UpdateRate);//Sine wave
                    break;
                case 1:
                    Generation.SquareWave(ref writeValue, amplitude, 50, frequency, aoTask.UpdateRate);//Square wave
                    break;
                case 2:
                    Generation.UniformWhiteNoise(ref writeValue, amplitude);//White Noise
                    break;
                default:
                    return;
            }
        }
        #endregion


    }
}
