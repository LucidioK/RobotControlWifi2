#ifndef _UTILS_H_
#define _UTILS_H_

#define DDBG(x) serialPrint(__FILE__);serialPrint(F("@"));serialPrint(__LINE__);serialPrint(" ");serialPrintln(x);

void serialPrint(String s)   { if (Serial) {  Serial.print(s); } }
void serialPrint(int i)      { if (Serial) {  Serial.print(i); } }
void serialPrintln(String s) { if (Serial) {  Serial.println(s); } }
void serialPrintln(int i)    { if (Serial) {  Serial.println(i); } }
int xtoi(String s, int start)
{
  s = s.substring(start, start+3);
  return (int)strtol(s.c_str(), 0, 16);
}

char conversion_buffer[8];
char *itox(int n)
{
  itoa(n, conversion_buffer, 16);
  return conversion_buffer;
}

char *itoa(int n)
{
  itoa(n, conversion_buffer, 10);
  return conversion_buffer;
}

char *ftos(float n)
{
  dtostrf((double)n, sizeof(conversion_buffer)-2, 2, conversion_buffer);
  return conversion_buffer;
}


String getFromSerial(String prompt)
{
  Serial.println(prompt);
  Serial.flush();
  while (!Serial.available());
  return Serial.readStringUntil('\n');
}

int getIntFromSerial(String prompt)
{
  return xtoi(getFromSerial(prompt),0);
}

float getFloatFromSerial(String prompt)
{
  return getFromSerial(prompt).toFloat();
}

#endif