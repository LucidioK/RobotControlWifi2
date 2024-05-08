#define VERSION "2024-05-08 14:47"
#include <Arduino.h>
#include <Wire.h>

#include "utils.h"
#include "wifi.h"
#include "distance.h"
#include "motor.h"
#include "accellerometer.h"
#include "compass.h"
#include "secrets.h"

#define WIFI_LINE     0
#define IP_LINE       1
#define VERSION_LINE  2
#define SENSOR_LINE_1 3
#define SENSOR_LINE_2 4
#define SENSOR_LINE_3 4
#define MESSAGE_LINE  6



String ssid = SSID;
String pass = WIFI_PASSWORD;
String displayText;
Wifi wifi;
bool wifi_initialized = false;
Accellerometer acc;
Compass  cmp;
Distance dst;

void controlMotorsWithCurrentLineIfNeeded(String currentLine)
{
  if (currentLine.indexOf("GET /M") >= 0) 
  {
    int l = xtoi(currentLine, 6);
    int r = xtoi(currentLine, 9);
    if (l >= -255 && l <=255 && r >= -255 && r <= 255)
    {
      String s = String(l) + " " + String(r);
      serialPrintln(s);
      controlMotors(l, r);
    }
  }
}

void wifi_exec(String currentLine)
{
  // Check to see if the client request was "GET /H" or "GET /L":
  if (currentLine.endsWith("GET /H")) {
    digitalWrite(led, HIGH);               // GET /H turns the LED on
  }
  if (currentLine.endsWith("GET /L")) {
    digitalWrite(led, LOW);                // GET /L turns the LED off
  }  
  controlMotorsWithCurrentLineIfNeeded(currentLine);
}

char response_buffer[256];
char* wifi_response()
{
  float x,y,z;
  acc.get_accellerations(&x, &y, &z);
  strcpy(response_buffer, "{");
  strcat(response_buffer, "\"robot_name\": \"GARY THE SMARTROBOT\"");
  strcat(response_buffer, ",\"version\":\""); 
    strcat(response_buffer, VERSION); 
    strcat(response_buffer, "\"");
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

void setup() 
{
  Serial.begin(74880);
  Wire.begin();
  serialPrint("Robot "); serialPrintln(VERSION);

  pinMode(led, OUTPUT);      // set the LED pin mode
  wifi_initialized = wifi.initialize(ssid.c_str(), pass.c_str());
  if (!wifi_initialized)
  {
    serialPrintln("NO WIFI!");
  }
  wifi.printWiFiStatus();
  //agt.initialize();
  dst.initialize();
  serialPrintln(F("\n\nReady!"));
}

void loop() 
{
  displayText = "";
  int dist = dst.getDistanceInCentimeters();
  if (dist < 15)
  {
    stop();
  }

  if (wifi_initialized)
    wifi.wifi_loop(wifi_exec, wifi_response);
  // else
  //   serialPrintln("NO WIFI!");
  if (Serial.available() > 0)
  {
    String s = Serial.readStringUntil('\n');
    controlMotorsWithCurrentLineIfNeeded(s);
  }
  delay(50);
}


