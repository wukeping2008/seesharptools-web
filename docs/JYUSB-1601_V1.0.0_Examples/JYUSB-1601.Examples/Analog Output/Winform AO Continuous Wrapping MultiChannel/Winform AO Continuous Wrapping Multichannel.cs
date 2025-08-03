using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.DSP.Fundamental;
using SeeSharpTools.JY.ArrayUtility;

/// <summary>
/// JYUSB1601 multi-channel continuous Wrapping output
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and the number of channels
/// 2. Input the update rate, update points, and output voltage range limits
/// 3. Input the waveform information for each channel, including WaveType, amplitude, frequency
/// 4. Press the Start button to start continuous Wrapping waveform output
/// </summary>

namespace Winform_AO_Continuous_Wrapping_MulitiChannel
{
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        ///AO Task
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
            comboBox_channelCount.Items.Clear();
            for (int i=1;i<3;i++)
            {
                comboBox_channelCount.Items.Add(i);
            }
            comboBox_channelCount.SelectedIndex = 0;
            comboBox_waveformType0.SelectedIndex = 0;
            comboBox_waveformType1.SelectedIndex = 1;
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
                //new task based on the selected Solt Number
                aoTask = new JYUSB1601AOTask(comboBox_SoltNumber.SelectedIndex.ToString());

                //AddChannel
                for (int i = 0; i <Convert.ToInt16(comboBox_channelCount.Text); i++)
                {
                    aoTask.AddChannel(i);
                }

                //Basic parameter configuration
                aoTask.Mode = AOMode.ContinuousWrapping;
                aoTask.UpdateRate = (double)numericUpDown_updateRate.Value;
                aoTask.SamplesToUpdate = (int)numericUpDown_samplesToUpdate.Value;
                writeValue = new double[(int)numericUpDown_samplesToUpdate.Value,aoTask.Channels.Count];
                displaywriteValue = new double[writeValue.GetLength(1), writeValue.GetLength(0)];

                //Generate waveform
                WaveGenration();

                try
                {
                    //Write data to buffer
                    aoTask.WriteData(writeValue, -1);

                    easyChartX_AO.Plot(displaywriteValue);

                }
                catch (Exception ex)
                {
                    //Display driver error message
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
                    //Display driver error message
                   MessageBox.Show(ex.Message);
                   return;
                }

            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
               return;
            }

            //Disable parameter configuration button and start button
            groupBox_genParam.Enabled = false;
            groupBox_waveConfig.Enabled = false;
            Start.Enabled = false;
            Stop.Enabled = true;
        }

        /// <summary>
        /// Stop aoTask
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


            //enable the parameter configuration button and start button, disable the Stop button
            groupBox_genParam.Enabled = true;
            groupBox_waveConfig.Enabled = true;
            Start.Enabled = true;
            Stop.Enabled = false;
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
            for (int i = 0; i < writeValue0.Length; i++)
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

            for (int i = 0; i < writeValue0.Length; i++)
            {
                writeValue[i,0] = writeValue0[i];
                writeValue[i,1] = writeValue1[i];
            }
            ArrayManipulation.Transpose(writeValue, ref displaywriteValue);

        }

        #endregion

    }
}
