using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single mode TwoEdgeSeparation 
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. There are 2 modes depending on the time of writing to the register:
/// The first type is not enabled. SampleClk: When the rising edge of the second signal comes,
/// the number of Timebase pulses between the first signal and the rising edge of the second signal is written to the register.
/// Measure two edge differences that are not on the same channel. The second is to enable implicit clocking: 
/// when a measurement is completed, the count value is written to the register.
/// 2. Select which two rising edges are to be measured. The first signal comes from the Gate side and the second signal comes from the Aux side.
///
/// single mode TwoEdgeSeparation:
/// In this mode, the difference between the two channel edges of CTR0_GATE and CTR0_AUX is mainly recorded in this mode.
/// </summary>

namespace Winform_CI_Single_TwoEdgeSeparationMeasure
{
    public partial class MainForm : Form
    {
        #region private Fields
        /// <summary>
        /// ciTask
        /// </summary>
        private JYUSB1601CITask ciTask;

        /// <summary>
        /// the Buffer of data read by the ciTask
        /// </summary>
        private double SecondToFirst;
        private double FirstToSecond;
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
            for (int i = 0; i < 2; i++)
            {
                comboBox_counterNumber.Items.Add(i);
            }
            comboBox_counterNumber.SelectedIndex = 0;          
        }

        /// <summary>
        /// Start ciTask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                //New Task
                ciTask = new JYUSB1601CITask(comboBox_SoltNumber.SelectedIndex.ToString(), comboBox_counterNumber.SelectedIndex);

                //Basic parameter configuration
                ciTask.Type = CIType.TwoEdgeSeparation;
                try
                {
                    //start Task
                    ciTask.Start();
                }

                catch (Exception ex)
                {
                    //Drive error message display
                   MessageBox.Show(ex.Message);
                   return;
                }

                //enable timer and Stop button，Disable parameter configuration and start button
                timer_FetchData.Enabled = true;
                groupBox_genPara.Enabled = false;
                button_start.Enabled = false;
                button_stop.Enabled = true;
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
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
                if (ciTask != null)
                {
                    ciTask.Stop();
                }
            }

            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
               return;
            }

            //Disable timer and Stop buttons, enable parameter configuration and start button
            timer_FetchData.Enabled = false;
            groupBox_genPara.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
        }

        /// <summary>
        /// timer，fresh every 100ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void time_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                //read measurevalue and display
                ciTask.ReadSinglePoint(ref FirstToSecond, ref SecondToFirst);
                textBox_FirstToSecond.Text = FirstToSecond.ToString();
                textBox_SecondToFirst.Text = SecondToFirst.ToString();

            }
            catch(Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }

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
                if (ciTask != null)
                {
                    ciTask.Stop();
                }
            }

            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
               return;
            }
        }
        #endregion

        #region Methods
        #endregion

    }
}
