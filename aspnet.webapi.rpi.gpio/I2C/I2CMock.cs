using System;

namespace aspnet.webapi.rpi
{
    public class I2CMock : II2C
    {
        // From: https://github.com/spotify/linux/blob/master/include/linux/i2c-dev.h

        public I2CMock(string file, int address)
        {
            OpenDevice(file, address);
        }

        private void OpenDevice(string file, int address)
        {
            Console.WriteLine($"Open Device : {file} @ 0x{Convert.ToString(address, 16)}");
        }

        public void CloseDevice()
        {
            Console.WriteLine("Close Device");
        }

        public void WriteByte(byte data)
        {
            byte[] bdata = new byte[] { data };
            Console.WriteLine(Convert.ToString(data, 2).PadLeft(8, '0'));
        }
    }
}