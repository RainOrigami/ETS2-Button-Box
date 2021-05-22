#include <LibPrintf.h>

#pragma region IO Definitions
// Pins for LED shift registers
#define PIN_LED_SR_LATCH 17
#define PIN_LED_SR_CLOCK 6
#define PIN_LED_SR_DATA 16

// Amount of LED shift registers
#define LED_SR_COUNT 4

// Amount of LEDs
#define LED_COUNT LED_SR_COUNT * sizeof(uint8_t) * 8

// Note: These indices must be equal to the corresponding shift out position of the connected LED
#define LED_BR 24
#define LED_CC 12
#define LED_DIF 0
#define LED_EB 30
#define LED_EF 1
#define LED_ENB 28
#define LED_ENG 15
#define LED_FLS 22
#define LED_GR 14
#define LED_HRN 18
#define LED_IND_H 6
#define LED_IND_L 2
#define LED_IND_R 4
#define LED_PWR 13
#define LED_RET 26
#define LED_SCR 20
#define LED_TOW 16
#define LED_F0 3
#define LED_F1 5
#define LED_F2 7
#define LED_F3 25
#define LED_F4 27
#define LED_F5 29
#define LED_F6 31
#define LED_F7 17
#define LED_F8 19
#define LED_F9 21
#define LED_F10 23
#define LED_UNUSED1 8
#define LED_UNUSED2 9
#define LED_UNUSED3 10
#define LED_UNUSED4 11

// Note: These indices must be equal to the corresponding shift in position of the connected button
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
#define BTN_TOW 2
#define BTN_HRN 3
#define BTN_SCR 4
#define BTN_FLS 5
#define BTN_RET 6
#define BTN_RET1 7
#define BTN_RET2 8
#define BTN_ENB 9
#define BTN_ENB1 10
#define BTN_ENB2 11
#define BTN_EB 12
#define BTN_PWR 20
#define BTN_ENG 21

uint8_t ledStates[LED_SR_COUNT];
#pragma endregion

#pragma region IO Functions
/// <summary>
/// Enable an LED
/// </summary>
/// <param name="def">LED IO definition reference</param>
void enableLED(uint8_t led) {
	ledStates[led / (sizeof(uint8_t) * 8)] |= 1 << (led % (sizeof(uint8_t) * 8));
}

/// <summary>
/// Disable an LED
/// </summary>
/// <param name="def">LED IO definition reference</param>
void disableLED(uint8_t led) {
	ledStates[led / (sizeof(uint8_t) * 8)] &= ~(1 << (led % (sizeof(uint8_t) * 8)));
}

/// <summary>
/// Disables all LEDs
/// </summary>
void resetLEDs() {
	for (size_t i = 0; i < LED_SR_COUNT * sizeof(uint8_t) * 8; i++)
		disableLED(i);
}

/// <summary>
/// Read a button based on an IO definition
/// </summary>
/// <param name="def">Button IO definition reference</param>
/// <returns>true when button is pressed, false when released</returns>
bool readButton() {
	// TODO: implement
	return false;
}

/// <summary>
/// Write changes to LED states to the corresponding pins
/// This is mainly for buffering changes until the end of
/// the loop to prevent flashing LEDs when resetting and
/// enabling LEDs in one loop.
/// </summary>
void writeLEDs() {
	digitalWrite(PIN_LED_SR_LATCH, LOW);
	for (size_t i = 0; i < LED_SR_COUNT; i++)
		shiftOut(PIN_LED_SR_DATA, PIN_LED_SR_CLOCK, MSBFIRST, ledStates[i]);
	digitalWrite(PIN_LED_SR_LATCH, HIGH);
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
			enableLED(LED_PWR);
			enableLED(LED_IND_H);
		}
		return;
	}

	// Set last receive time for timeout
	lastReceiveTime = millis();

	// data contains a string of 0 and 1 in order of the LED enum as per host application

	// Check that received data is of equal length as the LED IO definitions
	if (LED_SR_COUNT * sizeof(uint8_t) * 8 != data.length())
	{
		// Either less or more LEDs have been sent over serial
		// To indicate this error to the user we will disable all LEDs...
		resetLEDs();

		// ...and only enable PWR and EF to show a fault and IND_L to show an LED data mismatch failure
		enableLED(LED_PWR);
		enableLED(LED_EF);
		enableLED(LED_IND_L);

		// Stop further processing
		return;
	}

	// Loop through all LEDs in the leds IO definition
	for (size_t i = 0; i < LED_SR_COUNT * sizeof(uint8_t) * 8; i++)
	{
		// Enable or disable each corresponding LED
		if (data.charAt(i) == '1')
			enableLED(i);
		else
			disableLED(i);
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

	// Initialise shift registers
	pinMode(PIN_LED_SR_LATCH, OUTPUT);
	pinMode(PIN_LED_SR_DATA, OUTPUT);
	pinMode(PIN_LED_SR_CLOCK, OUTPUT);

	// Clear all shift registers and turn all LEDs off
	resetLEDs();

	//// Initially PWR and IND_H LED is enabled to show button box is powered and ready for connection
	enableLED(LED_PWR);
	enableLED(LED_IND_H);
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