using System.Runtime.InteropServices;

namespace aspnet.webapi.rpi
{
    public class I2C : II2C
    {
        // From: https://github.com/spotify/linux/blob/master/include/linux/i2c-dev.h
        private enum IOCTL_COMMAND
        {
            /* /dev/i2c-X ioctl commands. The ioctl's parameter is always an
            * unsigned long, except for:
            * - I2C_FUNCS, takes pointer to an unsigned long
            * - I2C_RDWR, takes pointer to struct i2c_rdwr_ioctl_data
            * - I2C_SMBUS, takes pointer to struct i2c_smbus_ioctl_data
            */

            // number of times a device address should be polled when not acknowledging 
            I2C_RETRIES = 0x0701,

            // set timeout in units of 10 ms
            I2C_TIMEOUT = 0x0702,

            // Use this slave address 
            I2C_SLAVE = 0x0703,

            // 0 for 7 bit addrs, != 0 for 10 bit 
            I2C_TENBIT = 0x0704,

            // Get the adapter functionality mask
            I2C_FUNCS = 0x0705,

            // Use this slave address, even if it is already in use by a driver!
            I2C_SLAVE_FORCE = 0x0706,

            // Combined R/W transfer (one STOP only) 
            I2C_RDWR = 0x0707,

            // != 0 to use PEC with SMBus 
            I2C_PEC = 0x0708,

            // SMBus transfer 
            I2C_SMBUS = 0x0720,
        }

        private static int OPEN_READ_WRITE = 2;

        [DllImport("libc.so.6", EntryPoint = "open")]
        public static extern int Open(string fileName, int mode);

        [DllImport("libc.so.6", EntryPoint = "close")]
        public static extern int Close(int handle);

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        private extern static int Ioctl(int handle, int request, int data);

        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        internal static extern int Read(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        internal static extern int Write(int handle, byte[] data, int length);

        private int handle = -1;

        public I2C(string file, int address)
        {
            OpenDevice(file, address);
        }

        private void OpenDevice(string file, int address)
        {
            // From: https://stackoverflow.com/a/41187358
            // The I2C slave address set by the I2C_SLAVE ioctl() is stored in an i2c_client
            // that is allocated everytime /dev/i2c-X is opened. So this information is local 
            // to each "opening" of /dev/i2c-X.
            handle = Open(file, OPEN_READ_WRITE);
            var deviceReturnCode = Ioctl(handle, (int)IOCTL_COMMAND.I2C_SLAVE, address);
        }

        public void CloseDevice()
        {
            Close(handle);
            handle = -1;
        }

        public void WriteByte(byte data)
        {
            byte[] bdata = new byte[] { data };
            Write(handle, bdata, bdata.Length);
        }
    }
}