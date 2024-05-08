#ifndef _MOTOR_H_
#define _MOTOR_H_
#include <L298NX2.h>
const unsigned int EN_A = 9,
                   IN1_A = 8,
                   IN2_A = 7,

                   IN1_B = 4,
                   IN2_B = 5,
                   EN_B = 3;

// Initialize both motors
L298NX2 motors(EN_A, IN1_A, IN2_A, EN_B, IN1_B, IN2_B);
int motor_latest_l = 0, motor_latest_r = 0;
void controlMotors(int l, int r)
{
  motor_latest_l = l;
  motor_latest_r = r;
  if (l == 0 && r == 0)
  {
    motors.stop();
    return;
  }

  if (l == r)
  {
    if (l > 0)
    {
      motors.forward();
    } 
    else
    {
      motors.backward();
    }

    motors.setSpeed((short)abs(l));
    return;
  }

  if (l == 0) 
  {
    motors.stopA();
  } 
  else
  {
    if (l > 0)
    {
      motors.forwardA();
    }
    else
    {
      motors.backwardA();
    }
    motors.setSpeedA((short)abs(l));
  }

  if (r == 0)
  {
    motors.stopB();
  }
  else
  {
    if (r > 0)
    {
      motors.forwardB();
    }
    else
    {
      motors.backwardB();
    }

    motors.setSpeedB((short)abs(r));
  }
}

void stop() {
  controlMotors(0, 0);
}

#endif