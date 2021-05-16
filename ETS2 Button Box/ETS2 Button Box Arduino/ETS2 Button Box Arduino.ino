#include <Adafruit_MCP23017.h>

// ### WARNING! ###
// This code is made for use with the development board
// which is very different than the production board!

#pragma region External hardware
// U1 on development board
Adafruit_MCP23017 mcp1;
// U2 on development board
Adafruit_MCP23017 mcp2;
// U3 on development board
Adafruit_MCP23017 mcp3;
#pragma endregion

#pragma region IO Definitions
/// <summary>
/// Store required information for accessing buttons or LEDs using MCP23017
/// </summary>
struct IO_DEFINITION {
	Adafruit_MCP23017* mcp;
	uint8_t pin;
	bool enabled;
};

// IO definitions for accessing LEDs using MCP23017
// Note: The order must be equal to the corresponding values of the LED enum of the host application
IO_DEFINITION leds[] = {
	{ &mcp1, 13 },	// LED_FLS
	{ &mcp1, 12 },	// LED_SCR
	{ &mcp1, 11 },	// LED_HRN
	{ &mcp1, 10 },	// LED_TOW
	{ &mcp1, 9 },	// LED_EB
	{ &mcp1, 8 },	// LED_ENB
	{ &mcp1, 7 },	// LED_RET
	{ &mcp1, 6 },	// LED_BR
	{ &mcp1, 5 },	// LED_IND_H
	{ &mcp1, 4 },	// LED_IND_R
	{ &mcp1, 3 },	// LED_IND_L
	{ &mcp1, 2 },	// LED_DIF
	{ &mcp1, 1 },	// LED_GR
	{ &mcp1, 0 },	// LED_CC
	{ &mcp1, 14 },	// LED_PWR
	{ &mcp1, 15 },	// LED_ENG
	{ &mcp2, 0 },	// LED_EF
	{ &mcp2, 1 },	// LED_F0
	{ &mcp2, 2 },	// LED_F1
	{ &mcp2, 3 },	// LED_F2
	{ &mcp2, 4 },	// LED_F3
	{ &mcp2, 5 },	// LED_F4
	{ &mcp2, 6 },	// LED_F5
	{ &mcp2, 7 },	// LED_F6
	{ &mcp2, 8 },	// LED_F7
	{ &mcp2, 9 },	// LED_F8
	{ &mcp2, 10 },	// LED_F9
	{ &mcp2, 11 }	// LED_F10
};

// IO definitions for accessing buttons using MCP23017
// Note: uses readButton(IO_DEFINITION* def)
IO_DEFINITION buttons[] = {
	{ &mcp2, 12 },	// BTN_CCO
	{ &mcp2, 13 },	// BTN_CCR
	{ &mcp2, 14 },	// BTN_CCS1
	{ &mcp2, 15 },	// BTN_CCS2
	{ &mcp3, 0 },	// BTN_GR
	{ &mcp3, 1 },	// BTN_DIF
	{ &mcp3, 2 },	// BTN_VW1
	{ &mcp3, 3 },	// BTN_VW2
	{ &mcp3, 4 },	// BTN_VW3
	{ &mcp3, 5 },	// BTN_VW4
	{ &mcp3, 6 },	// BTN_INDH
	{ &mcp3, 7 },	// BTN_BC
	{ &mcp3, 8 },	// BTN_LI1
	{ &mcp3, 9 },	// BTN_LI2
	{ &mcp3, 10 },	// BTN_LI3
	{ &mcp3, 11 },	// BTN_LI4
	{ &mcp3, 12 },	// BTN_WI1
	{ &mcp3, 13 },	// BTN_WI2
	{ &mcp3, 14 },	// BTN_WI3
	{ &mcp3, 15 }	// BTN_WI4
};

// Note: These indices must be equal to the corresponding values of the LED enum of the host application
#define LED_FLS 0
#define LED_SCR 1
#define LED_HRN 2
#define LED_TOW 3
#define LED_EB 4
#define LED_ENB 5
#define LED_RET 6
#define LED_BR 7
#define LED_IND_H 8
#define LED_IND_R 9
#define LED_IND_L 10
#define LED_DIF 11
#define LED_GR 12
#define LED_CC 13
#define LED_PWR 14
#define LED_ENG 15
#define LED_EF 16
#define LED_F0 17
#define LED_F1 18
#define LED_F2 19
#define LED_F3 20
#define LED_F4 21
#define LED_F5 22
#define LED_F6 23
#define LED_F7 24
#define LED_F8 25
#define LED_F9 26
#define LED_F10 27

// Note: These indices must be equal to the corresponding values of the Button enum of the host application
#define BTN_CCO 0
#define BTN_CCR 1
#define BTN_CCS1 2
#define BTN_CCS2 3
#define BTN_GR 4
#define BTN_DIF 5
#define BTN_VW1 6
#define BTN_VW2 7
#define BTN_VW3 8
#define BTN_VW4 9
#define BTN_INDH 10
#define BTN_BC 11
#define BTN_LI1 12
#define BTN_LI2 13
#define BTN_LI3 14
#define BTN_LI4 15
#define BTN_WI1 16
#define BTN_WI2 17
#define BTN_WI3 18
#define BTN_WI4 19

// Note: remaining buttons are not accessed through MCP23017 but dierctly through pins on the Arduino
// Note: uses readButton(uint8_t pin)
const uint8_t BTN_TOW = 2;
const uint8_t BTN_HRN = 3;
const uint8_t BTN_SCR = 4;
const uint8_t BTN_FLS = 5;
const uint8_t BTN_RET = 6;
const uint8_t BTN_RET1 = 7;
const uint8_t BTN_RET2 = 8;
const uint8_t BTN_ENB = 9;
const uint8_t BTN_ENB1 = 10;
const uint8_t BTN_ENB2 = 11;
const uint8_t BTN_EB = 12;
const uint8_t BTN_PWR = A0;
const uint8_t BTN_ENG = A1;
#pragma endregion

#pragma region IO Functions
/// <summary>
/// Enable an LED
/// </summary>
/// <param name="def">LED IO definition reference</param>
void enableLED(IO_DEFINITION* def) {
	(*def).enabled = true;
	//(*(*def).mcp).digitalWrite((*def).pin, HIGH);
}

/// <summary>
/// Disable an LED
/// </summary>
/// <param name="def">LED IO definition reference</param>
void disableLED(IO_DEFINITION* def) {
	(*def).enabled = false;
	//(*(*def).mcp).digitalWrite((*def).pin, LOW);
}

/// <summary>
/// Disables all LEDs
/// </summary>
void resetLEDs() {
	for (size_t i = 0; i < sizeof(leds) / sizeof(IO_DEFINITION); i++)
		disableLED(&leds[i]);
}

/// <summary>
/// Read a button based on an IO definition
/// </summary>
/// <param name="def">Button IO definition reference</param>
/// <returns>true when button is pressed, false when released</returns>
bool readButton(IO_DEFINITION* def) {
	return !(*(*def).mcp).digitalRead((*def).pin);
}

/// <summary>
/// Read a button based on an internal Arduino port
/// </summary>
/// <param name="pin">Arduino pin number</param>
/// <returns>true when button is pressed, false when released</returns>
bool readButton(const uint8_t pin) {
	return !digitalRead(pin);
}

/// <summary>
/// Write changes to LED states to the corresponding pins
/// This is mainly for buffering changes until the end of
/// the loop to prevent flashing LEDs when resetting and
/// enabling LEDs in one loop.
/// </summary>
void writeLEDs() {
	for (size_t i = 0; i < sizeof(leds) / sizeof(IO_DEFINITION); i++)
		(*leds[i].mcp).digitalWrite(leds[i].pin, leds[i].enabled ? HIGH : LOW);
}
#pragma endregion

#pragma region Serial
unsigned long lastReceiveTime = 0;
#define RECEIVETIMEOUT 2000

void handleLedFromSerial() {
	// Read serial data until the delimiter character is reached
	String data = Serial.readStringUntil('\x00');
	if (data.length() == 0)
	{
		// Check if no data has been received for longer than the timeout
		if (millis() - lastReceiveTime > RECEIVETIMEOUT)
		{
			// Reset all LEDs
			resetLEDs();

			// Turn on PWR and IND_H LED to show button box is powered and ready for connection
			enableLED(&leds[LED_PWR]);
			enableLED(&leds[LED_IND_H]);
		}
		return;
	}

	// Set last receive time for timeout
	lastReceiveTime = millis();
	
	// data contains a string of 0 and 1 in order of the LED enum as per host application
	
	// Check that received data is of equal length as the LED IO definitions
	if (sizeof(leds) / sizeof(IO_DEFINITION) != data.length())
	{
		// Either less or more LEDs have been sent over serial
		// To indicate this error to the user we will disable all LEDs...
		resetLEDs();

		// ...and only enable PWR and EF to show a fault and IND_L to show an LED data mismatch failure
		enableLED(&leds[LED_PWR]);
		enableLED(&leds[LED_EF]);
		enableLED(&leds[LED_IND_L]);

		// Stop further processing
		return;
	}

	// Loop through all LEDs in the leds IO definition
	for (size_t i = 0; i < sizeof(leds) / sizeof(IO_DEFINITION); i++)
	{
		// Enable or disable each corresponding LED
		if (data.charAt(i) == '1')
			enableLED(&leds[i]);
		else
			disableLED(&leds[i]);
	}
}

void sendButtonData() {
	// TODO: implement
}
#pragma endregion

#pragma region Arduino workflow
/// <summary>
/// Arduino power-on setup
/// </summary>
void setup() {
	// Setup serial connection for interacting with the host application
	Serial.begin(9600);

	// Initialise MCP23017
	mcp1.begin((uint8_t)0);
	mcp2.begin((uint8_t)1);
	mcp3.begin((uint8_t)2);

	// Initialise U1 for LEDs and turn them off
	for (size_t i = 0; i < 16; i++) {
		mcp1.pinMode(i, OUTPUT);
		mcp1.digitalWrite(i, LOW);
	}

	// Initialise U2 for LEDs and turn them off
	for (size_t i = 0; i < 12; i++) {
		mcp2.pinMode(i, OUTPUT);
		mcp2.digitalWrite(i, LOW);
	}

	// Initialise U2 for buttons
	for (size_t i = 12; i < 16; i++)
	{
		mcp2.pinMode(i, INPUT);
		mcp2.pullUp(i, HIGH);
	}

	// Initialise U3 for buttons
	for (size_t i = 0; i < 16; i++) {
		mcp3.pinMode(i, INPUT);
		mcp3.pullUp(i, HIGH);
	}

	// Initialise internal pins for buttons
	for (size_t i = 2; i < 13; i++)
		pinMode(i, INPUT_PULLUP);
	pinMode(A0, INPUT_PULLUP);
	pinMode(A1, INPUT_PULLUP);

	// Initially PWR and IND_H LED is enabled to show button box is powered and ready for connection
	enableLED(&leds[LED_PWR]);
	enableLED(&leds[LED_IND_H]);
}

/// <summary>
/// Arduino main loop
/// </summary>
void loop() {
	// TODO: handle button inputs
	// TODO: make sure this doesn't take more than 100ms in total

	handleLedFromSerial();

	writeLEDs();
}
#pragma endregion