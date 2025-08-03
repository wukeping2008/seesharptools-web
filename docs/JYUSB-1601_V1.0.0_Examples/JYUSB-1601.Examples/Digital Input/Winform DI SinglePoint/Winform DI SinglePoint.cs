using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single point mode digital input
/// Author: JYTEK
/// Date modified: 2020.8.28
/// Driver version: JYUSB1601 Installer_V2.0.0.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.4 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number
/// 2. Press the Start button to start single point digital signal acquisition
///
/// </summary>

namespace Winform_DI_SinglePoint
{
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        /// DITask
        /// </summary>
        private JYUSB1601DITask ditask;

        /// <summary>
        /// DI State
        /// </summary>
        private bool diState;

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
            checkedListBox_lineChoose.Items.Clear();
            for (int i = 0; i < 16; i++)
            {
                checkedListBox_lineChoose.Items.Add(string.Format("DI_{0}", i), false);
            }
            checkedListBox_lineChoose.SetItemCheckState(0, CheckState.Checked);
            checkedListBox_lineChoose.Enabled = true;
        }

        /// <summary>
        /// Start digital Signal input
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <summary>
        private void button_start_Click(object sender, EventArgs e)
         {
            try
            {
                //new DITask based on the selected Solt Number
                ditask = new JYUSB1601DITask(comboBox_SoltNumber.SelectedIndex.ToString());
                //Add channels
                for (int i = 0; i < 16; i++)
                {
                    if (checkedListBox_lineChoose.GetItemChecked(i))
                    {
                        ditask.AddChannel(i);
                    }
                }
                try
                {
                    //Start ATTask data acquisition
                    ditask.Start();
                }

                catch (Exception ex)
                {
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }
                timer1.Enabled = true;
                checkedListBox_lineChoose.Enabled = false;
                groupBox_anaInParam.Enabled = false;
                button_start.Enabled = false;
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
        }

        /// <summary>
        /// check di state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //禁用timer
            timer1.Enabled = false;
            // check di 
            for (int i = 0; i < 16; i++)
            {
                if (checkedListBox_lineChoose.GetItemChecked(i))
                {
                    ditask.ReadSinglePoint(ref diState,i);
                    switch(i)
                    {
                        case 0:
                            led0.Value = diState;
                            break;
                        case 1:
                            led1.Value = diState;
                            break;
                        case 2:
                            led2.Value = diState;
                            break;
                        case 3:
                            led3.Value = diState;
                            break;
                        case 4:
                            led4.Value = diState;
                            break;
                        case 5:
                            led5.Value = diState;
                            break;
                        case 6:
                            led6.Value = diState;
                            break;
                        case 7:
                            led7.Value = diState;
                            break;
                        case 8:
                            led8.Value = diState;
                            break;
                        case 9:
                            led9.Value = diState;
                            break;
                        case 10:
                            led10.Value = diState;
                            break;
                        case 11:
                            led11.Value = diState;
                            break;
                        case 12:
                            led12.Value = diState;
                            break;
                        case 13:
                            led13.Value = diState;
                            break;
                        case 14:
                            led14.Value = diState;
                            break;
                        case 15:
                            led15.Value = diState;
                            break;

                    }
                }
            }
            timer1.Enabled = true;
        }

        /// <summary>
        /// stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (ditask != null)
                {
                    ditask.Stop();
                }
                checkedListBox_lineChoose.Enabled = true;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            //启用参数设定和开始按钮，禁用timer功能
            timer1.Enabled = false;
            groupBox_anaInParam.Enabled = true;
            button_start.Enabled = true;
        }

        #endregion

        #region Methods
        #endregion

    }
}


        
