#ifndef _COMPASS_H_
#define _COMPASS_H_
#include "Wire.h"
#include <QMC5883LCompass.h>
#include "compass_parameters.h"

QMC5883LCompass __compass__;

class Compass
{
public:
  void initialize()
  {
    __compass__.init();
    if (USE_SMOOTHING)
    {
       start_smoothing();
    }
    calibrate(CALIBRATION_X_OFFSET, CALIBRATION_Y_OFFSET, CALIBRATION_Z_OFFSET, CALIBRATION_X_SCALE, CALIBRATION_Y_SCALE, CALIBRATION_Z_SCALE, DECLINATION_DEGREES, DECLINATION_MINUTES);
  }

  String get_direction()
  {
    char myArray[4] = {0, 0, 0, 0};
    __compass__.getDirection(myArray, get_azimuth());
    String s = String(myArray);
    s.trim();
    return s;
  }
  
  
  int get_azimuth()
  {
    __compass__.read();
    return __compass__.getAzimuth();
  }
  
  int get_bearing()        { return __compass__.getBearing(get_azimuth());                      }
  
  void calibrate(
    float x_offset, float y_offset, float z_offset, 
    float x_scale, float y_scale, float z_scale, 
    int declination_degrees, int declination_minutes)
  {
    __compass__.setReset();
    __compass__.setCalibrationOffsets(x_offset, y_offset, z_offset);
    __compass__.setCalibrationScales(x_scale, y_scale, z_scale);
    __compass__.setMagneticDeclination(declination_degrees, declination_minutes);
  }
  
  void clear_calibration() { __compass__.clearCalibration();                                    }
  
  void start_smoothing()   { __compass__.setSmoothing(SMOOTHING_STEPS, USE_ADVANCED_SMOOTHING); }
  
  void stop_smoothing()    { __compass__.setSmoothing(1, false);                                }
};

#endif