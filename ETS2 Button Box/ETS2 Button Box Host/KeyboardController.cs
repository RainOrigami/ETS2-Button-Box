using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ETS2_Button_Box_Host.ButtonController;

namespace ETS2_Button_Box_Host
{
    public static class KeyboardController
    {
        // TODO: needs validation of active application to prevent sending key strokes to application that is not ETS2

        /// <summary>
        /// Invoke a key press (key down and key up) event on the specified key strokes
        /// </summary>
        /// <param name="keyStrokes">Which keys to press</param>
        public static void InvokeKeyPress(Keyboard.DirectXKeyStrokes keyStrokes)
        {
            Keyboard.SendKey(keyStrokes, false, Keyboard.InputType.Keyboard);
            Keyboard.SendKey(keyStrokes, true, Keyboard.InputType.Keyboard);
        }

        /// <summary>
        /// Invoke a key press (key down and key up) event on the key resolved by buttonToKeyStroke from the specified ButtonAction
        /// </summary>
        /// <param name="buttonAction">Which button action has called for the invokation</param>
        public static void InvokeKeyPress(ButtonAction buttonAction) => InvokeKeyPress(ButtonToKeyStroke[buttonAction.Button]);

        /// <summary>
        /// Invoke a key down (hold) event on the specified key stroke
        /// </summary>
        /// <param name="keyStrokes">Which keys to press down</param>
        public static void InvokeKeyDown(Keyboard.DirectXKeyStrokes keyStrokes) => Keyboard.SendKey(keyStrokes, false, Keyboard.InputType.Keyboard);

        /// <summary>
        /// Invoke a key down (hold) event on the key resolved by buttonToKeyStroke from the specified ButtonAction
        /// </summary>
        /// <param name="buttonAction">Which button action has called for the invokation</param>
        public static void InvokeKeyDown(ButtonAction buttonAction) => InvokeKeyDown(ButtonToKeyStroke[buttonAction.Button]);

        /// <summary>
        /// Invoke a key up (release) event on the specified key stroke
        /// </summary>
        /// <param name="keyStrokes">Which keys to release</param>
        public static void InvokeKeyUp(Keyboard.DirectXKeyStrokes keyStrokes) => Keyboard.SendKey(keyStrokes, true, Keyboard.InputType.Keyboard);

        /// <summary>
        /// Invoke a key up (release ) event on the key resolved by buttonToKeyStroke from the specified ButtonAction
        /// </summary>
        /// <param name="buttonAction">Which button action has called for the invokation</param>
        public static void InvokeKeyUp(ButtonAction buttonAction) => InvokeKeyUp(ButtonToKeyStroke[buttonAction.Button]);

        /// <summary>
        /// Mapping of Button Box Buttons to Keyboard Buttons
        /// </summary>
        /// TODO: Make this configurable
        public static readonly IReadOnlyDictionary<Button, Keyboard.DirectXKeyStrokes> ButtonToKeyStroke = new Dictionary<Button, Keyboard.DirectXKeyStrokes>()
        {
            { Button.BC, Keyboard.DirectXKeyStrokes.DIK_A },
            { Button.CCO, Keyboard.DirectXKeyStrokes.DIK_B },
            { Button.CCR, Keyboard.DirectXKeyStrokes.DIK_C },
            { Button.CCS1, Keyboard.DirectXKeyStrokes.DIK_D },
            { Button.CCS2, Keyboard.DirectXKeyStrokes.DIK_E },
            { Button.DIF, Keyboard.DirectXKeyStrokes.DIK_F },
            { Button.EB, Keyboard.DirectXKeyStrokes.DIK_G },
            { Button.ENB, Keyboard.DirectXKeyStrokes.DIK_H },
            { Button.ENB1, Keyboard.DirectXKeyStrokes.DIK_I },
            { Button.ENB2, Keyboard.DirectXKeyStrokes.DIK_J },
            { Button.ENG, Keyboard.DirectXKeyStrokes.DIK_K },
            { Button.FLS, Keyboard.DirectXKeyStrokes.DIK_L },
            { Button.GR, Keyboard.DirectXKeyStrokes.DIK_M },
            { Button.HRN, Keyboard.DirectXKeyStrokes.DIK_N },
            { Button.INDH, Keyboard.DirectXKeyStrokes.DIK_O },
            { Button.LI1, Keyboard.DirectXKeyStrokes.DIK_P },
            { Button.LI2, Keyboard.DirectXKeyStrokes.DIK_P },
            { Button.LI3, Keyboard.DirectXKeyStrokes.DIK_P },
            { Button.LI4, Keyboard.DirectXKeyStrokes.DIK_Q },
            { Button.PWR, Keyboard.DirectXKeyStrokes.DIK_R },
            { Button.TB, Keyboard.DirectXKeyStrokes.DIK_S },
            { Button.RET1, Keyboard.DirectXKeyStrokes.DIK_T },
            { Button.RET2, Keyboard.DirectXKeyStrokes.DIK_U },
            { Button.SCR, Keyboard.DirectXKeyStrokes.DIK_V },
            { Button.TOW, Keyboard.DirectXKeyStrokes.DIK_W },
            { Button.VW1, Keyboard.DirectXKeyStrokes.DIK_1 },
            { Button.VW2, Keyboard.DirectXKeyStrokes.DIK_2 },
            { Button.VW3, Keyboard.DirectXKeyStrokes.DIK_3 },
            { Button.VW4, Keyboard.DirectXKeyStrokes.DIK_4 },
            { Button.WI1, Keyboard.DirectXKeyStrokes.DIK_X },
            { Button.WI2, Keyboard.DirectXKeyStrokes.DIK_X },
            { Button.WI3, Keyboard.DirectXKeyStrokes.DIK_X },
            { Button.WI4, Keyboard.DirectXKeyStrokes.DIK_X }
        };
    }
}
