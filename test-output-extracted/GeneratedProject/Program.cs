namespace USB1601ContinuousAcquisition
{
    class Program
    {
        static void Main(string[] args)
        {
            JYUSB1601AITask aiTask = new JYUSB1601AITask("0"); // Create task
            aiTask.Mode = AIMode.Continuous; // Configure mode
            aiTask.SampleRate = 1000; // Set sample rate to 1000Hz
            aiTask.AddChannel(0, -10, 10); // Add channel 0 with range from -10 to 10
            try
            {
                aiTask.Start(); // Start data acquisition
                double[] readValue = new double[100]; // Buffer to store read data
                while (true) // Continuously read data in a loop
                {
                    if (aiTask.AvailableSamples >= readValue.Length) // Check if enough samples are available
                    {
                        aiTask.ReadData(ref readValue, readValue.Length, -1); // Read data into buffer
                        // Process the read data or output it to the console or other means
                        Console.WriteLine($"Channel 0: {readValue[0]} V"); // Output the first sample for example
                        // Clear buffer for next read operation or use a different buffer if necessary
                        Array.Clear(readValue, 0, readValue.Length);
                    }
                }
            }
            catch (JYDriverException ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message); // Output error message if exception is thrown
            }
            finally
            {
                aiTask.Stop(); // Stop data acquisition in finally block to ensure cleanup
                aiTask.Channels.Clear(); // Clear channels configuration
            }
        }
    }
}   

This program continuously  acquires  data  from  channel  0  of  the  USB - 1601  device  in  continuous  mode  with  a  sample  rate  of  1000  Hz . It  reads  the  data  into  a  buffer  and  processes ( or  outputs ) it  after  each  read  operation . Proper  exception  handling  is  implemented   using  
try -catch -
finally
 
    blocks to handle any JYDriverException 
    and ensure cleanup of resources.