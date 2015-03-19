# Full Schematic #

![http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Electrical/MotionControl_sch.png](http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Electrical/MotionControl_sch.png)


# Details #

See [Electrical](http://code.google.com/p/modular-axis-motion-control/source/browse/trunk/+modular-axis-motion-control+--username+jaredsund%40gmail.com/Electrical/) for TinyCAD design files.


| **Component** | **Name** | **Details** | **URL**|
|:--------------|:---------|:------------|:-------|
|  | FEZ Mini | 72 Mhz Micro-Controller| [FEZ Mini](http://www.ghielectronics.com/catalog/product/134) |
| LM7805 | Linear Voltage Regulator | Supplies 5 VDC from incoming 5-35VDC input |  [DigiKey](http://search.digikey.com/scripts/DkSearch/dksus.dll?Detail&name=LM7805CT-ND) |
| TB6560AHQ | Bipolar Stepping Motor Control| 40V (max), 3.5 A (peak) | [DigiKey](http://search.digikey.com/scripts/DkSearch/dksus.dll?vendor=0&keywords=TB6560AHQ) |
| C.1 | Capacitor | 47uF | N/A |
| C.2 | Capacitor | 0.33uF | N/A |
| C.3 | Capacitor | 10uF | N/A |
| C.4 | Capacitor | 0.1uF | N/A |
| C.5 | Capacitor | 0.33nF | N/A |
| C.6 | Capacitor | 0.1uF | N/A |
| C.7 | Capacitor | 0.1uF | N/A |
| R.1 | Resistor | Current Sensing (see note) | N/A |
| R.2 | Resistor | Current Sensing (see note) | N/A |
| R.3 | Resistor | 2k Ohm | N/A |
| R.4 | Resistor | 2k Ohm| N/A |

Current Sensing Resistors (NOT WIREWOUND)
> _Value = 0.5 / Current_

> _Wattage = 0.5 X Current_


## Running off USB Power ##

To run 5VDC steppers off the USB powered mini, connect the positive power input to the encoder power output.  This will supply 5VDC to the to system.  See Image below.

<img src='http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/USBPowered.JPG' alt='http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/USBPowered.JPG' width='300' />

# Images #

Hot Wiring the prototype circuit board!

<img src='http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/wireMess2.JPG' width='400' />