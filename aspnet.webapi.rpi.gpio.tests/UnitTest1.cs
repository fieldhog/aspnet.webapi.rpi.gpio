using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace aspnet.webapi.rpi.gpio.tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (Lcd1602 lcd = new Lcd1602(new I2CMock("/dev/i2c-1", 0x27))) //0x27 address of LCD
            {
                Console.WriteLine("Init");
                lcd.Init();
                Console.WriteLine("Clear");
                lcd.Clear();
                Console.WriteLine("Hello");
                lcd.Write(0, 0, "Hello");
                Console.WriteLine("World!");
                lcd.Write(0, 1, "World!");
            }
        }
    }
}
