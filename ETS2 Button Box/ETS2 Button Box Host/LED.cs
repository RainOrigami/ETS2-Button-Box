using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETS2_Button_Box_Host
{
    /// <summary>
    /// Available LEDs on the button box
    /// </summary>
    public enum LED
    {
        /// <summary>
        /// Flasher (momentary high beam)
        /// </summary>
        FLS,
        /// <summary>
        /// Screamer (Loud horns)
        /// </summary>
        SCR,
        /// <summary>
        /// Horns (Normal horns)
        /// </summary>
        HRN,
        /// <summary>
        /// Trailer connection status
        /// </summary>
        TOW,
        /// <summary>
        /// E-Brake status
        /// </summary>
        EB,
        /// <summary>
        /// Engine brake status
        /// </summary>
        ENB,
        /// <summary>
        /// Retarder status
        /// </summary>
        RET,
        // TODO: remember!
        /// <summary>
        /// I forgot.
        /// </summary>
        BR,
        /// <summary>
        /// Indicator hazards
        /// </summary>
        INDH,
        /// <summary>
        /// Indicator right
        /// </summary>
        INDR,
        /// <summary>
        /// Indicator left
        /// </summary>
        INDL,
        /// <summary>
        /// Differential lock status
        /// </summary>
        DIF,
        /// <summary>
        /// Gear range
        /// </summary>
        GR,
        /// <summary>
        /// Cruise control status
        /// </summary>
        CC,
        /// <summary>
        /// Systems power status
        /// </summary>
        PWR,
        /// <summary>
        /// Engine power status
        /// </summary>
        ENG,
        /// <summary>
        /// Engine fault
        /// </summary>
        EF,
        /// <summary>
        /// Fuel 0%
        /// </summary>
        F0,
        /// <summary>
        /// Fuel 10%
        /// </summary>
        F1,
        /// <summary>
        /// Fuel 20%
        /// </summary>
        F2,
        /// <summary>
        /// Fuel 30%
        /// </summary>
        F3,
        /// <summary>
        /// Fuel 40%
        /// </summary>
        F4,
        /// <summary>
        /// Fuel 50%
        /// </summary>
        F5,
        /// <summary>
        /// Fuel 60%
        /// </summary>
        F6,
        /// <summary>
        /// Fuel 70%
        /// </summary>
        F7,
        /// <summary>
        /// Fuel 80%
        /// </summary>
        F8,
        /// <summary>
        /// Fuel 90%
        /// </summary>
        F9,
        /// <summary>
        /// Fuel 100%
        /// </summary>
        F10
    }
}
