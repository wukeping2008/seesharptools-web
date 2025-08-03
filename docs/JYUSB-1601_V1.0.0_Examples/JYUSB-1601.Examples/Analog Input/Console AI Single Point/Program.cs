using System;
using JYUSB1601;
using System.Windows.Forms;


///<summary>
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// </summary>
namespace Console_AI_Single_Point
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("USB-1601 Single Channel Analog Input in Single Mode");

            Console.WriteLine("Please Input BoardNumber：");
            string _boardNum = Console.ReadLine();

            double readValue = 0;
            int  _flag = 1;

            //New aiTask based on the selected Solt Number        
            JYUSB1601AITask aiTask = null;
            try
            {
                aiTask = new JYUSB1601AITask(_boardNum);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Console.WriteLine("Please Input ChannelID：");
            int _channelID = Convert.ToInt32(Console.ReadLine());

            //Basic parameter configuration
            aiTask.Mode = AIMode.Single;

            //AddChannel
            aiTask.AddChannel(_channelID, -10, 10);
            try
            {
                aiTask.Start();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            while (_flag == 1)
            {
                try
                {               
                    //ReadSinglePoint
                    aiTask.ReadSinglePoint(ref readValue, _channelID);
                    Console.WriteLine("Channel " + _channelID + " input " + readValue +
                     " V Voltage value finished!");

                    Console.WriteLine("whether continuous Read  Yes/No,1:Yes,0:No");
                    _flag = Convert.ToInt16(Console.ReadLine());

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }

            try
            {
                aiTask.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //Clear the channel that was added last time
            aiTask.Channels.Clear();
            Console.WriteLine("Single mode Analog input finished, press any key to exit");
            Console.ReadKey();
        }
    }
}