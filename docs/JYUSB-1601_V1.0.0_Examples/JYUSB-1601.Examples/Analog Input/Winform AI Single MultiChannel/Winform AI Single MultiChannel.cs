using System;
using System.Windows.Forms;
using JYUSB1601;
using SeeSharpTools.JY.ArrayUtility;

/// <summary>
/// JYUSB1601 multi-channel continuous acquisition
/// Author：JYTEK
/// Modified date：2023.6.6
/// Driver version： V0.0.3
/// Installation package:  SeeSharpTools.JY.GUI 1.4.7
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the board number and the number of channels
/// 2. Input sample rate and input voltage range limit
/// 3. Press the start button start for continuous data collection
/// </summary>

namespace SeeSharpExample.JY.JY5310
{
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        ///  AITask
        /// </summary>
        private JYUSB1601AITask aiTask;

        /// <summary>
        /// the Buffer of data acquisition by the AITask
        /// </summary>
        double readValue;
        /// input low limit
        /// </summary>
        private double lowRange;
        /// <summary>
        /// input high limit
        /// </summary>
        /// <returns></returns>
        private double highRange;

        int channel;
        private double[] AIRange = new double[] { 10, 5, 2.5 };
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
            comboBox_boardNumber.SelectedIndex = 0;
            checkedListBox_portChoose.Items.Clear();
            for (int i = 0; i < AIRange.Length; i++)
            {
                comboBox_inputRange.Items.Add(string.Format("±{0}V", AIRange[i]));
            }
            comboBox_inputRange.SelectedIndex = 0;
            showchannel();
            groupBox_genParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
        }

        /// <summary>
        /// select all channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_selectchannel_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox_selectchannel.Checked)
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
        /// Start data acquisition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //New AITask based on the selected Solt Number
                aiTask = new JYUSB1601AITask(comboBox_boardNumber.SelectedIndex.ToString());

                //AddChannel
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    if (checkedListBox_portChoose.GetItemChecked(i))
                    {
                        aiTask.AddChannel(i, lowRange, highRange);
                    }
                }
                //Basic parameter configuration
                aiTask.Mode = AIMode.Single;

                try
                {
                    //Start data acquisition
                    aiTask.Start();
                }

                catch (Exception ex)
                {
                    toolStripStatusLabel.Text = "AITask start failed";
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }

               
                //Enable timer, disable parameter configuration button and start button, display status
                timer_FetchData.Enabled = true;
                groupBox_channel.Enabled = false;
                groupBox_param.Enabled = false;
                //groupBox_genParam.Enabled = false;
                button_start.Enabled = false;
                button_stop.Enabled = true;
                toolStripStatusLabel.Text = "Start data acquisition";
            }
            catch(Exception ex)
            {
                toolStripStatusLabel.Text = "AITask start failed";
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

        }

        /// <summary>
        /// Stop AITask data acquisition
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
                    //stop
                    aiTask.Stop();
                    //Clear the channel that was added last time
                    aiTask.Channels.Clear();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
            
            //Disable timer, enable parameter configuration button and start button, display status
            timer_FetchData.Enabled = false;
            groupBox_channel.Enabled = true;
            groupBox_param.Enabled = true;
            //groupBox_genParam.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            toolStripStatusLabel.Text = "Stop AITask data acquisition";
        }

        /// <summary>
        /// Timer, read and display data every 10ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;
            dataGridView_waveConfigure.Rows.Clear();
            channel = 0;
            try
            {
                for (int i = 0; i < checkedListBox_portChoose.Items.Count; i++)
                {
                    if (checkedListBox_portChoose.GetItemChecked(i))
                    {
                        dataGridView_waveConfigure.Rows.Add();
                        aiTask.ReadSinglePoint(ref readValue, i);
                        toolStripStatusLabel.Text = "Reading data...";
                        ////Display data
                        dataGridView_waveConfigure.Rows[channel].Cells[0].Value = i;
                        dataGridView_waveConfigure.Rows[channel].Cells[1].Value = readValue;
                        channel++;
                    }
                }
            }
            catch (Exception ex)
            {
                timer_FetchData.Enabled = false;
                toolStripStatusLabel.Text = "Failed to read data";
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }

            //Enable timer, continue to check whether buffer data is enough
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
                if (aiTask != null) 
                {
                    //stop
                    aiTask.Stop();
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
        public void showchannel()
        {
            checkedListBox_portChoose.Items.Clear();
           
                    checkedListBox_portChoose.Items.Add("channel_0", false);
                    checkedListBox_portChoose.Items.Add("channel_1", false);
                    checkedListBox_portChoose.Items.Add("channel_2", false);
                    checkedListBox_portChoose.Items.Add("channel_3", false);
                    checkedListBox_portChoose.Items.Add("channel_4", false);
                    checkedListBox_portChoose.Items.Add("channel_5", false);
                    checkedListBox_portChoose.Items.Add("channel_6", false);
                    checkedListBox_portChoose.Items.Add("channel_7", false);
                    checkedListBox_portChoose.Items.Add("channel_8", false);
                    checkedListBox_portChoose.Items.Add("channel_9", false);
                    checkedListBox_portChoose.Items.Add("channel_10", false);
                    checkedListBox_portChoose.Items.Add("channel_11", false);
                    checkedListBox_portChoose.Items.Add("channel_12", false);
                    checkedListBox_portChoose.Items.Add("channel_13", false);
                    checkedListBox_portChoose.Items.Add("channel_14", false);
                    checkedListBox_portChoose.Items.Add("channel_15", false);
            

        }
        #endregion
    }
}
