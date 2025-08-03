using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.DSP.Utility;

/// <summary>
/// JYUSB1601 AI MultiCard synchronization
/// Author：JYTEK
/// Modified date：2023.6.6
/// Driver version： JYUSB1601 Installer_V0.0.3.msi
/// Installation package:  SeeSharpTools.JY.GUI 1.4.7
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Mastercard and slavecard Solt Number and ChannelID 
/// 2. Input sample rate and sample to acquire
/// 3. Set the trigger and sync param
/// 4. Press the Start button to start task
/// </summary>

namespace SeeSharpExample.JY.JYUSB1601
{
    public partial class AI_MultiCard_Sync : Form
    {
        #region Private Fields
        /// <summary>
        ///  AITask
        /// </summary>
        private JYUSB1601AITask masterTask;

        /// <summary>
        ///  AITask
        /// </summary>
        private JYUSB1601AITask slaveTask;

        /// <summary>
        /// the Buffer of data acquisition by the AITask，Sample points with capacity of 100ms
        /// </summary>
        private double[] masterReadValue;

        /// <summary>
        /// the Buffer of data acquisition by the AITask，Sample points with capacity of 100ms
        /// </summary>
        private double[] slaveReadValue;

        private double[,] displayData;

        /// <summary>
        /// Phase difference result
        /// </summary>
        private double diffResult;

        /// <summary>
        /// 主卡波形信息获取
        /// </summary>
        ToneInfo masterToneInfo;

        /// <summary>
        /// 从卡波形信息获取
        /// </summary>
        ToneInfo slaveToneInfo;

        /// <summary>
        /// phase diff
        /// </summary>
        double phasediff;

        #endregion

        #region Constructor

        public AI_MultiCard_Sync()
        {
            InitializeComponent();
        }

        #endregion

        # region Event Handler
        /// <summary>
        /// load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AI_MultiCard_Sync_Load(object sender, EventArgs e)
        {
            comboBox_channelNumber.SelectedIndex = 0;         
            button_start.Enabled = true;
            button_stop.Enabled = false;
        }

        /// <summary>
        /// start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            //get values from the form
            toolStripStatusLabel.Text = "configuration parameter";
            try
            {
                //New AITask based on the selected Solt Number
                masterTask = new JYUSB1601AITask(textBox_masterCardName.Text);

                //AddChannel
                masterTask.AddChannel(comboBox_channelNumber.SelectedIndex);

                //Basic parameter configuration
                masterTask.Mode =  AIMode.Finite;
                masterTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                masterTask.SamplesToAcquire = (int)numericUpDown_samples.Value;
                masterTask.Trigger.Type = AITriggerType.Immediate;
                masterTask.SignalExport.Add(AISignalExportSource.SampleClock, SignalExportDestination.DIO_15);
                masterTask.SignalExport.Add(AISignalExportSource.StartTrig, SignalExportDestination.DIO_0);
                
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "masterTask creat failed";
                button_start.Enabled = true;
                button_stop.Enabled = false;
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                //New AITask based on the selected Solt Number
                slaveTask = new JYUSB1601AITask(textBox_slaveCardName.Text);
                slaveTask.AddChannel(comboBox_channelNumber.SelectedIndex);

                //Basic parameter configuration
                slaveTask.Mode = AIMode.Finite;
                slaveTask.SampleRate = (double)numericUpDown_sampleRate.Value;
                slaveTask.SamplesToAcquire = (int)numericUpDown_samples.Value;

                slaveTask.SampleClock.Source = AISampleClockSource.External;

                slaveTask.SampleClock.External.ExpectedRate = (double)numericUpDown_sampleRate.Value;

                slaveTask.Trigger.Type = AITriggerType.Digital;

                slaveTask.Trigger.Digital.Source = AIDigitalTriggerSource.DIO_0;

            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "slaveTask creat failed";
                button_start.Enabled = true;
                button_stop.Enabled = false;
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
            try
            {
                slaveTask.Start();
            }
            catch (Exception ex)
            {
                slaveTask.Stop();
                toolStripStatusLabel1.Text = "SlaveTask start failed";
                button_start.Enabled = true;
                button_stop.Enabled = false;
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
            //Thread.Sleep(500);
            try
            {
                masterTask.Start();
            }
            catch (Exception ex)
            {
                masterTask.Stop();
                toolStripStatusLabel1.Text = "MasterTask start failed";
                button_start.Enabled = true;
                button_stop.Enabled = false;
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
            // New data buffer
            masterReadValue = new double[(int)numericUpDown_samples.Value];
            slaveReadValue = new double[(int)numericUpDown_samples.Value];
            displayData = new double[2, (int)numericUpDown_samples.Value];

            //Enable timer, disable parameter configuration button and start button, display status
            timer_FetchData.Enabled = true;
            button_start.Enabled = false;
            button_stop.Enabled = true;
            toolStripStatusLabel.Text = "Wait for trigger signal...";
        }

        /// <summary>
        /// read data and cal phase difff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;
            //Read data and display if the local buffer data is enough
            if ((masterTask.AvailableSamples >= (ulong)masterReadValue.Length) &&(slaveTask.AvailableSamples >= (ulong)slaveReadValue.Length))
            {
                try
                {
                    // Read data  
                    masterTask.ReadData(ref masterReadValue, masterReadValue.Length, 10000);
                    slaveTask.ReadData(ref slaveReadValue, slaveReadValue.Length, 10000);

                    // Display data and phase
                    for (int i = 0; i < masterReadValue.Length; i++)
                    {
                        displayData[0, i] = masterReadValue[i];
                        displayData[1, i] = slaveReadValue[i];
                    }
                    toolStripStatusLabel.Text = "trigger complete";

                    easyChartX_displayData.Plot(displayData);

                    // Get signal waveform information
                    masterToneInfo = ToneAnalyzer.SingleToneAnalysis(masterReadValue, masterTask.SampleRate);
                    slaveToneInfo = ToneAnalyzer.SingleToneAnalysis(slaveReadValue, slaveTask.SampleRate);
                    // Calculate the phase difference of two signals
                    phasediff = (masterToneInfo.Phase - slaveToneInfo.Phase) / (2.0 * Math.PI);
                    phasediff -= Math.Round(phasediff);
                    phasediff *= 360;
                    // Calculate the absolute time difference between two signals
                    diffResult = (phasediff / (masterToneInfo.Frequency * 360)) * 1e9;
                    textBox_timediff.Text = diffResult.ToString("0.0000");
                    textBox_phasediff.Text = phasediff.ToString();

                    slaveTask.Stop();
                    masterTask.Stop();

                    button_start.Enabled = true;
                    button_stop.Enabled = false;
                    timer_FetchData.Enabled = false;
                }
                catch (Exception ex)
                {
                    toolStripStatusLabel1.Text = "Read data failed";
                    button_start.Enabled = true;
                    button_stop.Enabled = false;
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else
            {
                timer_FetchData.Enabled = true;
            }
        }

        /// <summary>
        /// stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            toolStripStatusLabel.Text = "Stop AI multiCard synchronization task";
            slaveTask?.Stop();
            masterTask?.Stop();
        }

        #endregion-----------------

        #region Methods
        #endregion----------------------------------
    }
}
