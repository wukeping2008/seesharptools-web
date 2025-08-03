using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.DSP.Fundamental;

/// <summary>
/// JYUSB1601 Single Channel finite mode output (digital trigger)
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and ChannelID
/// 2. Input update rate, SamplesToAcquire, and output voltage range limits
/// 3. Input channel waveform information, including WaveType, amplitude and frequency
/// 4 Input numeric trigger parameters, including trigger source and trigger edge
/// 5. Press the Start button to start the finite point waveform output
/// <summary>

namespace Winform_AO_Finite_Digital_Trigger
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
            for(int i=0;i<2;i++)
            {
                comboBox_channelNumber.Items.Add(i);
            }
            comboBox_waveformType.SelectedIndex = 0;

            //Call the enumeration of AODigitalTriggerEdge in the driver as a menu
            comboBox_triggerEdge.Items.AddRange(Enum.GetNames(typeof(AODigitalTriggerEdge)));
            comboBox_triggerEdge.SelectedIndex = 0;

            comboBox_triggerSource.Items.AddRange(Enum.GetNames(typeof(AODigitalTriggerSource)));
            comboBox_triggerSource.SelectedIndex = 0;
            //Call the enumeration of AODigitalTriggerSource in the driver as a menu

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


                //Trigger parameter configuration
                aoTask.Trigger.Type = AOTriggerType.Digital;
                aoTask.Trigger.Digital.Edge = (AODigitalTriggerEdge)Enum.Parse(typeof(AODigitalTriggerEdge), comboBox_triggerEdge.Text, true);
                aoTask.Trigger.Digital.Source = (AODigitalTriggerSource)Enum.Parse(typeof(AODigitalTriggerSource), comboBox_triggerSource.Text, true);


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
            timer_FetchData.Enabled = true;
            groupBox_genParam.Enabled = false;
            groupBox_waveConfig.Enabled = false;
            groupBox_TrigPara.Enabled = false;
            button_start.Enabled = false;
            button_stop.Enabled = true;
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

            //enable the Start button and parameter configuration button,disable the Stop button
            timer_FetchData.Enabled = false;
            groupBox_genParam.Enabled = true;
            groupBox_waveConfig.Enabled = true;
            groupBox_TrigPara.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
        }

        /// <summary>
        /// Timer, query the Task state every 100ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            //If the Task is completed, execute stop and restart the Start parameter button
            if (aoTask.WaitUntilDone(10) == true)
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

                //Clear channel
                aoTask.Channels.Clear();

                timer_FetchData.Enabled = false;
                groupBox_genParam.Enabled = true;
                groupBox_waveConfig.Enabled = true;
                groupBox_TrigPara.Enabled = true;
                button_start.Enabled = true;
                button_stop.Enabled = false;
            }
            else
            {
                //Task is not completed, enable timer, continue to query
                timer_FetchData.Enabled = true;
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
                if (aoTask != null) 
                {
                    //Stop Task 
                    aoTask.Stop();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);return;
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
