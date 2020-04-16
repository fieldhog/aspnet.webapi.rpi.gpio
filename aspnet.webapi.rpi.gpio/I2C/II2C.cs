using System.Runtime.InteropServices;

namespace aspnet.webapi.rpi
{
    public interface II2C
    {
        void CloseDevice();

        void WriteByte(byte data);
    }
}