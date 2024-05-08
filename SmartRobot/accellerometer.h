#ifndef _ACCELLEROMETER_H_
#define _ACCELLEROMETER_H_
#include <Arduino_LSM6DS3.h>
#include "Wire.h"
#include "SPI.h"

class Accellerometer
{
public:
  void initialize()
  {
    IMU.begin();
  }

  bool get_accellerations(float *px, float *py, float *pz)
  {
    float x=0.0,y=0.0,z=0.0;
    bool ret = IMU.accelerationAvailable();
    if (ret) {
      IMU.readAcceleration(x, y, z);
    }
    *px = x;
    *py = y;
    *pz = z;
    return ret;
  }
};
#endif