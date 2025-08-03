using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.DSP.Fundamental;
using SeeSharpTools.JY.ArrayUtility;

/// <summary>
/// JYPXIe5112 multi-channel continuous non-surrounding output
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and the number of channels
/// 2. Input update rate, update points, and output voltage range limit
/// 3. Input the waveform information for each channel, including WaveType, amplitude, frequency
/// 4. Press the Start button to start a continuous non-surrounding waveform output
/// </summary>

namespace Winform_AO_ContinuousNoWrapping_MulitiChannel
{
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        /// AO Task
        /// </summary>
        private JYUSB1601AOTask aoTask;

        /// <summary>
        ///  the Buffer of data written by the aoTask
        /// </summary>
        private double[,] writeValue;

        /// <summary>
        /// the Buffer of data after writeValue transpose, the capacity is the same as writeValue
        /// </summary>
        private double[,] displaywriteValue;

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
            for(int i=1;i<3;i++)
            {
                comboBox_channelCount.Items.Add(i);
            }
            comboBox_waveformType0.SelectedIndex = 0;
            comboBox_waveformType1.SelectedIndex = 1;

            comboBox_channelCount.SelectedIndex = 0;

            Start.Enabled = true;
            Stop.Enabled = false;
            button_Update.Enabled = false;
        }

        /// <summary>
        /// Set the enable condition for the Configure Waveform button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_channelCount_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (Convert.ToInt16(comboBox_channelCount.Text))
            {
                case 1:
                    comboBox_waveformType0.Enabled = true;
                    comboBox_waveformType1.Enabled = false;

                    numericUpDown_waveformAmplitude0.Enabled = true;
                    numericUpDown_waveformAmplitude1.Enabled = false;

                    numericUpDown_waveformFrequency0.Enabled = true;
                    numericUpDown_waveformFrequency1.Enabled = false;
                    break;
                case 2:
                    comboBox_waveformType0.Enabled = true;
                    comboBox_waveformType1.Enabled = true;

                    numericUpDown_waveformAmplitude0.Enabled = true;
                    numericUpDown_waveformAmplitude1.Enabled = true;

                    numericUpDown_waveformFrequency0.Enabled = true;
                    numericUpDown_waveformFrequency1.Enabled = true;
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Start aoTask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, EventArgs e)
        {
            try
            {
                // new task based on the selected Solt Number
                aoTask = new JYUSB1601AOTask(comboBox_SoltNumber.SelectedIndex.ToString());

                //AddChannel
                for (int i = 0; i < Convert.ToInt16(comboBox_channelCount.Text); i++)
                {
                    aoTask.AddChannel(i);
                }

                //Basic parameter configuration
                aoTask.Mode = AOMode.ContinuousNoWrapping;
                aoTask.UpdateRate = (double)numericUpDown_updateRate.Value;
                aoTask.SamplesToUpdate = (int)numericUpDown_samplesToUpdate.Value;
                //aoTask.SampleClock.Source = AOSampleClockSource.External;
                //aoTask.SampleClock.External.Terminal = ClockTerminal.PXI_Trig0;
                //aoTask.SampleClock.External.ExpectedRate = (double)numericUpDown_updateRate.Value;

                writeValue = new double[(int)numericUpDown_samplesToUpdate.Value,aoTask.Channels.Count];
                displaywriteValue = new double[writeValue.GetLength(1), writeValue.GetLength(0)];

                //Waveform generation
                WaveGenration();

                try
                {
                    //Write data to buffer
                    aoTask.WriteData(writeValue, -1);

                    easyChartX_AO.Plot(displaywriteValue);
                }
                catch (Exception ex)
                {
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }
                try
                {
                    //StartTask
                    aoTask.Start();
                }
                catch (Exception ex)
                {
                    //Drive error message display
                   MessageBox.Show(ex.Message);
                   return;
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }

            //Disable parameter configuration and start button
            timer_FetchData.Enabled = true;
            groupBox_genParam.Enabled = false;
            Start.Enabled = false;
            Stop.Enabled = true;
            button_Update.Enabled = true;
        }

        /// <summary>
        /// Stop aoTask output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Click(object sender, EventArgs e)
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

            //Disable timer and Stop button, enable Start button and parameter configuration button
            timer_FetchData.Enabled = false;
            groupBox_genParam.Enabled = true;
            Start.Enabled = true;
            Stop.Enabled = false;
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

            WaveGenration();

            timer_FetchData.Enabled = true;
        }

        /// <summary>
        /// Timer, check the buffer data every 100ms is enough, if enough, Write data to buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            if (aoTask.AvaliableLenInSamples >= writeValue.GetLength(0))
            {
                aoTask.WriteData(writeValue, -1);

                easyChartX_AO.Plot(displaywriteValue);
            }

            //Enable the timer and continue to check the buffer data.
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
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Single Channel Waveform generation
        /// </summary>
        private void singleChannelwaveGeneration()
        {
            //Waveform setting
            double[] writeValue0 = new double[(int)numericUpDown_samplesToUpdate.Value];
            double amplitude = (double)numericUpDown_waveformAmplitude0.Value;
            double frequency = (double)numericUpDown_waveformFrequency0.Value;

            switch (comboBox_waveformType0.SelectedIndex)
            {
                case 0:
                    Generation.SineWave(ref writeValue0, amplitude, 0, frequency, (int)aoTask.UpdateRate);//Sine wave
                    break;
                case 1:
                    Generation.SquareWave(ref writeValue0, amplitude, 50, frequency, (int)aoTask.UpdateRate);//Square wave
                    break;
                case 2:
                    Generation.UniformWhiteNoise(ref writeValue0, amplitude);//White Noise
                    break;
                default:
                    return;
            }
            for (int i = 0; i < (int)numericUpDown_samplesToUpdate.Value; i++)
            {
                writeValue[i,0] = writeValue0[i];
            }
            ArrayManipulation.Transpose(writeValue, ref displaywriteValue);
        }

        /// <summary>
        /// Double Channel Waveform generation
        /// </summary>
        private void TwoChannelwaveGeneration()
        {
            //Channel0 Waveform setting
            double[] writeValue0 = new double[(int)numericUpDown_samplesToUpdate.Value];
            double amplitude0 = (double)numericUpDown_waveformAmplitude0.Value;
            double frequency0 = (double)numericUpDown_waveformFrequency0.Value;

            switch (comboBox_waveformType0.SelectedIndex)
            {
                case 0:
                    Generation.SineWave(ref writeValue0, amplitude0, 0, frequency0, (int)aoTask.UpdateRate);//Sine wave
                    break;
                case 1:
                    Generation.SquareWave(ref writeValue0, amplitude0, 50, frequency0, (int)aoTask.UpdateRate);//Square wave
                    break;
                case 2:
                    Generation.UniformWhiteNoise(ref writeValue0, amplitude0);//White Noise
                    break;
                default:
                    return;
            }

            //Channel1 Waveform setting
            double[] writeValue1 = new double[(int)numericUpDown_samplesToUpdate.Value];
            double amplitude1 = (double)numericUpDown_waveformAmplitude1.Value;
            double frequency1 = (double)numericUpDown_waveformFrequency1.Value;

            switch (comboBox_waveformType1.SelectedIndex)
            {
                case 0:
                    Generation.SineWave(ref writeValue1, amplitude1, 0, frequency1, (int)aoTask.UpdateRate);//Sine wave
                    break;
                case 1:
                    Generation.SquareWave(ref writeValue1, amplitude1, 50, frequency1, (int)aoTask.UpdateRate);//Square wave
                    break;
                case 2:
                    Generation.UniformWhiteNoise(ref writeValue1, amplitude1);//White Noise
                    break;
                default:
                    return;
            }

            for (int i = 0; i < (int)numericUpDown_samplesToUpdate.Value; i++)
            {
                writeValue[i,0] = writeValue0[i];
                writeValue[i,1] = writeValue1[i];
            }
            ArrayManipulation.Transpose(writeValue, ref displaywriteValue);
        }

      
        /// <summary>
        /// AO waveform generation
        /// </summary>
        private void WaveGenration()
        {
            switch (Convert.ToInt16(comboBox_channelCount.Text))
            {
                case 1:
                    singleChannelwaveGeneration();
                    break;
                case 2:
                    TwoChannelwaveGeneration();
                    break;
                default:
                    return;
            }
        }

        #endregion

    }
}
