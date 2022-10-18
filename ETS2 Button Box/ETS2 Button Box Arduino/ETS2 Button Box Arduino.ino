#include <FastLED.h>

#define VERSION "2.10"	// Version number sent as part of the handshake event

#define BAUD_RATE 19200	// Use 19200 baud rate

#define PIN_LED 10			// LEDs connected to D2
#define PIN_BTN_SR_LOAD 6	// PL connected to D7
#define PIN_BTN_SR_LATCH 8	// CE connected to D6
#define PIN_BTN_SR_CLOCK 7	// CP connected to D8
#define PIN_BTN_SR_DATA 9	// BTNSD connected to D4

#define LED_COUNT 31	// Total amount of connected LEDs

#define BTN_SHIFT_REGISTER_COUNT 5	// Amount of connected parallel-in-serial-out shift registers
#define BTN_SHIFT_REGISTER_SIZE 8	// Size of connected shift registers (eg. 8 bit)

CRGB ledStates[LED_COUNT];

bool buttonStates[BTN_SHIFT_REGISTER_COUNT * BTN_SHIFT_REGISTER_SIZE];	// Array of current button states
int changedButtons[BTN_SHIFT_REGISTER_COUNT * BTN_SHIFT_REGISTER_SIZE];	// Array of changed buttons
int buttonChangeCount = 0;	// Amount of changed buttons

#pragma region LED functions
/// <summary>
/// Initialise LED driver and set default brightness to something that doesn't burn your eyes out
/// </summary>
void initialiseLeds() {
	FastLED.addLeds<WS2811Controller800Khz, PIN_LED, EOrder::RGB>(ledStates, LED_COUNT);
	FastLED.setBrightness(50);
}

/// <summary>
/// Magic number 1 has been received, handle the serial LED data and apply the changes
/// </summary>
void processLeds() {
	// Read how many LED changes have been transmitted
	size_t ledAmount = readSerialBlocking();

	for (size_t ledPosition = 0; ledPosition < ledAmount; ledPosition++)
	{
		// Read which LED to change and the RGB values to change to
		int ledIndex = readSerialBlocking();
		uint8_t red = readSerialBlocking();
		uint8_t green = readSerialBlocking();
		uint8_t blue = readSerialBlocking();

		// Set the LED to the corresponding color
		setLedColor(ledIndex, CRGB(red, green, blue));
	}

	// Apply all changes
	applyLeds();
}

/// <summary>
/// Magic number 2 has been received, handle the serial brightness data and apply the changes
/// </summary>
void processBrightness() {
	// Set the brightness from the serial data
	FastLED.setBrightness(readSerialBlocking());

	// Apply change
	applyLeds();
}

/// <summary>
/// Set the color of a specific LED. Does not apply the change
/// </summary>
/// <param name="led">LED index</param>
/// <param name="color">Color</param>
void setLedColor(uint8_t led, CRGB color) {
	ledStates[led] = color;
}

/// <summary>
/// Disable an LED by setting the color to 0 (black). Does not apply the change
/// </summary>
/// <param name="led">LED index</param>
void disableLed(uint8_t led) {
	ledStates[led] = 0;
}

/// <summary>
/// Disable all LEDs. Does not apply the change
/// </summary>
void resetLeds() {
	for (size_t i = 0; i < LED_COUNT; i++)
		disableLed(i);
}

/// <summary>
/// Apply LED colors to connected LED chain
/// </summary>
void applyLeds() {
	FastLED.show();
}
#pragma endregion

#pragma region Button functions
void processButtons() {
	// Read all buttons and process changes
	readButtons();

	// Do nothing if no change was detected
	if (buttonChangeCount == 0) {
		return;
	}

	// Send changed buttons over serial
	sendButtonData();
}

/// <summary>
/// Read all button states from shift registers
/// </summary>
void readButtons() {
	buttonChangeCount = 0;

	// Load buttons into shift registers
	digitalWrite(PIN_BTN_SR_LOAD, LOW);
	delayMicroseconds(5);
	digitalWrite(PIN_BTN_SR_LOAD, HIGH);
	delayMicroseconds(5);

	// Retrieve data from shift registers
	digitalWrite(PIN_BTN_SR_CLOCK, HIGH);
	digitalWrite(PIN_BTN_SR_LATCH, LOW);
	for (size_t shiftRegisterIndex = 0; shiftRegisterIndex < BTN_SHIFT_REGISTER_COUNT; shiftRegisterIndex++)
	{
		for (size_t shiftRegisterByteIndex = 0; shiftRegisterByteIndex < 8 / BTN_SHIFT_REGISTER_SIZE; shiftRegisterByteIndex++)
		{
			uint8_t shiftRegisterButtonStates = shiftIn(PIN_BTN_SR_DATA, PIN_BTN_SR_CLOCK, LSBFIRST);
			for (size_t shiftRegisterBitIndex = 0; shiftRegisterBitIndex < 8; shiftRegisterBitIndex++)
			{
				uint8_t buttonStatesIndex = (shiftRegisterIndex * BTN_SHIFT_REGISTER_SIZE) + (shiftRegisterByteIndex * 8) + shiftRegisterBitIndex;
				bool shiftRegisterBit = (shiftRegisterButtonStates & (1 << shiftRegisterBitIndex)) > 0;

				// Check if button has changed
				if (buttonStates[buttonStatesIndex] != shiftRegisterBit) {
					buttonStates[buttonStatesIndex] = shiftRegisterBit;
					changedButtons[buttonChangeCount++] = buttonStatesIndex;
				}
			}
		}
	}
	digitalWrite(PIN_BTN_SR_LATCH, HIGH);
}

void sendButtonData() {
	// Send magic number 3
	Serial.write(3);

	// Send amount of changed buttons
	Serial.write(buttonChangeCount);

	// Send all changed button indices and states
	for (size_t changedButtonIndex = 0; changedButtonIndex < buttonChangeCount; changedButtonIndex++)
	{
		Serial.write(changedButtons[changedButtonIndex]);
		Serial.write((int)buttonStates[changedButtons[changedButtonIndex]]);
	}
}

#pragma endregion


#pragma region Serial handling and handshake
/// <summary>
/// Serial event callback, called by Arduino when there is data available
/// </summary>
void serialEvent() {
	// Handle magic number
	switch (readSerialBlocking())
	{
	case 42:
		processHandshake();
		break;
	case 1:
		processLeds();
		break;
	case 2:
		processBrightness();
		break;
	}
}

uint8_t readSerialBlocking() {
	while (Serial.available() == 0) {}
	return Serial.read();
}

void processHandshake() {
	// Read handshake string
	String data = Serial.readStringUntil('\n');

	// Check handshake string
	if (data != "HANDSHAKE") {
		// Data has been received that was not a handshake event, this really shouldn't happen
		return;
	}

	// Handshake gets an immediate handshake reply with version
	Serial.write(42);
	Serial.write("HANDSHAKE: ");
	Serial.write(VERSION);
	Serial.write("\n");

	resetLeds();
	applyLeds();
}

#pragma endregion

#pragma region Arduino setup and loop

/// <summary>
/// Initial call when arduino is booted
/// </summary>
void setup() {
	// Start serial transmission
	Serial.begin(BAUD_RATE);

	// Setup pin modes for shift registers
	pinMode(PIN_BTN_SR_LOAD, OUTPUT);
	pinMode(PIN_BTN_SR_LATCH, OUTPUT);
	pinMode(PIN_BTN_SR_CLOCK, OUTPUT);
	pinMode(PIN_BTN_SR_DATA, INPUT);

	// Initialise and reset LEDs
	initialiseLeds();
	resetLeds();
	applyLeds();
}


/// <summary>
/// Main program loop
/// </summary>
void loop() {
	processButtons();
}

#pragma endregion