// IMPORTANT: Arduino IDE does not have an INCLUDE setting.
// As such, I needed to include the full path in the includes below.
// So, change c:/dsv/RobotControlWIFI/SmartRobot/ to the location
// of SmartRobot.ino.
//
// Install these libraries:
// 1. QMC5883LCompass
//

#define VERSION "TEST_2405070918"
#if !( defined(__AVR_ATmega4809__) || defined(ARDUINO_AVR_UNO_WIFI_REV2) || defined(ARDUINO_AVR_NANO_EVERY) || \
      defined(ARDUINO_AVR_ATmega4809) || defined(ARDUINO_AVR_ATmega4808) || defined(ARDUINO_AVR_ATmega3209) || \
      defined(ARDUINO_AVR_ATmega3208) || defined(ARDUINO_AVR_ATmega1609) || defined(ARDUINO_AVR_ATmega1608) || \
      defined(ARDUINO_AVR_ATmega809) || defined(ARDUINO_AVR_ATmega808) )
#error This is designed only for Arduino or MegaCoreX megaAVR board! Please check your Tools->Board setting
#endif

#include <Arduino.h>
#include <SPI.h>
#include <Wire.h>

// These define's must be placed at the beginning before #include "megaAVR_TimerInterrupt.h"
// _TIMERINTERRUPT_LOGLEVEL_ from 0 to 4
// Don't define _TIMERINTERRUPT_LOGLEVEL_ > 0. Only for special ISR debugging only. Can hang the system.
#define TIMER_INTERRUPT_DEBUG         0
#define _TIMERINTERRUPT_LOGLEVEL_     0

// Select USING_16MHZ     == true for  16MHz to Timer TCBx => shorter timer, but better accuracy
// Select USING_8MHZ      == true for   8MHz to Timer TCBx => shorter timer, but better accuracy
// Select USING_250KHZ    == true for 250KHz to Timer TCBx => shorter timer, but better accuracy
// Not select for default 250KHz to Timer TCBx => longer timer,  but worse accuracy
#define USING_16MHZ     true
#define USING_8MHZ      false
#define USING_250KHZ    false

#define USE_TIMER_0     false
#define USE_TIMER_1     true
#define USE_TIMER_2     false
#define USE_TIMER_3     false
#define TIMER1_INTERVAL_MS 200
// Can be included as many times as necessary, without `Multiple Definitions` Linker Error
#include "megaAVR_TimerInterrupt.h"   //https://github.com/khoih-prog/megaAVR_TimerInterrupt

#include "c:/dsv/RobotControlWIFI/SmartRobot/utils.h"
#include "c:/dsv/RobotControlWIFI/SmartRobot/wifi.h"
#include "c:/dsv/RobotControlWIFI/SmartRobot/distance.h"
#include "c:/dsv/RobotControlWIFI/SmartRobot/motor.h"
//#include "c:/dsv/RobotControlWIFI/SmartRobot/display.h" //D
#include "c:/dsv/RobotControlWIFI/SmartRobot/accellerometer.h"
#include "c:/dsv/RobotControlWIFI/SmartRobot/compass.h"
#include "c:/dsv/RobotControlWIFI/SmartRobot/secrets.h"
#define WIFI_LINE     0
#define IP_LINE       1
#define VERSION_LINE  2
#define SENSOR_LINE_1 3
#define SENSOR_LINE_2 4
#define SENSOR_LINE_3 4
#define MESSAGE_LINE  6

#if !defined(LED_BUILTIN)
	#define LED_BUILTIN     13
#endif

unsigned int outputPin1 = LED_BUILTIN;


char ssid[] = SSID; // your network SSID (name)
char pass[] = WIFI_PASSWORD; // your network password (use for WPA, or use as key for WEP)

// Display display; //D
// String displayText;
Wifi wifi;
bool wifi_initialized = false;
bool wifi_running = false;
Accellerometer acc;
Compass  cmp;
Distance dst;

int previousDist = 0.0;
volatile int option;
volatile byte i2cAddress = 0;
#define MAX_I2C_ADDRESS 126
volatile int timerLock = 0;
volatile int i2cSeekWasSuccessful = 0;


void controlMotorsWithCurrentLineIfNeeded(String currentLine)
{
  if (currentLine.indexOf(F("GET /M")) >= 0)
  {
    serialPrintln(currentLine);
    int l = xtoi(currentLine, 6);
    int r = xtoi(currentLine, 9);
    if (l >= -255 && l <=255 && r >= -255 && r <= 255)
    {
      controlMotors(l, r);
    }
  }
}

void setup()
{
  Wire.begin();

  Serial.begin(74880);
  while (!Serial) {}

  Serial.println(VERSION);
	Serial.println(BOARD_NAME);
	Serial.println(MEGA_AVR_TIMER_INTERRUPT_VERSION);
	Serial.print(F("CPU Frequency = "));
	Serial.print(F_CPU / 1000000);
	Serial.println(F(" MHz"));
  Serial.print(F("TCB Clock Frequency = "));
  acc.initialize();
}

void i2cScan()
{
  int nDevices = 0;

  Serial.println(F("Scanning...\nIMPORTANT: if this hangs, check the sensor cables."));

  for (byte address = 1; address < 127; ++address) {
    // The i2c_scanner uses the return value of
    // the Wire.endTransmission to see if
    // a device did acknowledge to the address.
    Wire.beginTransmission(address);
    byte error = Wire.endTransmission();

    if (error == 0) {
      Serial.print(F("I2C device at 0x"));
      if (address < 16) {
        Serial.print("0");
      }
      Serial.println(address, HEX);
      ++nDevices;
    } else if (error == 4) {
      Serial.print(F("Error at 0x"));
      if (address < 16) {
        Serial.print("0");
      }
      Serial.println(address, HEX);
    }
  }
  if (nDevices == 0) {
    Serial.println(F("No I2C devices found\n"));
  } else {
    Serial.println(F("Done\n"));
  }
}

bool wifi_exec(String currentLine)
{
  // Check to see if the client request was "GET /H" or "GET /L":
  if (currentLine.endsWith(F("GET /H"))) {
    digitalWrite(led, HIGH);               // GET /H turns the LED on
    Serial.println("LED ON");
  }

  if (currentLine.endsWith(F("GET /L"))) {
    digitalWrite(led, LOW);                // GET /L turns the LED off
    Serial.println(F("LED OFF"));
  }

  if (currentLine.endsWith(F("GET /I"))) {
    digitalWrite(led, LOW);                // GET /L turns the LED off
    Serial.println(F("LED OFF"));
  }


  if (currentLine.endsWith(F("GET /Q"))) {
    Serial.println(F("STOPPING HTTP SERVER"));
    wifi_running = false;
    return false;
  }

  return true;
}

char response_buffer[256];
char* wifi_response()
{
  float x,y,z;
  //strcpy(response_buffer, "HTTP/1.1 200 OK\nContent-type:application/json\n");

  acc.get_accellerations(&x, &y, &z);
  strcat(response_buffer, "{");
  strcat(response_buffer, "\"robot_name\": \"GARY THE SMARTROBOT\"");
  strcat(response_buffer, ",\"version\":\""); strcat(response_buffer, VERSION); strcat(response_buffer, "\"");
  strcat(response_buffer, ",\"rnd\":"); strcat(response_buffer, itoa(analogRead(A1)));
  strcat(response_buffer, ",\"acx\":"); strcat(response_buffer, ftos(x));
  strcat(response_buffer, ",\"acy\":"); strcat(response_buffer, ftos(y));
  strcat(response_buffer, ",\"acz\":"); strcat(response_buffer, ftos(z));
  strcat(response_buffer, ",\"azi\":"); strcat(response_buffer, ftos(cmp.get_azimuth()));
  strcat(response_buffer, ",\"dir\":\""); strcat(response_buffer, cmp.get_direction().c_str()); strcat(response_buffer, "\"");
  strcat(response_buffer, ",\"mtl\":"); strcat(response_buffer, itoa(motor_latest_l));
  strcat(response_buffer, ",\"mtr\":"); strcat(response_buffer, itoa(motor_latest_r));
  strcat(response_buffer, "}\n\n");
  return response_buffer;
}

void testWifi()
{
  wifi_initialized = wifi.initialize(ssid, pass);
  if (!wifi_initialized)
  {
    Serial.println(F("Could not initialize wifi..."));
    return;
  }
  wifi.printWiFiStatus();
  wifi_running = true;
}

// void testDisplay()
// {
//   display.initialize();
//   for (int i = 0; i < SCREEN_HEIGHT/8 ; i++)
//   {
//     Serial.println(String("Line "+String(i)));
//     display.display_at(i, String("Line "+String(i)));
//   }
// }

void testDistance()
{
  Serial.println(F("Reading distance for about 10 seconds..."));
  dst.initialize();
  unsigned long start = millis();
  while (millis() - start < 10000)
  {
    Serial.println(String(String(dst.getDistanceInCentimeters())+"cm"));
  }
  Serial.println(F("Done.\n"));
}

void testCompass()
{
  //Serial.println(__LINE__);
  Serial.println(F("Reading heading for about 60 seconds..."));
  //Serial.println(__LINE__);
  cmp.initialize();
  //Serial.println(__LINE__);
  unsigned long  start = millis();
  Serial.println(start);
  while (millis() - start < 60000L)
  {
    Serial.println(String(cmp.get_azimuth())+F(" ")+String(cmp.get_bearing())+F(" ")+cmp.get_direction());
    Serial.println();
    delay(250);
  }
  Serial.println("Done.\n");
}

void calibrateCompass()
{
  QMC5883LCompass compass;
  compass.init();
  compass.setMagneticDeclination(DECLINATION_DEGREES, DECLINATION_MINUTES);
  String s = getFromSerial(F("\n\n\nStarting calibration.\nPressing Enter, move the magnetometer in all directions.\nPress Enter when ready: "));
  Serial.println(F("Calibration will begin in 5 seconds."));
  delay(5000);

  Serial.println(F("CALIBRATING. Keep moving your sensor..."));
  compass.calibrate();

  Serial.println(F("DONE. Copy the lines below to compass_parameters.h"));
  Serial.println();
  Serial.print(F("#define CALIBRATION_X_OFFSET "));Serial.println(compass.getCalibrationOffset(0));
  Serial.print(F("#define CALIBRATION_Y_OFFSET "));Serial.println(compass.getCalibrationOffset(1));
  Serial.print(F("#define CALIBRATION_Z_OFFSET "));Serial.println(compass.getCalibrationOffset(2));
  Serial.print(F("#define CALIBRATION_X_SCALE  "));Serial.println(compass.getCalibrationScale(0) );
  Serial.print(F("#define CALIBRATION_Y_SCALE  "));Serial.println(compass.getCalibrationScale(1) );
  Serial.print(F("#define CALIBRATION_Z_SCALE  "));Serial.println(compass.getCalibrationScale(2) );
  Serial.println(F(");"));

  String yn = getFromSerial(F("Send calibration parameters now? Y/N "));
  if (yn.charAt(0) == 'y' || yn.charAt(0) == 'Y')
  {
    int declination_degrees = (int)getFloatFromSerial(F("Magnetic declination degrees: "));
    int declination_minutes = (int)getFloatFromSerial(F("Magnetic declination minutes: "));
    cmp.initialize();
    cmp.calibrate(
      compass.getCalibrationOffset(0),
      compass.getCalibrationOffset(1),
      compass.getCalibrationOffset(2),
      compass.getCalibrationScale(0) ,
      compass.getCalibrationScale(1) ,
      compass.getCalibrationScale(2) ,
      declination_degrees,
      declination_minutes
    );
  }
}

void clearCompassCalibration()
{
  cmp.clear_calibration();
}

#define LEFT_MOTOR_BIT 1
#define RIGHT_MOTOR_BIT 2
void testMotors()
{
  int l = getIntFromSerial(F("L power (hex): "));
  int r = getIntFromSerial(F("R power (hex): "));
  int seconds = getIntFromSerial(F("For how many seconds (hex): "));
  Serial.print(F("L:"));
  Serial.print(l);
  Serial.print(F(", R:"));
  Serial.print(r);
  Serial.print(F(", SECONDS:"));
  Serial.println(seconds);
  controlMotors(l, r);
  delay(seconds * 1000);
  controlMotors(0, 0);
}

void testAccelerometer()
{
  //Serial.println(__LINE__);
  String s = getFromSerial(F("About to test accelerometer. Clean the Serial Monitor, display the Serial Plotter, then hit Enter..."));

  Serial.println(F("Reading accelerometer for about 60 seconds..."));
  //Serial.println(__LINE__);
  cmp.initialize();
  //Serial.println(__LINE__);
  unsigned long  start = millis();
  Serial.println(start);
  float x,y,z;
  while (millis() - start < 60000L)
  {
    acc.get_accellerations(&x, &y, &z);
    Serial.println("ax:"+String(x)+",ay:"+String(y)+",az:"+String(z)+"base:0");
    Serial.println();
    delay(10);
  }
  Serial.println(F("Done.\n"));

}

#define OPTIONS F("\n1. i2c scan\n2. WIFI\n3. Display\n4. Distance\n5. Compass\n6. Calibrate compass\n7. Clear compass calibration.\n8. Accelerometer\n9. Motors\n")
void loop()
{
  if (wifi.connected)
  {
    wifi.wifi_loop(wifi_exec, wifi_response);
    return;
  }
  //Serial.println(__LINE__);
  option = getIntFromSerial(OPTIONS);
  //Serial.println(__LINE__);
  Serial.println(String("Selected " + String(option, HEX)));
  switch (option)
  {
    case 1:
      i2cScan();
      break;
    case 2:
      testWifi();
      break;
    // case 3:
    //   testDisplay();
    //   break;
    case 4:
      testDistance();
      break;
    case 5:
      testCompass();
      break;
    case 6:
      calibrateCompass();
      break;
    case 7:
      clearCompassCalibration();
      break;
    case 8:
      testAccelerometer();
      break;
    case 9:
      testMotors();
      break;
    default:
      Serial.println(F("Invalid option."));
  }

}


