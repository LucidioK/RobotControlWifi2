#ifndef _DISTANCE_H_
#define _DISTANCE_H_
#include "utils.h"
#include <VL53L0X.h>
VL53L0X __distance__;

#define I2C_SLAVE_DEVICE_ADDRESS 0x8A
class Distance
{
public:
  void initialize()
  {
    DDBG("initialize start");
    //Wire.begin();
    DDBG("initialize before getAddress");
    uint8_t previous_address = __distance__.getAddress();
    DDBG(previous_address);
    DDBG("initialize before setAddress");
    __distance__.setAddress(I2C_SLAVE_DEVICE_ADDRESS);
    previous_address = __distance__.getAddress();
    DDBG(previous_address);    
    DDBG("initialize before init");
    __distance__.init();
    DDBG("initialize setTimeout");
    __distance__.setTimeout(500);
    DDBG("initialize startContinuous");
    __distance__.startContinuous();    
    DDBG("initialize end");
  }

  int getDistanceInCentimeters()
  {
    return (int)(__distance__.readRangeContinuousMillimeters() / 10);
  }
};
#endif