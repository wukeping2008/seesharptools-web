using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601  digital output (single mode)
/// Author: JYTEK
/// Date modified: 2020.8.28
/// Driver version: JYUSB1601  Installer_V2.0.0.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number
/// 2. Press the Start button to start a single digit output
/// 3. Modify the output level with the Siwtch switch
///
/// Note: JYUSB1601  board has 4 digital output ports
/// </summary>

namespace Winform_DO_SinglePoint
{
    public partial class MainForm : Form
    {
        #region Private Fields

        /// <summary>
        /// DOTask
        /// </summary>
        private JYUSB1601DOTask dotask;

        /// <summary>
        /// Store data to be written
        /// </summary>
        private bool[] writeValue;
        private int enableLineCount=0;
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
                checkedListBox_lineChoose.Items.Add(string.Format("DO_{0}", i), false);
            }
            checkedListBox_lineChoose.SetItemCheckState(0, CheckState.Checked);

            industrySwitch0.Enabled = false;
            industrySwitch1.Enabled = false;
            industrySwitch2.Enabled = false;
            industrySwitch3.Enabled = false;
            industrySwitch4.Enabled = false;
            industrySwitch5.Enabled = false;
            industrySwitch6.Enabled = false;
            industrySwitch7.Enabled = false;
            industrySwitch8.Enabled = false;
            industrySwitch9.Enabled = false;
            industrySwitch10.Enabled = false;
            industrySwitch11.Enabled = false;
            industrySwitch12.Enabled = false;
            industrySwitch13.Enabled = false;
            industrySwitch14.Enabled = false;
            industrySwitch15.Enabled = false;
            checkedListBox_lineChoose.Enabled = true;
        }

      
        /// <summary>
        /// Start DOTask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_start_Click(object sender, EventArgs e)
        {
            try
            {
                enableLineCount = 0;
                //new DOTask
                dotask = new JYUSB1601DOTask(comboBox_SoltNumber.SelectedIndex.ToString());

                //Add channels
                for (int i = 0; i < 16; i++)
                {
                    if (checkedListBox_lineChoose.GetItemChecked(i))
                    {
                        dotask.AddChannel(i);
                        enableLineCount++;
                        switch(i)
                        {
                            case 0:
                                industrySwitch0.Enabled = true;
                                break;
                            case 1:
                                industrySwitch1.Enabled = true;
                                break;
                            case 2:
                                industrySwitch2.Enabled = true;
                                break;
                            case 3:
                                industrySwitch3.Enabled = true;
                                break;
                            case 4:
                                industrySwitch4.Enabled = true;
                                break;
                            case 5:
                                industrySwitch5.Enabled = true;
                                break;
                            case 6:
                                industrySwitch6.Enabled = true;
                                break;
                            case 7:
                                industrySwitch7.Enabled = true;
                                break;
                            case 8:
                                industrySwitch8.Enabled = true;
                                break;
                            case 9:
                                industrySwitch9.Enabled = true;
                                break;
                            case 10:
                                industrySwitch10.Enabled = true;                              
                                break;
                            case 11:
                                industrySwitch11.Enabled = true;
                                break;
                            case 12:
                                industrySwitch12.Enabled = true;
                                break;
                            case 13:
                                industrySwitch13.Enabled = true;
                                break;
                            case 14:
                                industrySwitch14.Enabled = true;
                                break;
                            case 15:
                                industrySwitch15.Enabled = true;
                                break;
                        }
                    }
                }

                //Configure DOMode
                writeValue = new bool[enableLineCount];
                for(int i = 0; i < enableLineCount; i++)
                {
                    writeValue[i] = false;
                }
                dotask.WriteSinglePoint(writeValue);

                try
                {
                    //Start ATTask data acquisition
                    dotask.Start();
                }

                catch (Exception ex)
                {
                    //Drive error message display
                    MessageBox.Show(ex.Message);
                    return;
                }

                //Disable Parameter configuration button
                button_start.Enabled = false;
                button_stop.Enabled = true;
                checkedListBox_lineChoose.Enabled = false;
            }
            catch (Exception ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Write data to buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void industrySwitch0_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch0.Value, 0);
        }

        private void industrySwitch1_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch1.Value, 1);
        }

        private void industrySwitch2_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch2.Value, 2);
        }

        private void industrySwitch3_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch3.Value, 3);
        }

        private void industrySwitch5_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch5.Value, 5);
        }

        private void industrySwitch4_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch4.Value, 4);
        }

        private void industrySwitch6_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch6.Value, 6);
        }

        private void industrySwitch7_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch7.Value, 7);
        }

        private void industrySwitch8_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch8.Value, 8);
        }

        private void industrySwitch9_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch9.Value, 9);
        }

        private void industrySwitch10_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch10.Value, 10);
        }

        private void industrySwitch11_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch11.Value, 11);
        }

        private void industrySwitch12_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch12.Value, 12);
        }

        private void industrySwitch13_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch13.Value, 13);
        }

        private void industrySwitch14_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch14.Value, 14);
        }

        private void industrySwitch15_ValueChanged(object sender, EventArgs e)
        {
            dotask.WriteSinglePoint(industrySwitch15.Value, 15);
        }

        /// <summary>
        /// Stop output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                industrySwitch0.Value = false;
                industrySwitch1.Value = false;
                industrySwitch2.Value = false;
                industrySwitch3.Value = false;
                industrySwitch4.Value = false;
                industrySwitch5.Value = false;
                industrySwitch6.Value = false;
                industrySwitch7.Value = false;
                industrySwitch8.Value = false;
                industrySwitch9.Value = false;
                industrySwitch10.Value = false;
                industrySwitch11.Value = false;
                industrySwitch12.Value = false;
                industrySwitch13.Value = false;
                industrySwitch14.Value = false;
                industrySwitch15.Value = false;

                industrySwitch0.Enabled = false;
                industrySwitch1.Enabled = false;
                industrySwitch2.Enabled = false;
                industrySwitch3.Enabled = false;
                industrySwitch4.Enabled = false;
                industrySwitch5.Enabled = false;
                industrySwitch6.Enabled = false;
                industrySwitch7.Enabled = false;
                industrySwitch8.Enabled = false;
                industrySwitch9.Enabled = false;
                industrySwitch10.Enabled = false;
                industrySwitch11.Enabled = false;
                industrySwitch12.Enabled = false;
                industrySwitch13.Enabled = false;
                industrySwitch14.Enabled = false;
                industrySwitch15.Enabled = false;
                //Stop ATTask data acquisition
                dotask.Stop();
            }

            catch (Exception ex)
            {
                //Drive error message display
                MessageBox.Show(ex.Message);
                return;
            }

            //enable Start button and Parameter configuration button

            button_start.Enabled = true;
            button_stop.Enabled = false;
            checkedListBox_lineChoose.Enabled = true;

        }




        #endregion

        #region Methods
        #endregion

    }
}
