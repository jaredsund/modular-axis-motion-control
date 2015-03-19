# Introduction #

This code demonstrates how to use the register class to create a hardware level high speed interruptible counter from an external input using timer3 on the FEZ Mini (LPC2387 chip). The purpose of the exercise was to capture encoder pulses in a hardware timer, and to raise user level events at a certain number of pulses captured. An interruptible pin can be used to count pulses for slower speeds (<2Khz), however this process is very CPU intensive and provokes a constant change in the control flow of the applications running. Instead, we can use the hardware timer to capture/count pulses, and once we reach a particular value, raise an event through an interrupt to notify the user level code that the number of pulses has been achieved. Since most of the work is performed in the timer registers, this eliminates unneeded burden on our CPU. The code below documents how to use the register class on a FEZ mini to achieve this very task. For detailed information on the LPC23XX timers, see chapter 23 (Timer 0/1/2/3) of the LPC23XX User manual (Rev. 02 — 11 February 2009) at http://www.keil.com/dd/docs/datashts/philips/lpc23xx_um.pdf. Also, for pin out details for the FEZ mini, see the FEZ mini schematic at http://www.ghielectronics.com/downloads/FEZ/Mini/FEZMini_sch.pdf .

Note, this code will work on all FEZ devices that use the LPC2387 (100) chip (Panda, Panda II). That is, with slight modification to the CAP and MAT pins. With a few more changes to register settings, this can also be modified to work with the 144 pin applications as well (Rhino, Domino)

This code can be viewed on the code.TinyCLR.com site at http://code.tinyclr.com/project/360/externally-triggered-high-speed-counter/

An instruction guide is also available [LPC2387 Timer.docx](http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Documents/FEZ/LPC2387%20Timer.docx)

# Details #

Registers:
| **Symbol** | **Name** | **Usage** | **Location**| **Description** |
|:-----------|:---------|:----------|:------------|:----------------|
| IR | Interrupt register | T3IR | 0xE007 4000| write(clear), read |
| TCR| Timer Control Register | T3TCR| 0xE007 4004|  used to control timer counter functions (disable/reset)|
| TC | Timer Counter | T3TC | 0xE007 4008 | incremented every PR+1 cycles, controlled through the TCR |
| PR | Prescale Register | T3PR | 0xE007 4000 | when the PC is equal to this value, the next clock increments TC, and PC is cleared |
| PC  | Prescale Counter | T3PC | 0xE007 4010 | is incremented to the value stored in PR, then the TC is incremented and PC is cleared |
| MCR | Match Control Register | T3MCR | 0xE007 4014 | used to control if an interrupt is generated  and if TC is reset when a match occurs |
| MR[3:0] | Match Register | T3MR[3:0] | 0xE007 [4024, 4020, 4010, 4018] | used to control if an interrupt is generated and if TC is reset or stopped when match occurs |
| CCR | Capture Control Register | T3CCR | 0xE007 4028 | controls which edges of the capture inputs are used to load the capture register and whether an interrupt is generated when a capture takes place |
| CR0/CR1 | Capture Register 0/1 | T3CR0/1 | 0xE007 402C/4030 | CR0/1 is loaded with the value of TC, when there is an event on CAP3.[0/1] input |
| EMR | External Match Register | T2EMR | 0xE007 403C | Controls match pins MAT3.[0/1] |
| CTCR | Count Control Register | T3CTCR | 0xE007 4070 | selects between counter and timer mode.  In counter mode it is used to select the signal and edge(s) for counting |


**Interrupt Register** _T3IR (4000)_ - Reset by writing a 1 to the specific bit.  1 shows an interrupt, 0 - None
| 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|:--|:--|:--|:--|:--|:--|:--|:--|
| R | R | CR1 | CR0 | MR3 | MR2 | MR1 | MR0 |

**Timer Control Register** _T3CR (4004)_
| 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
|:--|:--|:--|:--|:--|:--|:--|:--|
| R | R | R | R | R | R | Reset | Enabled |

**Counter Control Register** _T3CCR (4070)_
| 7 | 6 | 5 | 4 | 3-2 | 1-0 |
|:--|:--|:--|:--|:----|:----|
| R | R | R | R | Count Input Select | Counter/Timer Mode |

| 3 | 2 | Pin| 1 | 0 | Mode| Input | Edge |
|:--|:--|:---|:--|:--|:----|:------|:-----|
| 0 | 0 | CAP3.0 | 0 | 0 | Timer | PCLK | Rising|
| 0 | 1 | CAP3.1 | 0 | 1 | Counter| CAP | Rising|
| 1 | 0 | Reserved | 1 | 0 | Counter| CAP | Falling|
| 1 | 1 | Reserved | 1 | 1 | Counter| CAP | Both|

**Timer Counter Register** _T3TC (4008)_  - Only Increments, Overflow can be detected with a match register, other wise just starts at zero and continues to count.
| Base | Start| | End|
|:-----|:-----|:|:---|
| HEX | 0x0000 0000 | - | 0xFFFF FFFF|
| DEC | 0 | - | 4,294,967,295|

**Prescale Register** _T3PR (4003)_
values from 0x0000 0000 to 0xFFFF FFFF

**Prescale Count Register** _T3PC (4010)_ - PCLK when PR==0, every 2 PCLK when PR==1, etc.

![http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/Prescale_Counter.jpg](http://modular-axis-motion-control.googlecode.com/svn/trunk/%20modular-axis-motion-control%20--username%20jaredsund@gmail.com/Images/Prescale_Counter.jpg)

**Match Register** _MR0 - MR3 (4018, 401C, 4020, 4024_ - value is continuously compared to the Timer Counter value.  When they equal, action defined in the MCR can be triggered automatically   TC = MR0,..,3

**Match Control Register** _T3MCR (4014)_  - MR[0,1,2,3] = TC _do something_

| 15 | 14 | 13 | 12 | 11 10 9 | 8 7 6 | 5 4 3 |2 | 1 | 0 |
|:---|:---|:---|:---|:--------|:------|:------|:-|:--|:--|
| R | R | R | R | MR3 S,R,I | MR2 S,R,I | MR1 S,R,I | MR0 S | MR0 R | MR0 I |
|  |  |  |  |  |  |  | 1 TC AND PC STOP MR0=TC | 1 TC RESET MR0=TC | 1 INTERRUPT WHEN MR0==TC |
|  |  |  |  |  |  |  | 0 - Disabled | 0 - Disabled | 0 - Disabled |

**Capture Registers** _CR0/CR1 (402C/4030) CAP3.0/CAP3.1_ - May be loaded with the Timer Counter value when an event occurs on CAP3.0/1 - Controlled by the CCR

**Capture Control Register** _T3CCR (4028)_
| 15-6 | 5 | 4 | 3 | 2 | 1 | 0 |
|:-----|:--|:--|:--|:--|:--|:--|
| R | CAP1I | CAP1FE | CAP1RE | CAP0I | CAP0FE | CAP0RE |
|  |  |  |  | Load if CR0 causes interrupt | Falling Edge (1 then 0) _(CR0 <-- TC)_ | Rising Edge (0 then 1) _(CR0 <-- TC)_ |

**External Match Register** _T3EMR (403C)_

| 15:12 | 11:10 | 9:8 | 7:6 | 5:4 | 3 | 2 | 1 | 0 |
|:------|:------|:----|:----|:----|:--|:--|:--|:--|
| R | EMC3 | EMC2 | EMC1 | EMC0 | EM3 | EM2 | EM1 | EM0 |
|  |  |  |  |  | TC = MR3| TC = MR2 | TC = MR1 | TC = MR0|
|  |  |  |  |  | MAT3.1| MAT3.0| MAT3.1| MAT3.0|

| Bit | Bit | EMC3-0 |
|:----|:----|:-------|
| 0 | 0 | Do nothing |
| 0 | 1 | MAT3.n = Low |
| 1 | 0 | MAT3.n = High |
| 1 | 1 | MAT3.n Toggled|