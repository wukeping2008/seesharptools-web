using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.ArrayUtility;

/// <summary>
/// JYUSB1601 Single Channel acquisition (infinite length mode stream)
/// Author：JYTEK
/// Modified date：2023.6.6
/// Driver version： V0.0.3
/// Installation package:  SeeSharpTools.JY.GUI 1.4.7
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and ChannelID and select AITerminal
/// 2. Input sample rate and input voltage range limit
/// 3. Input the preview point and save the path
/// 4. Select the save path
/// 5. Press the Start button to start the infinite length mode stream
/// </summary>
/// 
namespace SeeSharpExample.JY.JYUSB1601
{
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        /// AI Task
        /// </summary>
        private JYUSB1601AITask aiTask;

        /// <summary>
        /// the Buffer of data acquisition by the aiTask
        /// </summary>
        private double[,] RecordreadValue;
        private double[,] displayRecordreadValue;

        /// <summary>
        /// currentTime
        /// </summary>
        private DateTime currentTime;

        /// <summary>
        /// currentTime（String format）
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
        private int channelCount=16;
        private double[] JYRange = new double[] { 10, 5, 2.5};


      
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// Set the default index of combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox_boardID.SelectedIndex = 0;

            for (int j = 0; j < JYRange.Length; j++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", JYRange[j]));
            }
        

            for (int i = 0; i<channelCount; i++)
            {
                checkedListBox_portChoose.Items.Add(string.Format("channel_{0}", i), false);
            }
            checkedListBox_portChoose.SetItemCheckState(0, CheckState.Checked);
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
        /// start record Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            //New aiTask
            aiTask = new JYUSB1601AITask(comboBox_boardID.SelectedIndex.ToString());

            //AddChannel
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
            aiTask.Record.Mode = RecordMode.Infinite;
            aiTask.SampleRate = (double)numericUpDown_samplerate.Value;
            currentTime = new DateTime();
            currentTime = DateTime.Now;
            stringCurrentTime = currentTime.ToString("m") + "_" + Convert.ToString(currentTime.Hour)
                + "_" + Convert.ToString(currentTime.Minute) + "_" + Convert.ToString(currentTime.Second);
            aiTask.Record.FilePath = textBox_path.Text + "\\" + stringCurrentTime + ".bin";

            try
            {
                //Start data acquisition
                aiTask.Start();
            }

            catch (JYDriverException ex)
            {
                toolStripStatusLabel1.Text = "Task start failed";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            RecordreadValue = new double[(int)numericUpDown_PreviewSamplesPerChannels.Value, aiTask.Channels.Count];
            displayRecordreadValue = new double[RecordreadValue.GetLength(1), RecordreadValue.GetLength(0)];

            timer_FetchData.Enabled = true;
            button_start.Enabled = false;
            button_stop.Enabled = true;
            groupBox_param.Enabled = false;
            toolStripStatusLabel1.Text = "start record Task";
        }

        /// <summary>
        /// Timer, read buffer preview data every 10ms and display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;
            try
            {
                if(aiTask.AvailableSamples >= (ulong)numericUpDown_PreviewSamplesPerChannels.Value)
                {
                    aiTask.GetRecordPreviewData(ref RecordreadValue, (int)numericUpDown_PreviewSamplesPerChannels.Value, 1000);
                    toolStripStatusLabel1.Text = "Reading preview data...";
                    ArrayManipulation.Transpose(RecordreadValue, ref displayRecordreadValue);
                    easyChartX_readrecordData.Plot(displayRecordreadValue);
                }
    
            }
            catch (JYDriverException ex)
            {
                toolStripStatusLabel1.Text = "Failed to read preview data";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
            timer_FetchData.Enabled = true;
        }

        /// <summary>
        /// Stop Task
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
                    toolStripStatusLabel1.Text = "Stop Task";
                    //Stop Task
                    aiTask.Stop();
                }
            }
            catch (JYDriverException ex)
            {
                toolStripStatusLabel1.Text = "Failed to stop Task";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //Disable timer and stop button, enable start button and parameter configuration button disable timer and stop task button, enable parameter configuration and start button
            timer_FetchData.Enabled = false;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            groupBox_param.Enabled = true;
        }

        /// <summary>
        /// FormClosing
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

        /// <summary>
        /// save data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_savepath_Click_1(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            //Pop up the dialog box and select the saving path manually
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_path.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        #endregion

        #region  Methods
        #endregion
    }
}
