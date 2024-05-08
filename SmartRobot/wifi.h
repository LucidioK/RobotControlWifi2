#ifndef _WIFI_H_
#define _WIFI_H_
#include "utils.h"
#include <WiFiNINA.h>

typedef void (*execute_when_wifi_loop_finishes_retrieving_http_content)(const String content);
typedef char* (*response_getter)();
int keyIndex = 0;                // your network key Index number (needed only for WEP)

int led =  LED_BUILTIN;
int status = WL_IDLE_STATUS;
char saved_ssid[32];
char saved_pass[64];
WiFiServer __server__(80);
class Wifi
{
public:
  bool connected = false;
  WiFiClient client;
  void printWiFiStatus() {
    // print the SSID of the network you're attached to:
    serialPrint(F("SSID: "));
    serialPrintln(SSID());

    // print your WiFi shield's IP address:
    serialPrint(F("IP Address: "));
    serialPrintln(local_ip());

    // print where to go in a browser:
    serialPrint(F("To see this page in action, open a browser to http://"));
    serialPrintln(local_ip());

    serialPrint(F("MAC: "));
    serialPrintln(MacAddress());
  }

  String SSID()
  {
    return WiFi.SSID();
  }

  String local_ip()
  {
    return IpAddressToString(WiFi.localIP());
  }

  bool initialize(char ssid[], char pass[])
  {
    serialPrint(F("wifi.initialize "));
    serialPrint(ssid);
    serialPrint(F(" "));
    serialPrintln(pass);
    strcpy(saved_ssid, ssid);
    DDBG("");
    strcpy(saved_pass, pass);
    DDBG("");
    // check for the WiFi module:
    if (WiFi.status() == WL_NO_MODULE) {
      serialPrintln("Communication with WiFi module failed!");
      // don't continue
      return false;
    }
    DDBG("before checking fw version");
    String fv = WiFi.firmwareVersion();
    if (fv < WIFI_FIRMWARE_LATEST_VERSION) {
      serialPrintln("Please upgrade the firmware");
    }

    //WiFi.setHostname("ARDUINO_ROBOT");
    DDBG("before connect");
    return connect(10000);
  }

  String statusName(int wifiStatus)
  {
    switch (wifiStatus)
    {
      case WL_NO_SHIELD:       return String("WL_NO_SHIELD");
      //case WL_NO_MODULE:       return String("WL_NO_MODULE");
      case WL_IDLE_STATUS:     return String("WL_IDLE_STATUS");
      case WL_NO_SSID_AVAIL:   return String("WL_NO_SSID_AVAIL");
      case WL_SCAN_COMPLETED:  return String("WL_SCAN_COMPLETED");
      case WL_CONNECTED:       return String("WL_CONNECTED");
      case WL_CONNECT_FAILED:  return String("WL_CONNECT_FAILED");
      case WL_CONNECTION_LOST: return String("WL_CONNECTION_LOST");
      case WL_DISCONNECTED:    return String("WL_DISCONNECTED");
      case WL_AP_LISTENING:    return String("WL_AP_LISTENINg(");
      case WL_AP_CONNECTED:    return String("WL_AP_CONNECTED");
      case WL_AP_FAILED:       return String("WL_AP_FAILED");
    }
  }
  bool connect(int delayMilliseconds) 
  {
    DDBG("STARTING connect");
    serialPrintln(MacAddress());
    int count = 0;
    WiFi.end();
    for (count = 0; status != WL_CONNECTED && count < 8; count++) {
      WiFi.disconnect();
      serialPrint("Attempting to connect to Network: ");
      serialPrint(saved_ssid);
      serialPrint(" ");
      serialPrint(saved_pass);
      status = WiFi.begin(saved_ssid, saved_pass);
      delay(delayMilliseconds);
      status = WiFi.status();
      serialPrintln(String(" Status: " + statusName(status)));
    }

    if (status != WL_CONNECTED) 
    {
      serialPrintln("Failed to connect to Network.");      
      return false;
    }


    delay(delayMilliseconds);
    // start the web server on port 80
    __server__.begin();

    // you're connected now, so print out the status
    printWiFiStatus();
    client = __server__.available();
    connected = true;
    return true;  
  }

  String IpAddressToString(const IPAddress& ipAddress) 
  {
    return String(ipAddress[0]) + String(".") + 
          String(ipAddress[1]) + String(".") +
          String(ipAddress[2]) + String(".") +
          String(ipAddress[3]);
  }

  String MacAddress() 
  {
    uint8_t macAddress[WL_MAC_ADDR_LENGTH];
    String s = "";
    uint8_t* p = WiFi.macAddress(macAddress);
    for (uint8_t i = 0; i < WL_MAC_ADDR_LENGTH; i++)
    {
      s += String(macAddress[WL_MAC_ADDR_LENGTH-i-1], HEX);
      if (i < WL_MAC_ADDR_LENGTH - 1)
        s += String(".");
    }

    return s;
  }

  String wifi_get()
  {
    String line = "";
    if (client)
    {
      while (client.connected() && client.available())
      {
        char c = client.read();
        if (c == '\n')
        { 
          break;
        }
        if (c >= ' ')
        {
          line += c;
        }
      }
      //client.stop();
    }
    return line;
  }

  void wifi_post(String line)
  {
    if (client)
    {
      if (client.connected() && client.available())
      {
        client.println(line);
      }
      //client.stop();
    }
  }

  void wifi_loop(execute_when_wifi_loop_finishes_retrieving_http_content exec, response_getter content_getter)
  {
      // compare the previous status to the current status
    if (status != WiFi.status()) {
      // it has changed update the variable
      status = WiFi.status();

      if (status == WL_AP_CONNECTED) {
        // a device has connected to the AP
        serialPrintln("Device connected to AP");
      } else {
        // a device has disconnected from the AP, and we are back in listening mode
        serialPrintln("Device disconnected from AP, retrying connection");
        if (!connect(200))
        {
          serialPrintln("Could not reconnect");
        }
      }
    }

    WiFiClient client = __server__.available();   // listen for incoming clients
    if (client) {                             // if you get a client,
      serialPrintln("new client");           // print a message out the serial port
      String currentLine = "";                // make a String to hold incoming data from the client
      String commandLine = "";
      while (client.connected()) {            // loop while the client's connected
        if (client.available()) {             // if there's bytes to read from the client,
          char c = client.read();             // read a byte, then
          //Serial.write(c);                    // print it out the serial monitor
          if (c == '\n') {                    // if the byte is a newline character

            // if the current line is blank, you got two newline characters in a row.
            // that's the end of the client HTTP request, so send a response:
            if (currentLine.length() == 0) {
              // HTTP headers always start with a response code (e.g. HTTP/1.1 200 OK)
              // and a content-type so the client knows what's coming, then a blank line:
              client.println("HTTP/1.1 200 OK");
              client.println("Content-type:application/json");
              client.println();
              client.print(content_getter());
              // The HTTP response ends with another blank line:
              client.println();
              // break out of the while loop:
              serialPrintln(String("Before exec ["+commandLine+"]"));
              exec(commandLine);
              break;
            }
            else {      // if you got a newline, then clear currentLine:
              serialPrintln(currentLine);
              if (currentLine.length() > 0 && commandLine.length() == 0) {
                commandLine = currentLine;
                int httpPosition = commandLine.indexOf(" HTTP");
                if (httpPosition > 0)
                {
                  commandLine = commandLine.substring(0, httpPosition);
                }
              }
              currentLine = "";
            }
          }
          else if (c != '\r') {    // if you got anything else but a carriage return character,
            currentLine += c;      // add it to the end of the currentLine
          }
        }
      }
      // close the connection:
      client.stop();
      serialPrintln("client disconnected");
    }
  }
};
#endif