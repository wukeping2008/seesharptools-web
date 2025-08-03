using System;
using JYUSB1601;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;

///<summary>
/// Author: JYTEK
/// Date modified: 2023.6.6
/// Driver version: JYUSB1601 Installer_V0.0.3.msi or later
/// Installation package: SeeSharpTools.JY.GUI 1.4.7 or later
/// Use environment: .NET 4.0 or later
/// </summary>
namespace Console_AO_Single_Point
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("USB-1601 Single Channel Analog Output in Single Mode");

            Console.WriteLine("Please Input BoardNumber：");
            string _boardNum = Console.ReadLine();
            
            JYUSB1601AOTask aoTask=null;
            try
            {
                //New aoTask based on the selected Solt Number
                aoTask = new JYUSB1601AOTask(_boardNum);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Console.WriteLine("Please Input ChannelID：");
            int _channelID = Convert.ToInt32(Console.ReadLine());
            //AddChannel
            for(int i=0;i<2;i++)
            {
                aoTask.AddChannel(i);
            }
            

            double[] writeValue = new double[2];
            for(int i = 0; i < 2; i++) 
            {
                writeValue[i] = 0;
            }
            

            //Basic parameter configuration
            aoTask.Mode = AOMode.Single;
            aoTask.WriteSinglePoint(writeValue);
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


            int _flag= 1;

            while (_flag==1)
            {
                Console.WriteLine("Please Input WriteValue，range：(-10,10)：");
                double _writeValue = Convert.ToDouble(Console.ReadLine());

                for (int i = 0; i < 2; i++)
                {
                    if (i == _channelID)
                    {
                        writeValue[i] = _writeValue;
                    }
                    else
                    {
                        writeValue[i] = 0;
                    }
                        
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

                Console.WriteLine("Channel " + _channelID + " Output " + writeValue[_channelID] +
                    " V Voltage value finished,whether continuous Write Yes/No,1:Yes,0:No");
                _flag = Convert.ToInt32(Console.ReadLine());
            }

            try
            {
                aoTask.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            Console.WriteLine("Single mode voltage value has been output, press any key to exit");
            Console.ReadKey();
        }
    }
}
