The scope of this project is to create an inexpensive open source intelligent single axis motion control solution.  The single axis device is modular in that many can be combined to create a multi-axis solution.

Problem:  There is no (or, non found) availability of an inexpensive (open source) intelligent motion control solution.

Solution: Create an open source intelligent motion control system which can be constructed inexpensively.

General Requirements:
  1. To accept detailed command sets
  1. To accept/monitor digital feedback (pulsed input, i.e. encoder)
  1. To accept digital inputs for limits/home/etc.
  1. To general controls for operation of the following
    * Stepper, 40VDC (max) upto 3.5 amps
    * Brushed DC
    * RC Servo

Motion Control Basics:

<img src='http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/MotionControlFlow.jpg' width='700' />

Motion Control Solution"
<img src='http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/MotionControlFlowSolution.jpg' width='700' />

Indexer:

Micro Controller - Can be any, but for now we'll be using the GHI FEZ Mini

Needs:

  * FeedBack - Main purpose(s)
    * ulsed Input (encoder {1,600 Pulses RPM per channel (quad)})
    * Limit Switches (2 plus home , min)
  * Command Inputs - command set and interface needs
    * to be determined (standard buses, Mach3 -> CNC software)
  * Driver Outputs
    * PWM
    * Pulsed Output
  * Command Output (results)
    * simple digital Output (flag, interrupt, etc.)
    * Command result set

Driver:

  * Bipolar Stepper-motor Driver
    * TB6560AHQ (Toshiba) 3.5 amp peak, (Digikey $4.73)
  * RC Servo
    * Direct from PWM
  * Brushed DC
    * should be able to run this from the TB6560AHQ chip