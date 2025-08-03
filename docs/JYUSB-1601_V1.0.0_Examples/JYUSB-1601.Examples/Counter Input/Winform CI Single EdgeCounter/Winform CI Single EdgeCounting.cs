using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single mode simple EdgeCounter
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. According to the time of writing to the register, there are 2 modes for edge counting:
/// The first type is not enabled. SampleClk: When the rising edge of the measurement signal comes, the count value is written to the register. The second type of implicit clock is enabled: when the rising edge of the measurement signal comes, the count value is written to the register.
/// 2. Counting direction When the external pin is selected to determine the counting direction, the input pin is CTR_AUX.
/// Simple counting function:
/// In this mode, the timer will record the number of pulses above the CTR_Source after the software start.
/// </summary>

namespace Winform_CI_Single_EdgeCounter
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
        uint counterValue = 0;
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// Set the default index of the parameter settings control
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

            //Call the enumeration of CountDirection in the driver as a menu
            comboBox_countDIR.Items.AddRange(Enum.GetNames(typeof(CountDirection)));
            comboBox_countDIR.SelectedIndex = 0;

            //Call the enumeration of LevelPolarity in the driver as a menu
            comboBox_pauseActivePolarity.Items.AddRange(Enum.GetNames(typeof(LevelPolarity)));
            comboBox_pauseActivePolarity.SelectedIndex = 2;

        }
        
        /// <summary>
        ///  Start Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Start_Click(object sender, EventArgs e)
        {
            try
            {
                //new ciTask based on the selected Solt Number and counterID
                ciTask = new JYUSB1601CITask(comboBox_SoltNumber.SelectedIndex.ToString(), comboBox_counterNumber.SelectedIndex);

                //Parameter configuration
                ciTask.Type = CIType.EdgeCounting;
                ciTask.EdgeCounting.Direction = (CountDirection)Enum.Parse(typeof(CountDirection), comboBox_countDIR.Text, true);
                ciTask.EdgeCounting.InitialCount = (int)numericUpDown_initCount.Value;
                ciTask.EdgeCounting.OutEvent.Threshold = (uint)numericUpDown_OutEventThreshold.Value;
                ciTask.EdgeCounting.Pause.ActivePolarity = (LevelPolarity)Enum.Parse(typeof(LevelPolarity), comboBox_pauseActivePolarity.Text, true); 

                try
                {
                    //start ciTask
                    ciTask.Start();
                }

                catch (Exception ex)
                {
                    //Drive error message display
                   MessageBox.Show(ex.Message);
                   return;
                }
               
                //Disable parameter configuration and start button
                timer_FetchData.Enabled = true;
                groupBox_genPara.Enabled = false;
                button_Start.Enabled = false;
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
        /// Counter，Read the count value every 100ms and display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                ciTask.ReadSinglePoint(ref counterValue);
                textBox_countValue.Text = counterValue.ToString();
            }
            catch (Exception ex)
            {
                //Display driver error message
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
       
        /// <summary>
        /// Stop ciTask
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

            //enable Parameter configuration and start button，disable Stop button
            timer_FetchData.Enabled = false;
            groupBox_genPara.Enabled = true;
            button_Start.Enabled = true;
            button_stop.Enabled = false;
        }
        #endregion

        #region Methods  
        #endregion

    }
}
