using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single mode single cycle pulse width measurement
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. According to the time of writing to the register, there are 2 modes: not enabled. SampleClk: When the rising edge of the measurement signal comes, 
/// the high and low Tick meters are written into the register
/// Enable implicit clock: When a measurement is completed, the count value is written to the register
/// 2. When the pulse is measured, the pulse to be measured is selected as a high level pulse or a low level pulse
///
/// In this mode, CTR0_GATE is connected to the pulse signal to be measured, and the pulse width time of the pulse on CTR0_GATE is measured
/// </summary>

namespace Winform_CI_Single_FrequencyMeasure
{
    public partial class MainForm : Form
    {
        #region private Fields
        /// <summary>
        /// ciTask
        /// </summary>
        private JYUSB1601CITask ciTask;

        /// <summary>
        /// 读取测量值的缓存
        /// </summary>
        private double PeroidMeas;
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handler
        /// <summary>
        ///Set the default index of comboBox
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
                //new ciTask based on the selected Solt Number and counterID
                ciTask = new JYUSB1601CITask(comboBox_SoltNumber.SelectedIndex.ToString(), comboBox_counterNumber.SelectedIndex);

                //Basic parameter configuration
                ciTask.Type = CIType.Period;

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

                //enable timer and stop button，Disable parameter configuration and start button
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
        /// CounterID，每隔100ms读取一次测量值并显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void time_FetchData_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                //读取测量值并显示
                ciTask.ReadSinglePoint(ref PeroidMeas);
                textBox_PeroidMeas.Text = PeroidMeas.ToString("F5");
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
               return;
            }

            timer_FetchData.Enabled = true;
        }

        /// <summary>
        /// Stop单脉宽测量Task
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

            //禁用定时器和Stop按钮，启用Parameter configuration数据和start按钮
            timer_FetchData.Enabled = false;
            groupBox_genPara.Enabled = true;
            button_start.Enabled = true;
            button_stop.Enabled = false;
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
