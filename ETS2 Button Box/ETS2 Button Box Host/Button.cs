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
        CCO,
        /// <summary>
        /// Cruise control resume
        /// </summary>
        CCR,
        /// <summary>
        /// Cruise control rotary encoder pin 1
        /// </summary>
        CCS1,
        /// <summary>
        /// Cruise control rotary encoder pin 2
        /// </summary>
        CCS2,
        /// <summary>
        /// Gear range high/low
        /// </summary>
        GR,
        /// <summary>
        /// Differential lock engage/disengage
        /// </summary>
        DIF,
        /// <summary>
        /// View selector selection 1
        /// </summary>
        VW1,
        /// <summary>
        /// View selector selection 2
        /// </summary>
        VW2,
        /// <summary>
        /// View selector selection 3
        /// </summary>
        VW3,
        /// <summary>
        /// View selector selection 4
        /// </summary>
        VW4,
        /// <summary>
        /// Hazard indicator toggle
        /// </summary>
        INDH,
        /// <summary>
        /// Beacon on/off
        /// </summary>
        BC,
        /// <summary>
        /// Lights selector selection 1
        /// </summary>
        LI1,
        /// <summary>
        /// Lights selector selection 2
        /// </summary>
        LI2,
        /// <summary>
        /// Lights selector selection 3
        /// </summary>
        LI3,
        /// <summary>
        /// Lights selector selection 4
        /// </summary>
        LI4,
        /// <summary>
        /// Wiper selector selection 1
        /// </summary>
        WI1,
        /// <summary>
        /// Wiper selector selection 2
        /// </summary>
        WI2,
        /// <summary>
        /// Wiper selector selection 3
        /// </summary>
        WI3,
        /// <summary>
        /// Wiper selector selection 4
        /// </summary>
        WI4,
        /// <summary>
        /// Trailer connection toggle
        /// </summary>
        TOW,
        /// <summary>
        /// Horns momentary (Normal horns)
        /// </summary>
        HRN,
        /// <summary>
        /// Screamer momentary (Loud horns)
        /// </summary>
        SCR,
        /// <summary>
        /// Flasher momentary (momentary high beam)
        /// </summary>
        FLS,
        /// <summary>
        /// Retarder on/off
        /// </summary>
        RET,
        /// <summary>
        /// Retarder strength rotary encoder pin 1
        /// </summary>
        RET1,
        /// <summary>
        /// Retarder strength rotary encoder pin 2
        /// </summary>
        RET2,
        /// <summary>
        /// Engine brake on/off
        /// </summary>
        ENB,
        /// <summary>
        /// Engine brake strength rotary encoder pin 1
        /// </summary>
        ENB1,
        /// <summary>
        /// Engine brake strength rotary encoder pin 2
        /// </summary>
        ENB2,
        /// <summary>
        /// E-Brake on/off
        /// </summary>
        EB,
        /// <summary>
        /// Systems power on/off
        /// </summary>
        PWR,
        /// <summary>
        /// Engine power enable
        /// </summary>
        ENG
    }
}
