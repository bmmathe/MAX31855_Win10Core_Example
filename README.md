# MAX31855_Win10Core_Example
An example of how to use the MAX31855 with a Raspberry Pi 2 and Windows 10 Core.

This is ZingPow's conversion of his MAX31855 .NetMF class to Windows 10 Core.

https://github.com/ZingPow/Breakout_Boards/tree/master/Adafruit/MAX31855Thermocouple/MAX31855Thermocouple/MAX31855Thermocouple_43

Also, see Adafruit's Python example here: https://github.com/adafruit/Adafruit_Python_MAX31855/blob/master/Adafruit_MAX31855/MAX31855.py

Here is the datasheet for the MAX31855: https://www.adafruit.com/datasheets/MAX31855.pdf

I am using the hardware SPI pins on the Raspberry Pi 2.
See this Fritz on Adafruit: https://learn.adafruit.com/assets/19767
And the tutorial here: https://learn.adafruit.com/max31855-thermocouple-python-library/hardware

One gotcha if you are using the CanaKit GPIO to Breadboard Interface Board (T interface), CS0 is actually labeled CE0 on the CanaKit board.  I used the SC pin on the RP2 and wondered why I was only getting zeroes back.
