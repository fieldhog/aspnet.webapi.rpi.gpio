using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace aspnet.webapi.rpi.gpio
{
    public static class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("-d")) {
                Console.WriteLine("waiting for debugger attach");
                for (; ; )
                {
                    Console.Write(".");
                    if (Debugger.IsAttached) break;
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine("Hello World!");

            Pi.Init<BootstrapWiringPi>();

            //Test();

            //BlinkLed();

            //Test1602();

            //PulseLED();

            ShiftRegister();

            Console.WriteLine("Goodbye World!");
        }

        static void ShiftRegister()
        {
            var dataPin = Pi.Gpio[BcmPin.Gpio17];
            var latchPin = Pi.Gpio[BcmPin.Gpio27];
            var clockPin = Pi.Gpio[BcmPin.Gpio22];

            dataPin.PinMode = GpioPinDriveMode.Output;
            latchPin.PinMode = GpioPinDriveMode.Output;
            clockPin.PinMode = GpioPinDriveMode.Output;

            const int updateDelay = 50;

            var output = "abcdefghijklmnopqrstuvwxyz  serial to prallel converter";
            foreach(var c in Encoding.ASCII.GetBytes(output)) {
                latchPin.Write(false);
                ShiftOut(dataPin, clockPin, true, c);
                latchPin.Write(true);
                Thread.Sleep(updateDelay);
            }
        }

        static void ShiftOut(IGpioPin dPin, IGpioPin cPin, bool lsbFirst, int val)
        {
            const int shiftDelay = 1;

            int i; for (i = 0; i < 8; i++)
            {
                cPin.Write(false);

                if (lsbFirst)
                    dPin.Write((0x01 & (val >> i)) == 0x01); 
                else
                    dPin.Write((0x80 & (val << i)) == 0x80);

                Thread.Sleep(shiftDelay);
                cPin.Write(true);
            }
        }

        static void Test()
        {
            Console.WriteLine("Total I2C devices = " + Pi.I2C.Devices.Count);
        }

        static void PulseLED()
        {
            var pin = (GpioPin)Pi.Gpio[BcmPin.Gpio18];
            pin.PinMode = GpioPinDriveMode.PwmOutput;
            pin.PwmMode = PwmMode.Balanced;
            pin.PwmClockDivisor = 2;
            while (true)
            {
                for (var x = 0; x <= 100; x++)
                {
                    pin.PwmRegister = (int)pin.PwmRange / 100 * x;
                    Thread.Sleep(5);
                }

                for (var x = 0; x <= 100; x++)
                {
                    pin.PwmRegister = (int)pin.PwmRange - ((int)pin.PwmRange / 100 * x);
                    Thread.Sleep(5);
                }
            }
        }

        static void Test1602()
        {
            using (Lcd1602 lcd = new Lcd1602(new I2C("/dev/i2c-1", 0x27))) //0x27 address of LCD
            {
                lcd.Init();
                lcd.Clear();

                var screenWidth = 16;
                var message = "Hello Krysia,  how are you today?";
                var text = message.PadLeft(message.Length + screenWidth).PadRight(message.Length + 2 * screenWidth);

                for (var i = 0; i < text.Length - screenWidth + 1; i++)
                {
                    lcd.Write(0, 0, text.Substring(i, screenWidth));
                    Thread.Sleep(250);
                }
            }
        }

        static void BlinkLed()
        {
            // Get a reference to the pin you need to use.
            var blinkingPin = Pi.Gpio[BcmPin.Gpio17];
            var buttonPin = Pi.Gpio[BcmPin.Gpio18];

            // Configure the pin as an output
            blinkingPin.PinMode = GpioPinDriveMode.Output;
            buttonPin.PinMode = GpioPinDriveMode.Input;

            while (buttonPin.Read().Equals(false)) { } //Loop until button is pressed

            Console.WriteLine("Someone pressed my button.");

            // perform writes to the pin by toggling the isOn variable
            var isOn = false;
            for (var i = 0; i < 200; i++)
            {
                isOn = !isOn;
                blinkingPin.Write(isOn);
                Thread.Sleep(2000);
            }
        }
    }
}
