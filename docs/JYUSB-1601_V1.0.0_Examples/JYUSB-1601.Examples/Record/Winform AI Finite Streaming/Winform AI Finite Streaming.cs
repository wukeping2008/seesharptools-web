using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.ArrayUtility;

/// <summary>
/// JYUSB1601 Single Channel acquisition (finite length mode stream)
/// Author: JYTEK
/// Date modified: 2020.8.28
/// Driver version: JYUSB1601 Installer_V2.0.0.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and ChannelID and select AITerminal
/// 2. Input sample rate and input voltage range limit
/// 3. Input preview points, save path and stream time
/// 4. Select the save path
/// 5. Press the Start button to start the finite length mode stream
/// </summary>

namespace Winform_AI_Finite_Streaming
{
    public partial class MainForm : Form
    {
        #region Private Fields

        /// <summary>
        /// aiTask
        /// </summary>
        private JYUSB1601AITask aiTask;

        /// <summary>
        /// the Buffer of data acquisition by the aiTask
        /// </summary>
        private double[,] RecordreadValue;
        private double[,] displayRecordreadValue;

        //Finite mode record time
        double recordedLength = 0;

        //Mark whether the stream is over
        bool recordDone = false;

        /// <summary>
        ///Current time
        /// </summary>
        private DateTime currentTime;

        /// <summary>
        /// Current time（String format）
        /// </summary>
        private string stringCurrentTime;

        /// input low limit
        /// </summary>
        private double lowRange;

        /// <summary>
        /// input high limit
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
            for (int i = 0; i < 16; i++)
            {
                checkedListBox_portChoose.Items.Add(string.Format("channel_{0}", i), false);
            }
            checkedListBox_portChoose.SetItemCheckState(0, CheckState.Checked);
            comboBox_slotNumber.SelectedIndex = 0;
            comboBox_inputRange.SelectedIndex = 0;
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
        /// start record Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //New aiTask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_slotNumber.SelectedIndex.ToString());

                //AddChannel
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    if (checkedListBox_portChoose.GetItemChecked(i))
                    {
                        aiTask.AddChannel(i, lowRange, highRange);
                    }
                }

                //Basic parameter configuration
                aiTask.Mode = AIMode.Record;
                aiTask.Record.Mode = RecordMode.Finite;
                aiTask.SampleRate = (double)numericUpDown_samplerate.Value;
                aiTask.SamplesToAcquire = (int)numericUpDown_PreviewSamplesPerChannels.Value;
                currentTime = new DateTime();
                currentTime = DateTime.Now;
                stringCurrentTime = currentTime.ToString("m") + "_" + Convert.ToString(currentTime.Hour)
                    + "_" + Convert.ToString(currentTime.Minute) + "_" + Convert.ToString(currentTime.Second);
                aiTask.Record.FilePath = textBox_path.Text + "\\" + stringCurrentTime + ".bin";
                aiTask.Record.Length = (double)numericUpDown_streamingtime.Value;
                //Start data acquisition
                aiTask.Start();
                toolStripStatusLabel1.Text = "start record Task";
            }

            catch (JYDriverException ex)
            {
                toolStripStatusLabel1.Text = "aiTask start failed";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
          
            progressBar_progress.Maximum = (int)(aiTask.Record.Length);
            RecordreadValue = new double[(int)numericUpDown_PreviewSamplesPerChannels.Value, aiTask.Channels.Count];
            displayRecordreadValue = new double[RecordreadValue.GetLength(1), RecordreadValue.GetLength(0)];

            timer_FetchData.Enabled = true;
            button_start.Enabled = false;
            button_stop.Enabled = true;
            groupBox_param.Enabled = false;

        }

        /// <summary>
        /// The timer checks the status of the streaming disk every 10ms. If the read data is completed, 
        /// it will be Stop Task, otherwise it will continue to read the buffer data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            aiTask.GetRecordStatus(out recordedLength, out recordDone);
            progressBar_progress.Value = (int)recordedLength;
            label_progress.Text = string.Format("{0}s / {1}s", (double)recordedLength, aiTask.Record.Length);
            if (recordDone == true)
            {
                try
                {
                    if(aiTask!=null)
                    {
                        aiTask.Stop();
                    }
                   
                }
                catch (JYDriverException ex)
                {
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }
                toolStripStatusLabel1.Text = "Streaming completed";
                //Clear the channel that was added last time
                aiTask.Channels.Clear();

                //Disable timer and Stop button, enable start button and parameter configuration button
                timer_FetchData.Enabled = false;
                groupBox_param.Enabled = true;
                button_start.Enabled = true;
                button_stop.Enabled = false;
            }
            else
            {
               if(aiTask.AvailableSamples >= (ulong)aiTask.SamplesToAcquire)
                {
                    aiTask.GetRecordPreviewData(ref RecordreadValue, aiTask.SamplesToAcquire, -1);
                    toolStripStatusLabel1.Text = "Reading preview data...";
                    ArrayManipulation.Transpose(RecordreadValue, ref displayRecordreadValue);
                    easyChartX_readrecordData.Plot(displayRecordreadValue);
                }
            }
        }

        /// <summary>
        /// select all channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_selectchannel_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_selectchannel.Checked)
            {
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    checkedListBox_portChoose.SetItemCheckState(i, CheckState.Checked);
                }
            }
            else
            {
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    checkedListBox_portChoose.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
        }

        /// <summary>
        /// Stop aiTask
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
            toolStripStatusLabel1.Text = "Stop Streaming Task";
            //Disable timer and Stop buttons, enable parameter configuration and start button
            timer_FetchData.Enabled = false;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            groupBox_param.Enabled = true;
        }

        /// <summary>
        /// save data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_savepath_Click_1(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            //Pop up a dialog box and manually select the save path
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_path.Text = folderBrowserDialog1.SelectedPath;
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


    }
}
