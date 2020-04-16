using System;
using System.Text;
using System.Threading;

namespace aspnet.webapi.rpi
{
    public class Lcd1602 : IDisposable
    {
        public Lcd1602(II2C i2C)
        {
            I2C = i2C;
        }

        private II2C I2C { get; }

        // Source for enums:
        // https://github.com/fdebrabander/Arduino-LiquidCrystal-I2C-library/blob/master/LiquidCrystal_I2C.h
        // commands
        private enum Commands
        {
            LCD_CLEARDISPLAY = 0x01,
            LCD_RETURNHOME = 0x02,
            LCD_ENTRYMODESET = 0x04,
            LCD_DISPLAYCONTROL = 0x08,
            LCD_CURSORSHIFT = 0x10,
            LCD_FUNCTIONSET = 0x20,
            LCD_SETCGRAMADDR = 0x40,
            LCD_SETDDRAMADDR = 0x80,
        }

        // flags for display entry mode
        private enum DisplayEntryMode
        {
            LCD_ENTRYRIGHT = 0x00,
            LCD_ENTRYLEFT = 0x02,
            LCD_ENTRYSHIFTINCREMENT = 0x01,
            LCD_ENTRYSHIFTDECREMENT = 0x00,
        }

        // flags for display on/off control
        private enum DisplayControl
        {
            LCD_DISPLAYON = 0x04,
            LCD_DISPLAYOFF = 0x00,
            LCD_CURSORON = 0x02,
            LCD_CURSOROFF = 0x00,
            LCD_BLINKON = 0x01,
            LCD_BLINKOFF = 0x00,
        }

        // flags for display/cursor shift
        private enum DisplayCursorShift
        {
            LCD_DISPLAYMOVE = 0x08,
            LCD_CURSORMOVE = 0x00,
            LCD_MOVERIGHT = 0x04,
            LCD_MOVELEFT = 0x00,
        }

        // flags for function set
        private enum FunctionSet
        {
            LCD_8BITMODE = 0x10,
            LCD_4BITMODE = 0x00,
            LCD_2LINE = 0x08,
            LCD_1LINE = 0x00,
            LCD_5x10DOTS = 0x04,
            LCD_5x8DOTS = 0x00,
        }

        // flags for backlight control
        private enum BacklightControl
        {
            LCD_BACKLIGHT = 0x08,
            LCD_NOBACKLIGHT = 0x00,
        }

        private enum ControlBits
        {
            En = 0x04, // Enable bit
            Rw = 0x02, // Read/Write bit
            Rs = 0x01 // Register select bit
        }

        protected void SendCommand(int comm)
        {
            byte buf;
            // Send bit7-4 firstly
            buf = (byte)(comm & 0xF0);
            buf |= (byte)ControlBits.En | (byte)BacklightControl.LCD_BACKLIGHT;
            I2C.WriteByte(buf);  //First command nibble with LED and 3 register bits
            Thread.Sleep(2);
            I2C.WriteByte((byte)BacklightControl.LCD_BACKLIGHT); // Simplified, as we should only need to set En to 0 and write

            // Send bit3-0 secondly
            buf = (byte)((comm & 0x0F) << 4);
            buf |= (byte)ControlBits.En | (byte)BacklightControl.LCD_BACKLIGHT;
            I2C.WriteByte(buf);  //Second command nibble with LED and 3 register bits
            Thread.Sleep(2);
            I2C.WriteByte((byte)BacklightControl.LCD_BACKLIGHT); // Simplified, as we should only need to set En to 0 and write
        }

        protected void SendData(int data)
        {
            //https://www.codeproject.com/Articles/1274259/The-Year-of-IoT-Hooking-up-a-2-line-LCD-Display

            byte buf;
            // Send bit7-4 firstly
            buf = (byte)(data & 0xF0);
            buf |= (byte)ControlBits.En | (byte)ControlBits.Rs; // RS = 1, RW = 0, EN = 1
            buf |= (byte)BacklightControl.LCD_BACKLIGHT;
            I2C.WriteByte(buf); //First data nibble with LED and 3 register bits
            Thread.Sleep(2);
            I2C.WriteByte((byte)BacklightControl.LCD_BACKLIGHT);  //Simplified, as we should only need to set En to 0 and write

            // Send bit3-0 secondly
            buf = (byte)((data & 0x0F) << 4);
            buf |= (byte)ControlBits.En | (byte)ControlBits.Rs; // RS = 1, RW = 0, EN = 1
            buf |= (byte)BacklightControl.LCD_BACKLIGHT;
            I2C.WriteByte(buf); //Second data nibble with LED and 3 register bits
            Thread.Sleep(2);
            I2C.WriteByte((byte)BacklightControl.LCD_BACKLIGHT); //Simplified, as we should only need to set En to 0 and write

        }

        public void Init()
        {
            SendCommand(0x33); // Must initialize to 8-line mode at first
            Thread.Sleep(2);
            SendCommand(0x32); // Then initialize to 4-line mode
            Thread.Sleep(2);
            SendCommand(0x28); // 2 Lines & 5*7 dots
            Thread.Sleep(2);
            SendCommand(0x0C); // Enable display without cursor
            Thread.Sleep(2);
            SendCommand((byte)Commands.LCD_CLEARDISPLAY); // Clear Screen
        }

        public void Clear()
        {
            SendCommand((byte)Commands.LCD_CLEARDISPLAY); //clear Screen
        }

        public void Write(int x, int y, string str)
        {
            // Move cursor
            int addr = (byte)Commands.LCD_SETDDRAMADDR + 0x40 * y + x;
            SendCommand(addr); //Set the inital dram address for text output

            byte[] charData = Encoding.ASCII.GetBytes(str);

            foreach (byte b in charData)
            {
                SendData(b);
            }
        }

        public void Dispose()
        {
            I2C.CloseDevice();
        }
    }
}