using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.DSP.Fundamental;

/// <summary>
/// JYUSB1601 single channel continuous NOWrapping output
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and ChannelID
/// 2. Input the update rate, update points, and output voltage range limits
/// 3. Press the Start button to start continuous waveform output
///
/// No-Wrappinged output means that the output waveform can be updated by continuously writing new data to the buffer while the program is running.
/// </summary>

namespace Winform_AO_Continuous_NoWrapping
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

        /// <summary>
        /// Output waveform amplitude
        /// </summary>
        private double amplitude;

        /// <summary>
        /// Output Waveform frequency
        /// </summary>
        private double frequency;

        /// output low limit
        /// </summary>
        private double lowRange;

        /// <summary>
        /// output high limit
        /// </summary>
        /// <returns></returns>
        private double highRange;
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
            for(int i=0;i<2;i++)
            {
                comboBox_channelNumber.Items.Add(i);
            }
            comboBox_channelNumber.SelectedIndex = 0;

            comboBox_waveformType.SelectedIndex = 0;


            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_Update.Enabled = false;
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
                //new aoTask based on the selected Solt Number
                aoTask = new JYUSB1601AOTask(comboBox_SoltNumber.SelectedIndex.ToString());

                //AddChannel
                aoTask.AddChannel(comboBox_channelNumber.SelectedIndex);

                //Basic parameter configuration
                aoTask.Mode = AOMode.ContinuousNoWrapping;
                aoTask.UpdateRate = (double)numericUpDown_updateRate.Value;

                writeValue = new double[(int)numericUpDown_samplesToUpdate.Value];
                aoTask.SamplesToUpdate = (int)numericUpDown_samplesToUpdate.Value;
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
                    //StartAOtask
                    aoTask.Start();
                }
                catch (Exception ex)
                {
                    //Display driver error message
                   MessageBox.Show(ex.Message);
                   return;
                }
                
            }
            catch (Exception ex)
            {
                //Display driver error message
               MessageBox.Show(ex.Message);
               return;
            }

            //Enable timer, disable parameter configuration button and start button
            timer_FetchData.Enabled = true;
            groupBox_genParam.Enabled = false;
            button_start.Enabled = false;
            button_stop.Enabled = true;
            button_Update.Enabled = true;
        }

        /// <summary>
        /// Stop aoTask
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

            //Disable timer, enable Start button and parameter configuration button
            timer_FetchData.Enabled = false;
            groupBox_genParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_Update.Enabled = false;
        }


        /// <summary>
        /// Update Signal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Update_Click(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            waveGeneration();
            easyChartX_AO.Plot(writeValue);
            timer_FetchData.Enabled = true;
        }

        /// <summary>
        /// Timer, check the buffer data every 100 milliseconds is enough, if enough, Write data to buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            if(aoTask.AvaliableLenInSamples>= writeValue.Length)
            {
                aoTask.WriteData(writeValue, -1);
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
        #endregion

        #region Methods
        /// <summary>
        /// Waveform generation
        /// </summary>
        private void waveGeneration()
        {
            //Waveform setting
            amplitude = (double)numericUpDown_waveformAmplitude.Value;
            frequency = (double)numericUpDown_waveformFrequency.Value;
            switch (comboBox_waveformType.SelectedIndex)
            {
                case 0:
                    //Sine wave
                    Generation.SineWave(ref writeValue, amplitude, 0, frequency, aoTask.UpdateRate);
                    break;
                case 1:
                    //Square wave
                    Generation.SquareWave(ref writeValue, amplitude, 50, frequency, aoTask.UpdateRate);
                    break;
                case 2:
                    //White Noise
                    Generation.UniformWhiteNoise(ref writeValue, amplitude);
                    break;
                default:
                    return;
            }
        }

        #endregion


    }
}
