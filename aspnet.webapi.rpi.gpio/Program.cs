using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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

            //ShiftRegister();

            MatrixTest();

            Console.WriteLine("Goodbye World!");
        }

        static void MatrixTest()
        {
            var dataPin = Pi.Gpio[BcmPin.Gpio17];
            var latchPin = Pi.Gpio[BcmPin.Gpio27];
            var clockPin = Pi.Gpio[BcmPin.Gpio22];

            dataPin.PinMode = GpioPinDriveMode.Output;
            latchPin.PinMode = GpioPinDriveMode.Output;
            clockPin.PinMode = GpioPinDriveMode.Output;

            var pic = new []{ 0x1c, 0x22, 0x51, 0x45, 0x45, 0x51, 0x22, 0x1c };
            var clr = new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var nine = new[] { 0x00, 0x00, 0x32, 0x49, 0x49, 0x3E, 0x00, 0x00 };
            var eight = new[] { 0x00, 0x00, 0x36, 0x49, 0x49, 0x36, 0x00, 0x00 };
            var seven = new[] { 0x00, 0x00, 0x60, 0x47, 0x48, 0x70, 0x00, 0x00 };
            var six = new[] { 0x00, 0x00, 0x3E, 0x49, 0x49, 0x26, 0x00, 0x00 };
            var five = new[] { 0x00, 0x00, 0x79, 0x49, 0x49, 0x46, 0x00, 0x00 };
            var four = new[] { 0x00, 0x00, 0x0E, 0x32, 0x7F, 0x02, 0x00, 0x00 };
            var three = new[] { 0x00, 0x00, 0x22, 0x49, 0x49, 0x36, 0x00, 0x00 };
            var two = new[] { 0x00, 0x00, 0x23, 0x45, 0x49, 0x31, 0x00, 0x00 };
            var one = new[] { 0x00, 0x00, 0x21, 0x7F, 0x01, 0x00, 0x00, 0x00 };
            var zero = new[] { 0x00, 0x00, 0x3E, 0x41, 0x41, 0x3E, 0x00, 0x00 };


            MatrixOut(pic, TimeSpan.FromSeconds(5), latchPin, dataPin, clockPin);
            MatrixOut(clr, TimeSpan.FromMilliseconds(1), latchPin, dataPin, clockPin);
            MatrixOut(nine, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(eight, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(seven, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(six, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(five, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(four, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(three, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(two, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(one, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(zero, TimeSpan.FromSeconds(1), latchPin, dataPin, clockPin);
            MatrixOut(clr, TimeSpan.FromMilliseconds(1), latchPin, dataPin, clockPin);
        }

        static void MatrixOut(int[] matrix, TimeSpan timeSpan, IGpioPin latchPin, IGpioPin dataPin, IGpioPin clockPin)
        {
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeSpan)
            {
                var column = 0x80;
                for (var i = 0; i < 8; i++)
                {
                    latchPin.Write(false);
                    ShiftOut(dataPin, clockPin, false, matrix[i]);// first shift data of line information to the first stage 74HC959
                    ShiftOut(dataPin, clockPin, false, ~column);//then shift data of column information to the second stage 74HC959
                    latchPin.Write(true); //Output data of two stage 74HC595 at the same 
                    column >>= 1;   //display the next columndelay(1);
                    Thread.Sleep(2); //Try and keep the column LEDs on a short time to make them brighter.
                }
            }
        }

        static void ShiftOut(IGpioPin dPin, IGpioPin cPin, bool lsbFirst, int val)
        {
            int i; for (i = 0; i < 8; i++)
            {
                cPin.Write(false);

                if (lsbFirst)
                    dPin.Write((0x01 & (val >> i)) == 0x01);
                else
                    dPin.Write((0x80 & (val << i)) == 0x80);

                cPin.Write(true);
            }
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
