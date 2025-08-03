using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// PCIe-5112  Multichannel Single Output
/// Author：JYTEK 
/// Modified data: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later 
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description: 
///         1. Enter the board number and channel number
///         2. Press the Start button to start Single waveform output
/// </summary>
namespace Winform_AO_Single_Point_MultiChannel
{
    public partial class MainForm : Form
    {
        #region Private Fields

        /// <summary>
        ///  AO task
        /// </summary>
        private JYUSB1601AOTask aoTask;

        /// <summary>
        /// data buffer
        /// </summary>
        private double[] writeValue;

        private int channelCount;
        ChannelParam channelParam = new ChannelParam();
        public BindingList<ChannelParam> channelsList;

        private List<int> CheckedChannels;

        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// Initialize controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox_BoardNumber.SelectedIndex = 0;
            channelsList = new BindingList<ChannelParam>();
            channelCount = 2;


            try
            {
                for (int i = 0; i < channelCount; i++)
                {
                    ChannelParam channelParam = new ChannelParam();
                    channelParam.ChannelSelected = false;
                    channelParam.ChannelNumber = i;
                    channelParam.WriteValue = 5;
                    channelParam.WaveType = "DC";
                    channelsList.Add(channelParam);
                }
                dataGridView_waveConfigure.DataSource = channelsList;
                dataGridView_waveConfigure.Update();
                dataGridView_waveConfigure.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            checkBox_selectAll.Checked = false;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_update.Enabled = false;
        }

        /// <summary>
        /// AO WriteWave Class
        /// </summary>
        public class ChannelParam
        {
            public bool ChannelSelected { get; set; }
            public int ChannelNumber { get; set; }
            public double WriteValue { get; set; }
            public string WaveType { get; set; }
        }

        /// <summary>
        /// Start task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            //addchannel
            CheckedChannels = new List<int>();
            for (int i = 0; i < channelCount; i++)
            {
                if ((bool)dataGridView_waveConfigure.Rows[i].Cells[0].Value)
                {
                    CheckedChannels.Add(i);
                }
            }

            // Create task
            aoTask = new JYUSB1601AOTask(comboBox_BoardNumber.SelectedIndex.ToString());

            //AddChannel
            for (int i = 0; i < CheckedChannels.Count; i++)
            {
                aoTask.AddChannel(CheckedChannels[i]);
            }
            //Set single point value
            double[] aoSingleValue = new double[CheckedChannels.Count];
            for (int i = 0; i < CheckedChannels.Count; i++)
            {
                aoSingleValue[i] = (double)dataGridView_waveConfigure.Rows[i].Cells[2].Value;
            }
            
            //Basic parameter configuration
            aoTask.Mode = AOMode.Single;
            aoTask.WriteSinglePoint(aoSingleValue);
            writeValue = new double[CheckedChannels.Count];

            //Waveform generation
            waveGeneration();

            //aoTask.CompleteState = OutputCompleteState.Zero;
            
            try
            {
                //Start Task
                aoTask.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                //WriteSinglePoint
                aoTask.WriteSinglePoint(writeValue);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            groupBox_parameter.Enabled = true;
            button_start.Enabled = false;
            button_stop.Enabled = true;
            button_update.Enabled = true;
        }

        /// <summary>
        /// update writeValue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_update_Click(object sender, EventArgs e)
        {
            waveGeneration();
            aoTask.WriteSinglePoint(writeValue);
            groupBox_parameter.Enabled = true;
            button_start.Enabled = false;
            button_stop.Enabled = true;
            button_update.Enabled = true;
        }

        /// <summary>
        /// stop task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                aoTask.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //Clear the channel that was added last time
            aoTask.Channels.Clear();
            checkBox_selectAll.Checked = false;
            for (int i = 0; i < channelCount; i++)
            {
                if ((bool)dataGridView_waveConfigure.Rows[i].Cells[0].Value)
                {
                    dataGridView_waveConfigure.Rows[i].Cells[0].Value = false;
                }
            }

            groupBox_parameter.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_update.Enabled = false;
        }

        /// <summary>
        /// selectAllchannel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_selectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_selectAll.Checked)
            {
                foreach (var info in channelsList)
                    info.ChannelSelected = true;
            }
            else
            {
                foreach (var info in channelsList)
                    info.ChannelSelected = false;
            }

            dataGridView_waveConfigure.DataSource = channelsList;
            dataGridView_waveConfigure.Update();
            dataGridView_waveConfigure.Refresh();
        }

        /// <summary>
        /// Close pannel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (aoTask != null)//Determine if the object is null
            {
                aoTask.Stop();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Waveform generation
        /// </summary>
        private void waveGeneration()
        {
            for (int i = 0; i < CheckedChannels.Count; i++)
            {
                writeValue[i] = (double)dataGridView_waveConfigure.Rows[CheckedChannels[i]].Cells[2].Value;
            }
            //for (int i = 0; i < 2; i++)
            //{
            //    writeValue[i] = 5;
            //}
        }


        #endregion


    }
}