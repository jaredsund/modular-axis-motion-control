using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;

using GHIElectronics.NETMF.Hardware.LowLevel;
using GHIElectronics.NETMF.Hardware;

namespace FEZ_Panda_Counter
{
    public class Program
    {
        
        private static Register T2TCR;

        static void IntButton_OnInterrupt(uint port, uint state, DateTime time)
        {
            T2TCR.SetBits((1 << 1));
            T2TCR.ClearBits((1 << 1));
        }

        public static void Main()
        {

            PWM pwm = new PWM((PWM.Pin)FEZ_Pin.PWM.Di10);
            pwm.Set(false);
            pwm.Set(500000, 50);

            InterruptPort IntButton =new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.LDR, true,
                                   Port.ResistorMode.PullUp,
                                   Port.InterruptMode.InterruptEdgeBoth);

            // add an interrupt handler to the pin
            IntButton.OnInterrupt +=new NativeEventHandler(IntButton_OnInterrupt);

            
            //this is the power control register (periferials) - PCONP
            Register PCONP = new Register(0xE01FC0C4);
            PCONP.SetBits(1 << 22);//enable timer2 

            
            // Select IO16 on FEZ
            Register PINSEL0 = new Register(0xE002C000);
            PINSEL0.SetBits((3 << 8));//set bits 8 and 9

            // To enable timer/counter
            T2TCR = new Register(0xE0070004);
            T2TCR.Write(1);
            /*
             * Timer Control Register, The Timer Control Register (TCR) is used to control the operation of the Timer/Counter.
             * See page 554, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            // set prescale to 0
            Register T2PR = new Register(0xE007000C);
            T2PR.Write(0);
            /*
             * Prescale register: The 32-bit Prescale register specifies the maximum value for the Prescale Counter.
             * See page 556, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            Register T2CTCR = new Register(0xE0070070);
            T2CTCR.Write(2 << 0 | 0 << 2);//count on falling edge and use CAPn.0
            /*
             * Count Control Register
             * See Page 555, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            // should be 0 for a counter
            Register T2CCR = new Register(0xE0070028);
            T2CCR.ClearBits(0x07);
            /*
             * Capture Control Register. The CCR
             * controls which edges of the capture inputs
             * are used to load the Capture Registers
             * and whether or not an interrupt is
             * generated when a capture takes place.
             * 
             * Each Capture register is associated with a device pin and may be loaded with the Timer
             * Counter value when a specified event occurs on that pin. The settings in the Capture
             * Control Register register determine whether the capture function is enabled, and whether
             * a capture event happens on the rising edge of the associated pin, the falling edge, or on
             * both edges
             * 
             * see Page 558, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            // Don't do anything on match
            Register T2MCR = new Register(0xE0070014);
            T2MCR.Write(0);
            /*
             *Match Control Register (T[0/1/2/3]MCR - 0xE000 4014, 0xE000 8014,
             *0xE007 0014, 0xE007 4014) 
             *See Page 557, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            // To reset the counter
            T2TCR.SetBits((1 << 1));
            T2TCR.ClearBits((1 << 1));

            // To read
            Register T2TC = new Register(0xE0070008);
            while (true)
            {
                 
                 Debug.Print("Total count: " + T2TC.Read());

                Thread.Sleep(10);
            }

            
        }

    }
}
