using System;
using System.Windows.Forms;
using JYUSB1601;

/// <summary>
/// JYUSB1601 single pulse output
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// Description:
/// 1. Input the Solt Number and CounterID number
/// 2. Configure the pulse output type and its parameters
/// 3. Press the Start button to start a single pulse generation
///
/// </summary>

namespace Winform_CO_Single
{
    public partial class MainForm : Form
    {
        #region private Fields
        /// <summary>
        /// coTask
        /// </summary>
        private JYUSB1601COTask coTask;

        private bool allowUpdate;
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
        private void MainForm_Load(object sender, EventArgs e)
        {
            allowUpdate = false;

            timer_FetchData.Enabled = false;

            comboBox_SlotNumber.SelectedIndex = 0;
            for (int i = 0; i < 2; i++)
            {
                comboBox_counterNumber.Items.Add(i);
            }
            comboBox_counterNumber.SelectedIndex = 0;

            //Call the enumeration of IdleState in the driver as a menu
            comboBox_IdleState.Items.AddRange(Enum.GetNames(typeof(COIdleState)));
            comboBox_IdleState.SelectedIndex = 0;

            //Call the enumeration of COPulseType in the driver as a menu
            comboBox_pulseType.Items.AddRange(Enum.GetNames(typeof(COPulseType)));
            comboBox_pulseType.SelectedIndex = 0;
        }


        /// <summary>
        /// Start Task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (!allowUpdate)
                {
                    //new coTask based on the selected Solt Number and counterID
                    coTask = new JYUSB1601COTask(comboBox_SlotNumber.SelectedIndex.ToString(), comboBox_counterNumber.SelectedIndex);

                    //Basic parameter configuration
                  
                    coTask.IdleState = (COIdleState)Enum.Parse(typeof(COIdleState), comboBox_IdleState.Text, true);
                    coTask.WriteSinglePoint(new COPulse((COPulseType)Enum.Parse(typeof(COPulseType), comboBox_pulseType.Text), (double)numericUpDown_highPulseWidth.Value, (double)numericUpDown_lowPulseWidth.Value, (int)numericUpDown_PulseCount.Value));

                    try
                    {
                        //Start coTask
                        coTask.Start();
                    }

                    catch (Exception ex)
                    {
                        //Drive error message display
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    if (numericUpDown_PulseCount.Value == -1) //the number of pulses that is being sent is infinite
                    {
                        button_Start.Enabled = true;
                        button_Start.Text = "Update";
                        groupBox_PulsePara.Enabled = true;
                        allowUpdate = true;
                    }
                    else
                    {
                        button_Start.Enabled = false;
                        allowUpdate = false;
                        groupBox_PulsePara.Enabled = false;
                    }
                }
                else
                {
                    coTask.WriteSinglePoint(new COPulse((COPulseType)Enum.Parse(typeof(COPulseType), comboBox_pulseType.Text), (double)numericUpDown_highPulseWidth.Value, (double)numericUpDown_lowPulseWidth.Value, (int)numericUpDown_PulseCount.Value));
                }
                //enable Parameter configuration and start button，disable Stop button
                timer_FetchData.Enabled = true;
                groupBox_paraConfig.Enabled = false;
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
        /// close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //Determine if the task exists
                if (coTask != null) 
                {
                    //Stop Task 
                    coTask.Stop();
                }
            }
            catch (Exception ex)
            {
                //Drive error message display
               MessageBox.Show(ex.Message);
               return;
            }
        }

        /// <summary>
        /// timer，check coTask if is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_FetchData_Tick(object sender, EventArgs e)
        {
            if (numericUpDown_PulseCount.Value != -1 && coTask.WaitUntilDone(10) == true)
            {
                try
                {
                    if (coTask != null)
                    {
                        //Stop Task 
                        coTask.Stop();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                
                button_Start.Text = "Start";
                timer_FetchData.Enabled = false;
                groupBox_paraConfig.Enabled = true;
                groupBox_PulsePara.Enabled = true;
                button_Start.Enabled = true;
                button_stop.Enabled = false;
            }
        }

        /// <summary>
        /// Set pulse parameters according to outputPulse Type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_pulseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Pulse Type is configured according to the high and low Time numbers
            if (comboBox_pulseType.SelectedIndex == 1)
            {
                label_HighLevel.Text = "High Level Time(s)";
                numericUpDown_highPulseWidth.Increment = 0.0001M;
                numericUpDown_highPulseWidth.DecimalPlaces = 4;
                numericUpDown_highPulseWidth.Value = 0.0005M;

                label_LowLevel.Text = "Low Level Time(s)";
                numericUpDown_lowPulseWidth.Increment = 0.0001M;
                numericUpDown_lowPulseWidth.DecimalPlaces = 4;
                numericUpDown_lowPulseWidth.Value = 0.0005M;
            }
            //Pulse Type is configured according to the high and low Tick numbers
            else if (comboBox_pulseType.SelectedIndex == 2)
            {
                label_HighLevel.Text = "High Level Tick";
                numericUpDown_highPulseWidth.Increment = 1;
                numericUpDown_highPulseWidth.DecimalPlaces = 0;
                numericUpDown_highPulseWidth.Value = 100000;

                label_LowLevel.Text = "Low Level Tick";
                numericUpDown_lowPulseWidth.Increment = 1;
                numericUpDown_lowPulseWidth.DecimalPlaces = 0;
                numericUpDown_lowPulseWidth.Value = 100000;
            }

            //Pulse Type is configured according to the dutycycle and frequency
            else
            {
                label_HighLevel.Text = "Frequency";
                numericUpDown_highPulseWidth.Value = 1000;

                label_LowLevel.Text = "Duty Cycle";
                numericUpDown_lowPulseWidth.Value = 0.5m;
            }
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
                if (coTask != null)
                {
                    //Stop Task 
                    coTask.Stop();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            
            button_Start.Text = "Start";
            allowUpdate = false;
            timer_FetchData.Enabled = false;
            groupBox_paraConfig.Enabled = true;
            groupBox_PulsePara.Enabled = true;
            button_Start.Enabled = true;
            button_stop.Enabled = false;
        }

        #endregion

        #region Methods
        #endregion


    }
 }
