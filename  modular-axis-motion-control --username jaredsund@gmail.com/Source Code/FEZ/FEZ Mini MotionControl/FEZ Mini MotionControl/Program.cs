﻿using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.Hardware.LowLevel;

namespace FEZ_Mini_MotionControl
{
    public class Program
    {
        //private static FEZ_Counter myCounter;
        private static bool ledState;
        private static OutputPort led;

        private static OutputPort enableSignal;
        private static OutputPort dirSignal;

        private static Register T3CR;   //Timer Control Register
        private static Register T3IR;   //Interrupt Register 
        private static Register T3MR0; // Timer3 Match Register
        private static Register EM0;
        private static Register T3TC;

        private static int programMode = 0;
        private static PWM pwm;

        private static int freq = 100;
        private const  byte duty = 50;

        private const UInt32 matchValue = 200;

        static void IntButton_OnInterrupt(uint port, uint state, DateTime time)
        {
            //reset
            T3CR.SetBits((1 << 1));
            T3CR.ClearBits((1 << 1));

            T3IR.Write(1 << 0);

            programMode = 0;
            freq = 100;
            pwm.Set(freq, duty);
        }

        static void IntDI8_OnInterrupt(uint port, uint state, DateTime time)
        {
            //The external match register caused an interrupt
            Debug.Print("INTDI8 Interrupted");
            ledState = !ledState;
            led.Write(ledState);
            

            switch (programMode)
            {
                case 0:
                case 1:
                case 2:
                    freq += 100;
                    pwm.Set(freq, duty);
                    programMode++;
                    break;
                case 3:
                case 4:
                case 5:
                    freq -= 100;
                    pwm.Set(freq, duty);
                    programMode++;
                    break;
                case 6:
                    dirSignal.Write(!dirSignal.Read());
                    programMode++;
                    break;
                default:
                    programMode = 0;
                    break;
            }

        }


        public static void Main()
        {
            pwm = new PWM((PWM.Pin)FEZ_Pin.PWM.Di10);
            freq = 100;
           

            pwm.Set(false);
            pwm.Set(freq, duty);

            enableSignal = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di2, true);
            dirSignal = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di3, true);

            OutputPort TQ1 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di4, false);
            OutputPort TQ2 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di5, false);
            OutputPort MD1 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di6, false);
            OutputPort MD2 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false);
            OutputPort DCY1 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.An6, false);
            OutputPort DCY2 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.An7, false);



            //The LDR button is used as a physical counter and interupt reset
            InterruptPort IntButton = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.LDR, true,
                                   Port.ResistorMode.PullUp,
                                   Port.InterruptMode.InterruptEdgeHigh);
            IntButton.OnInterrupt += new NativeEventHandler(IntButton_OnInterrupt);


            //DI8 is used as the MAT3.0 pin, interrupted by the External Match Register
            InterruptPort IntDI8 = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di8, true,
                                   Port.ResistorMode.PullUp,
                                   Port.InterruptMode.InterruptEdgeBoth);
            IntDI8.OnInterrupt += new NativeEventHandler(IntDI8_OnInterrupt);

            
            ledState = false;
            led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            /*PINSEL0 - 0xE002 C000
             * 21:20 P0.10 GPIO Port 0.10 TXD2 SDA2 MAT3.0 00
             * see page 157, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * Use the Pin function select register PINSEL0, to enable MAT3.0
             * on P0.10.  This is used for the External Match Register
            */
            Register PINSEL0 = new Register(0xE002C000);
            PINSEL0.Write(3 << 20);

            /*Power Control for Peripherals register (PCONP - 0xE01F C0C4)
             * 23 PCTIM3 Timer 3 power/clock control bit.
             * see pages 68-69, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * In the Power Control for Peripherals register, turn on bit 23
             * to turn power on for timer3
             */
            Register PCONP = new Register(0xE01FC0C4);
            PCONP.SetBits(1 << 23);

            /*Pin Function Select Register 1 (PINSEL1 - 0xE002 C004)
             * 15:14 P0.23 GPIO Port 0.23 AD0.0 I2SRX_CLK CAP3.0 00
             * see page 158, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * Use the Pin finction select register PINSEL1, to enable Cap3.0
             * on P0.23.  Select An0 on Mini (Cap3.0) - An0 - Mapped to the Cap3.0
             * This will be the external input for the timer, used with the Capture Control Register
             */
            Register PINSEL1 = new Register(0xE002C004);
            PINSEL1.SetBits((3 << 14));//set bits 14 and 15

            /*Timer Control Register (T[0/1/2/3]CR - 0xE000 4004, 0xE000 8004,0xE007 0004, 0xE007 4004)
             * 0 Counter Enable When one, the Timer Counter and Prescale Counter are enabled for counting. 
             * When zero, the counters are disabled
             * see pages 554-555, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This will enable timer3
             */
            T3CR = new Register(0xE0074004);
            T3CR.Write(1);

            /*TPrescale register (T0PR - T3PR, 0xE000 400C, 0xE000 800C, 0xE007 000C, 0xE007 400C)
             * The 32-bit Prescale register specifies the maximum value for the Prescale Counter.
             * see page 556, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This will set the prescale to 0 for timer3
             */
            Register T3PR = new Register(0xE007400C);
            T3PR.Write(0);

            /*Count Control Register (T[0/1/2/3]CTCR - 0xE000 4070, 0xE000 8070, 0xE007 0070, 0xE007 4070)
             * 10 Counter Mode: TC is incremented on falling edges on the CAP input selected by bits 3:2.
             * see page 555, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This will count on falling edge of CAP3.0, An0(mini)
             */
            Register T3CTCR = new Register(0xE0074070);
            T3CTCR.Write(2 << 0 | 0 << 2);

            /*Capture Control Register (T[0/1/2/3]CCR - 0xE000 4028, 0xE000 8028, 0xE007 0028, 0xE007 4028)
             * see page 558, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This is set to 0, for a counter
             */
            Register T3CCR = new Register(0xE0074028);
            T3CCR.ClearBits(0x07);
            // To reset the counter
            T3CR.SetBits((1 << 1));
            T3CR.ClearBits((1 << 1));

            /*------------------------------------------------------------------*/

            /*Match Control Register (T[0/1/2/3]MCR - 0xE000 4014, 0xE000 8014, 0xE007 0014, 0xE007 4014)
             * 0 MR0I 1 Interrupt on MR0: an interrupt is generated when MR0 matches the value in the TC
             * see page 557, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This will generate a interrupt when MR0 and TC match
             */
            Register T3MCR = new Register(0xE0074014);
            T3MCR.Write(3 << 0);  //1 - interrupt, 3 - interrupt and reset

            /*MR0 Match Register
             * see page 553, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This is value that will be tested against the TC
             */
            T3MR0 = new Register(0xE0074018);
            T3MR0.Write(matchValue); // change this value as needed

            /*Interrupt Register (T[0/1/2/3]IR - 0xE000 4000, 0xE000 8000, 0xE007 0000, 0xE007 4000)
             * MR0 Interrupt Interrupt flag for match channel 0.
            * see page 554, LPC23XX User manual Rev. 02 — 11 February 2009
            * 
            * This provides the interrupt on MR0
            */
            T3IR = new Register(0xE0074000);
            T3IR.Write(1 << 0);

            /*External Match Register (T[0/1/2/3]EMR - 0xE000 403C, 0xE000 803C, 0xE007 003C, 0xE007 403C)
             * see page 559, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * Handel the interrupt on EM0 (MAT3.0)
             * Setting bits 5:4 high, toggles the corresponding External Match bit/output (MAT3.0).
             */
            EM0 = new Register(0xE007403C);
            EM0.Write(3 << 4);

            /*Timer Counter registers (T0TC - T3TC, 0xE000 4008, 0xE000 8008, 0xE007 0008, 0xE007 4008)
             * see page 556, LPC23XX User manual Rev. 02 — 11 February 2009
             * 
             * This is the count of Timer3
             */
            T3TC = new Register(0xE0074008);


            while (true)
            {
                Debug.Print("Total count: " + T3TC.Read());
                Debug.Print("IR: " + T3IR.Read());
                Debug.Print("External Match Register: " + EM0.Read().ToString());
                Debug.Print("");

                Debug.Print("Di8: " + IntDI8.Read().ToString());
                // Sleep for 500 milliseconds
                Thread.Sleep(100);

            }
        }

    }
}
