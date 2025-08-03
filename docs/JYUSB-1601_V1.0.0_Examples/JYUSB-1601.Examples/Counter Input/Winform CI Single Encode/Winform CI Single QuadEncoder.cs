using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single mode Encode
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.4 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Orthogonal coding function, orthogonal coding includes three coding methods: X1, X2, X4
/// 2.X1: When A leads B, the count increase occurs on the rising edge of A; when B leads A, the count decrease occurs on the falling edge of A.
/// X2: When A leads B, the count increase occurs on the rising edge and falling edge of A; when B leads A, the count reduction occurs on the rising edge and falling edge of A.
/// X4: When A leads B, the increment increases on the rising and falling edges of A and B. When B leads A, the count reduction occurs on the rising and falling edges of A and B.
/// single mode Encode:
/// In this mode, input pins A and B refer to the Source port and AUX port of the CounterID, respectively, and the Gate port is grounded.
/// </summary>
///

namespace Winform_CI_Single_Encode
{
    public partial class MainForm : Form
    {
        #region private Fields
        /// <summary>
        /// CITask
        /// </summary>
        private JYUSB1601CITask citask;

        /// <summary>
        /// the Buffer of data read by the CITask
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

            comboBox_encodeType.Items.AddRange(Enum.GetNames(typeof(QuadEncodingType)));
            comboBox_encodeType.SelectedIndex = 0;
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
                //new CITask based on the selected Solt Number and counterID
                citask = new JYUSB1601CITask(comboBox_SoltNumber.SelectedIndex.ToString(), comboBox_counterNumber.SelectedIndex);

                //Parameter configuration
               
                citask.Type = CIType.QuadEncoder;
                citask.QuadEncoder.EncodingType = (QuadEncodingType)Enum.Parse(typeof(QuadEncodingType), comboBox_encodeType.Text, true);
                citask.QuadEncoder.ZReloadEnabled = false;
                citask.QuadEncoder.InitialCount = (int)numericUpDown_initCount.Value;
                try
                {
                    //start Task
                    citask.Start();
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
        /// Stop Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (citask != null)
                {
                    citask.Stop();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //disable timer and stop button ,enable Parameter configuration button and start button
            timer_FetchData.Enabled = false;
            groupBox_genPara.Enabled = true;
            button_Start.Enabled = true;
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
                if (citask != null)
                {
                    citask.Stop();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// timer，refresh every 100ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer_FetchData.Enabled = false;

            try
            {
                //read countervalue and display
                citask.ReadSinglePoint(ref counterValue);
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

        #endregion

        #region Methods  
        #endregion
    }
}
