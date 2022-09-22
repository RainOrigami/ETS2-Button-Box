# ETS2-Button-Box
Open Source Button Box with LEDs for Euro Truck Simulator 2

Check out the demo video on YouTube:

[![Demo Video](https://img.youtube.com/vi/kai-H34dEho/0.jpg)](https://www.youtube.com/watch?v=kai-H34dEho)


# License

[![CC-BY-SA-4.0](https://i.creativecommons.org/l/by-sa/4.0/88x31.png)](http://creativecommons.org/licenses/by-sa/4.0/)

Contributions by [Arduino](https://www.arduino.cc/)

# Features

## Input Buttons, Switches, Selectors and Rotary Encoders

- Elecrical Power Enable/Disable
- Engine Enable
- E-Brake Enable/Disable
- Engine Brake Enable/Disable
- Engine Brake Increase/Decrease
- Retarder Enable/Disable
- Retarder Increase/Decrease
- Flasher (High Beam/Brights)
- Screamer (Momentary)
- Horn (Momentary)
- Beacon Enable/Disable
- Trailer Toggle (Attach/Detach)
- Wipers (Off, Min, Low, High)
- Lights (Off, Day, Night, High Beam/Brights)
- Hazard Indicator Toggle (Enable/Disable)
- View switch (Inside, Outside, Birds View, Freelook)
- Differential Enable/Disable
- Gear Range High/Low
- Cruise Control On/Off/Resume
- Cruise Control Increase/Decrease

## Status Indicator LEDs
- Electrical Power Status (On/Off)
- Engine Status (On/Off)
- Engine Fault (On/Off)
- Fuel Status (10 LED Fuel Gauge + 1 LED Reserve)
- Retarder Status (On/Off)
- Engine Brake Status (On/Off)
- Trailer Status (Attached/Detached)
- Horn (On)
- Screamer (On)
- Flasher (On)
- High Beam/Brights Status (On/Off)
- Indicators Left, Right, Hazard Lamp Status (On/Off)
- Differential Status (On/Off)
- Cruise Control Status (On/Off)

# Hardware

The project is based on an Arduino Nano ATmega168.

Custom Arduino sketches can easly be uploaded through the USB Serial Interface.

# Software

The project consists of three software stages:

## Stage 1 - Arduino

The Arduino stage provides Serial communication with the host through USB and handles inputs (buttons, switches, selectors, rotary encoders) and outputs (LEDs).

## Stage 2 - Host application

The host application is written in C# and handles communication between ETS2 and the Arduino.

It simulates a keyboard to send inputs to ETS2 and uses the ETS2 Telemetry SDK for reading information from the game.

## Stage 3 - ETS2

ETS2 uses the Telemetry SDK to provide information about the game and the truck to the host application.

# How to get a button box

**The box is currently under development. Please be patient until it has been sufficiently tested.** Expected release date: Whenever the part shortage is finally over q_q

The box will be available as a set for self-assemly on eBay for the price of the parts, shipping and a small 5€ fee for supporting the creator. Some minor soldering and box assembly will be required.

Current price of components:
- PCB: ?€
- Buttons/Switches/Selectors/Rotary Encoders: ~25€
- LEDs: ?€
- Acrylic Box (Lasercut): 10€
- Rubber feet: <1€
- USB Cable: 3€
- Internal cables: <1€
- (Optional) Carbon Vinyl Frontplate: 4€

Estimated total of set: ?€ excl. shipping

Please note that, as the box is still in development, the price may be subject to change before the release.

Otherwise all components of the ETS2 Button Box are open source, you are free to take the PCB layout, schematics, Arduino sketch and host application and build or modify the box yourself by any means and use your own project box, buttons, switches, LEDs, etc.

# Major changes in v2.7

Since the PCB supplier in Djina is having trouble getting all the required components at a reasonable price (>50€ for an ATmega...) I have completely reworked the PCB to use an Arduino Nano instead.

I have also decided to use individually adressable WS2811-based RGB LEDs. They are only a little bit more expensive than single color LEDs and it saves a lot of hassle connecting every single LED through a resistor to the PCB. Instead only three connections are required for all the LEDs and they can easily be expanded simply by soldering to the last LED.

The arduino code has been completely reworked to be more flexible. It is now not necessary anymore for the arduino to know each button and LED by name. This is now only done in the host software. The serial transmission protocol has also been adjusted to use magic numbers, packet length indicators and binary data instead of transmitting everything as a string.

Code interaction with the host application using the new arduino base has not been fully tested. A test project has been used to validate functionality of connection, LED and button handling on arduino.

For v2.8 I will try and integrate most LEDs, buttons, switches, selectors and rotary encoders directly on the PCB for even easier assembly. Only some chunky ones like the key switches should require external cabeling.

It is highly possible that v2.8 as a release candidate will be available by the end of the year to be ordered and assembled. It should be a little cheaper even with the individually adressable RGBs as you will have to provide your own arduino nano :)