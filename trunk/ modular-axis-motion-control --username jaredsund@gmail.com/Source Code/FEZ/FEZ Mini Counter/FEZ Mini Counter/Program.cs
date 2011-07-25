using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;

using GHIElectronics.NETMF.Hardware.LowLevel;
using GHIElectronics.NETMF.Hardware;

namespace FEZ_Mini_Counter
{
    public class Program
    {
        private static Register T3TCR;
        private static Register T3IR;

        static void IntButton_OnInterrupt(uint port, uint state, DateTime time)
        {
            T3TCR.SetBits((1 << 1));
            T3TCR.ClearBits((1 << 1));
            T3IR.Write(1 << 2);
        }

        static void IntDI7_OnInterrupt(uint port, uint state, DateTime time)
        {
            Debug.Print("INTDI7 Interrupt");
        }

        static void IntDI8_OnInterrupt(uint port, uint state, DateTime time)
        {
            Debug.Print("INTDI8 Interrupt");
        }

        public static void Main()
        {

            //Disable the garbage collector messages
            Debug.EnableGCMessages(false);

            PWM pwm = new PWM((PWM.Pin)FEZ_Pin.PWM.Di3);
            pwm.Set(false);
            pwm.Set(2, 50);

            //http://www.keil.com/dd/vtr/4531/9857.htm

            InterruptPort IntButton = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.LDR, true,
                                   Port.ResistorMode.PullUp,
                                   Port.InterruptMode.InterruptEdgeBoth);

            // add an interrupt handler to the pin
            IntButton.OnInterrupt += new NativeEventHandler(IntButton_OnInterrupt);

            //InterruptPort IntDI7 = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di7 , true,
            //                       Port.ResistorMode.PullUp,
            //                       Port.InterruptMode.InterruptEdgeBoth);

            //// add an interrupt handler to the pin
            //IntDI7.OnInterrupt += new NativeEventHandler(IntDI7_OnInterrupt);

            //InterruptPort IntDI8 = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di8 , true,
            //                       Port.ResistorMode.PullUp,
            //                       Port.InterruptMode.InterruptEdgeBoth);

            //// add an interrupt handler to the pin
            //IntDI8.OnInterrupt += new NativeEventHandler(IntDI8_OnInterrupt);


            


            //this is the power control register (periferials) - PCONP
            Register PCONP = new Register(0xE01FC0C4);
            PCONP.SetBits(1 << 23);//enable timer3 


            // Select An0 on Mini (Cap3.0)
            Register PINSELCAP30 = new Register(0xE002C004);
            PINSELCAP30.SetBits((3 << 14));//set bits 14 and 15


            // To enable timer/counter
            //TCR Timer Control Register 
            //timers are 0,1,2,3 - this one is timer 3
            T3TCR = new Register(0xE0074004);
            T3TCR.Write(1);
            /*
             * Timer Control Register, The Timer Control Register (TCR) is used to control the operation of the Timer/Counter.
             * See page 554, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            // set prescale to 0
            Register T3PR = new Register(0xE007400C);
            T3PR.Write(0);

            /*
             * Prescale register: The 32-bit Prescale register specifies the maximum value for the Prescale Counter.
             * See page 556, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            Register T3CTCR = new Register(0xE0074070);
            T3CTCR.Write(2 << 0 | 0 << 2);//count on falling edge and use CAP3.0
            /*
             * Count Control Register
             * See Page 555, LPC23XX User manual Rev. 02 — 11 February 2009
             */

            // should be 0 for a counter
            Register T3CCR = new Register(0xE0074028);
            T3CCR.ClearBits(0x07);
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

            // To reset the counter
            T3TCR.SetBits((1 << 1));
            T3TCR.ClearBits((1 << 1));

            /*------------------------------------------------------------------*/
            /*
             * MCR Match Control Register. The MCR is used to control if an interrupt is generated and if 
             * the TC is reset when a Match occurs. R/W 0 
             * T0MCR - 0xE000 4014
             * T1MCR - 0xE000 8014
             * T2MCR - 0xE007 0014
             * T3MCR - 0xE007 4014
             * */

            Register T3MCR = new Register(0xE0074014);
            T3MCR.Write(1 << 6);

            /*
             * Bit
             * 6 MR2I 1 Interrupt on MR2: an interrupt is generated when MR2 matches the value in the TC.
             *  0 This interrupt is disabled
             * 7 MR2R 1 Reset on MR2: the TC will be reset if MR2 matches it. 0
             *  0 Feature disabled.
             * 8 MR2S 1 Stop on MR2: the TC and PC will be stopped and TCR[0] will be
             *  0 set to 0 if MR2 matches the TC.
             */



            /*
             * Match Register 2. MR2 can be enabled through the MCR to reset the TC, stop 
             * both the TC and PC, and/or generate an interrupt every time MR0 matches the TC.
             */
            Register T3MR2 = new Register(0xE0074020);
            T3MR2.Write(10);

            //T2IR - 0xE007 4000  Interrupt Register for timer3
            T3IR = new Register(0xE0074000);
            T3IR.Write(1 << 2);

            Register GIOI = new Register (0xE0028080);
            Register EM3 = new Register (0xE007403C);
            EM3.Write(3<<8);
            //EM3.Write(1 << 11);


            // To read
            Register T3TC = new Register(0xE0074008);
            while (true)
            {
                // if(T2IR.Read () ==1)
                //{
                Debug.Print("Total count: " + T3TC.Read());
                Debug.Print("IR: " + T3IR.Read());
                Debug.Print("");
                Debug.Print(EM3.Read().ToString());

                Thread.Sleep(10);
                //}
            }


        }

    }
}
