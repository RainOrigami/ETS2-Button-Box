using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETS2_Button_Box_Host
{
    /// <summary>
    /// Available buttons on the button box
    /// </summary>
    public enum Button
    {
        /// <summary>
        /// Cruise control enable
        /// </summary>
        CCO = 1,
        /// <summary>
        /// Cruise control resume
        /// </summary>
        CCR = 3,
        /// <summary>
        /// Cruise control rotary encoder pin 1
        /// </summary>
        CCS1 = 7,
        /// <summary>
        /// Cruise control rotary encoder pin 2
        /// </summary>
        CCS2 = 5,
        /// <summary>
        /// Gear range high/low
        /// </summary>
        GR = 11,
        /// <summary>
        /// Differential lock engage/disengage
        /// </summary>
        DIF = 9,
        /// <summary>
        /// View selector selection 1
        /// </summary>
        VW1 = 15,
        /// <summary>
        /// View selector selection 2
        /// </summary>
        VW2 = 13,
        /// <summary>
        /// View selector selection 3
        /// </summary>
        VW3 = 19,
        /// <summary>
        /// View selector selection 4
        /// </summary>
        VW4 = 17,
        /// <summary>
        /// Hazard indicator toggle
        /// </summary>
        INDH = 23,
        /// <summary>
        /// Beacon on/off
        /// </summary>
        BC = 21,
        /// <summary>
        /// Lights selector selection 1
        /// </summary>
        LI1 = 27,
        /// <summary>
        /// Lights selector selection 2
        /// </summary>
        LI2 = 25,
        /// <summary>
        /// Lights selector selection 3
        /// </summary>
        LI3 = 31,
        /// <summary>
        /// Lights selector selection 4
        /// </summary>
        LI4 = 29,
        /// <summary>
        /// Wiper selector selection 1
        /// </summary>
        WI1 = 32,
        /// <summary>
        /// Wiper selector selection 2
        /// </summary>
        WI2 = 2,
        /// <summary>
        /// Wiper selector selection 3
        /// </summary>
        WI3 = 0,
        /// <summary>
        /// Wiper selector selection 4
        /// </summary>
        WI4 = 6,
        /// <summary>
        /// Trailer connection toggle
        /// </summary>
        TOW = 4,
        /// <summary>
        /// Horns momentary (Normal horns)
        /// </summary>
        HRN = 10,
        /// <summary>
        /// Screamer momentary (Loud horns)
        /// </summary>
        SCR = 8,
        /// <summary>
        /// Flasher momentary (momentary high beam)
        /// </summary>
        FLS = 14,
        /// <summary>
        /// Retarder on/off
        /// </summary>
        RET = 12,
        /// <summary>
        /// Retarder strength rotary encoder pin 1
        /// </summary>
        RET1 = 18,
        /// <summary>
        /// Retarder strength rotary encoder pin 2
        /// </summary>
        RET2 = 16,
        /// <summary>
        /// Engine brake on/off
        /// </summary>
        ENB = 22,
        /// <summary>
        /// Engine brake strength rotary encoder pin 1
        /// </summary>
        ENB1 = 20,
        /// <summary>
        /// Engine brake strength rotary encoder pin 2
        /// </summary>
        ENB2 = 26,
        /// <summary>
        /// E-Brake on/off
        /// </summary>
        EB = 24,
        /// <summary>
        /// Systems power on/off
        /// </summary>
        PWR = 30,
        /// <summary>
        /// Engine power enable
        /// </summary>
        ENG = 28,

        // Unused shift register inputs
        UNUSED1 = 33,
        UNUSED2 = 34,
        UNUSED3 = 35,
        UNUSED4 = 36,
        UNUSED5 = 37,
        UNUSED6 = 38,
        UNUSED7 = 39,
    }
}
