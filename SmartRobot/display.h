
#ifndef _DISPLAY_H_
#define _DISPLAY_H_
#include "utils.h"

#include <Adafruit_GFX.h>
#include <Adafruit_SH110X.h>
/* Uncomment the initialize the I2C address , uncomment only one, If you get a totally blank screen try the other*/
//define i2c_Address 0x3c //initialize with the I2C addr 0x3C Typically eBay OLED's
#define i2c_Address 0x3d //initialize with the I2C addr 0x3D Typically Adafruit OLED's

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels
#define OLED_RESET -1   //   QT-PY / XIAO
Adafruit_SH1106G __display__ = Adafruit_SH1106G(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);
//Adafruit_SH1107 __display__ = Adafruit_SH1107(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);
class Display
{
private:
  static const uint16_t number_of_lines = SCREEN_HEIGHT/8;
  uint16_t pixels_per_line;
  String lines[number_of_lines];
public:
  Display()
  {
    pixels_per_line = SCREEN_HEIGHT / number_of_lines;
    for (uint16_t i = 0; i < number_of_lines; i++)
    {
      lines[i] = "";
    }
  }

  void initialize()
  {
    DDBG("initialize start");
    delay(250); // wait for the OLED to power up
    DDBG("initialize before begin");
    __display__.begin(i2c_Address, true); // Address 0x3C default
    DDBG("initialize before display");
    __display__.display();
    DDBG("initialize before clearDisplay");
    __display__.clearDisplay();
    DDBG("initialize before setTextSize");
    __display__.setTextSize(1);
    DDBG("initialize before setTextColor");
    __display__.setTextColor(SH110X_WHITE);       
    DDBG("initialize end");
  }

  void display_at(int line, String text)
  {
    lines[line] = text;
    refresh();
  }

  void refresh()
  {
    __display__.clearDisplay();
    for (int i = 0; i < number_of_lines; i++)
    {
      __display__.setCursor(0, i * pixels_per_line);
      __display__.print(lines[i]);
    }
    __display__.display();
  }
};
#endif