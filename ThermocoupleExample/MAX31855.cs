using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace ThermocoupleExample
{
    /// <summary>
    /// This is a conversion of ZingPow's .NetMF code to Win10Core.
    /// https://github.com/ZingPow/Breakout_Boards
    /// Also, see reference from Adafruit.
    /// https://github.com/adafruit/Adafruit_Python_MAX31855/blob/master/Adafruit_MAX31855/MAX31855.py
    /// Data sheet is here: https://www.adafruit.com/datasheets/MAX31855.pdf
    /// </summary>
    public class MAX31855
    {
        public enum Faults
        {
            /// <summary>
            ///     No faults detected.
            /// </summary>
            OK = 0,

            /// <summary>
            ///     Thermocouple is short-circuited to VCC.
            /// </summary>
            SHORT_TO_VCC = 1,

            /// <summary>
            ///     Thermocouple is short-circuited to GND..
            /// </summary>
            SHORT_TO_GND = 2,

            /// <summary>
            ///     Thermocouple is open (no connections).
            /// </summary>
            OPEN_CIRCUIT = 4,

            /// <summary>
            ///     Problem with thermocouple.
            /// </summary>
            GENERAL_FAULT = 5,

            /// <summary>
            ///     Out of temperature range.
            /// </summary>
            OUT_OF_RANGE = 6
        }


        private readonly int SPI_CHIP_SELECT_LINE;
        private readonly string SPI_CONTROLLER_NAME;
        private SpiDevice _device;
        private double _internal;
        private double _temperature;
        private Faults _fault = Faults.OK;

        public double Offset { get; set; }


        public MAX31855(int spiChipSelectLine, string spiControllerName)
        {
            SPI_CHIP_SELECT_LINE = spiChipSelectLine;
            SPI_CONTROLLER_NAME = spiControllerName;
        }

        public async void Init()
        {
            var spi = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
            spi.ClockFrequency = 5000000;
            spi.Mode = SpiMode.Mode0;

            string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
            var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
            _device = await SpiDevice.FromIdAsync(devicesInfo[0].Id, spi);
            if (_device == null)
                throw new DeviceNotfoundException($"SPI device not found on controller {SPI_CONTROLLER_NAME}.");
        }

        public double GetInternalFahrenheit()
        {
            GetData();

            if (double.IsNaN(_internal))
                return double.NaN;

            return _internal * 1.8 + 32.0;

        }

        public double GetTemperatureFahrenheit()
        {
            GetData();
            if (double.IsNaN(_temperature))
                return double.NaN;

            return _temperature * 1.8 + 32.0;

        }

        public double GetInternalCelcius()
        {
            GetData();
            return _internal;
        }

        public double GetTemperatureCelcius()
        {
            GetData();
            return _temperature + Offset;
        }


        private void GetData()
        {
            byte[] raw = new byte[4];
            _device.Read(raw);            
            Array.Reverse(raw);
            var data = BitConverter.ToInt32(raw, 0);

            //check for fault
            switch (data & 0x07) // first three bits are faults 0b111, if all are 0 then we have good data
            {
                case 0x0:
                    _fault = Faults.OK;
                    break;
                case 0x1:
                    _fault = Faults.OPEN_CIRCUIT;
                    break;
                case 0x2:
                    _fault = Faults.SHORT_TO_GND;
                    break;
                case 0x4:
                    _fault = Faults.SHORT_TO_VCC;
                    break;
                default:
                    _fault = Faults.GENERAL_FAULT;
                    break;
            }

            if (_fault != Faults.OK)
            {
                _temperature = double.NaN;
                _internal = double.NaN;
            }

            data >>= 4; // shift right 4 spaces (the fourth space is unused)
            _internal = (data & 0x7FF)*0.0625; // 7FF = 0b11111111111, 11 bits for the temp + 1 for the sign bit (see below)

            if ((data & 0x800) != 0) //check negative sign bit 0b100000000000
            {
                _internal += -128;
            }

            data >>= 14; //trim off everything except the thermocouple temperature, i.e. shift right the 14 bits of the internal temp bits

            _temperature = (data & 0x1FFF) * 0.25; // 1FFF = 0b1111111111111, 13 bits for the temp + 1 for the sign bit

            if ((data & 0x02000) != 0) //check negative sign bit (14th bit from the right) 0b10000000000000
            {
                _temperature += -2048;
            }
        }
    }

    public class DeviceNotfoundException : Exception
    {
        public DeviceNotfoundException(string message) : base(message)
        {
            
        }
    }
}
