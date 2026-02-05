using System.Collections.Generic;
using LimeLibrary.Utility;

namespace LimeLibrary.Input {

public static class InputBindingPath {
  private const string GamePadRoot = "<Gamepad>/";
  private const string Keyboard = "<Keyboard>/";
  private const string Mouse = "<Mouse>/";

  private static Dictionary<InputBindingType, string> BindingDictionary { get; } = new() {
    { InputBindingType.GamePadButtonEast, $"{GamePadRoot}buttonEast" },
    { InputBindingType.GamePadButtonNorth, $"{GamePadRoot}buttonNorth" },
    { InputBindingType.GamePadButtonSouth, $"{GamePadRoot}buttonSouth" },
    { InputBindingType.GamePadButtonWest, $"{GamePadRoot}buttonWest" },

    { InputBindingType.GamePadLeftShoulder, $"{GamePadRoot}leftShoulder" },
    { InputBindingType.GamePadLeftTrigger, $"{GamePadRoot}leftTrigger" },
    { InputBindingType.GamePadRightShoulder, $"{GamePadRoot}rightShoulder" },
    { InputBindingType.GamePadRightTrigger, $"{GamePadRoot}rightTrigger" },

    { InputBindingType.GamePadDPad, $"{GamePadRoot}dpad" },
    { InputBindingType.GamePadDPadDown, $"{GamePadRoot}dpad/down" },
    { InputBindingType.GamePadDPadLeft, $"{GamePadRoot}dpad/left" },
    { InputBindingType.GamePadDPadRight, $"{GamePadRoot}dpad/right" },
    { InputBindingType.GamePadDPadUp, $"{GamePadRoot}dpad/up" },
    { InputBindingType.GamePadDPadX, $"{GamePadRoot}dpad/x" },
    { InputBindingType.GamePadDPadY, $"{GamePadRoot}dpad/y" },

    { InputBindingType.GamePadLeftStick, $"{GamePadRoot}leftStick" },
    { InputBindingType.GamePadLeftStickDown, $"{GamePadRoot}leftStick/down" },
    { InputBindingType.GamePadLeftStickLeft, $"{GamePadRoot}leftStick/left" },
    { InputBindingType.GamePadLeftStickRight, $"{GamePadRoot}leftStick/right" },
    { InputBindingType.GamePadLeftStickUp, $"{GamePadRoot}leftStick/up" },
    { InputBindingType.GamePadLeftStickX, $"{GamePadRoot}leftStick/x" },
    { InputBindingType.GamePadLeftStickY, $"{GamePadRoot}leftStick/y" },
    { InputBindingType.GamePadLeftStickPress, $"{GamePadRoot}leftStickPress" },

    { InputBindingType.GamePadRightStick, $"{GamePadRoot}rightStick" },
    { InputBindingType.GamePadRightStickDown, $"{GamePadRoot}rightStick/down" },
    { InputBindingType.GamePadRightStickLeft, $"{GamePadRoot}rightStick/left" },
    { InputBindingType.GamePadRightStickRight, $"{GamePadRoot}rightStick/right" },
    { InputBindingType.GamePadRightStickUp, $"{GamePadRoot}rightStick/up" },
    { InputBindingType.GamePadRightStickX, $"{GamePadRoot}rightStick/x" },
    { InputBindingType.GamePadRightStickY, $"{GamePadRoot}rightStick/y" },
    { InputBindingType.GamePadRightStickPress, $"{GamePadRoot}rightStickPress" },

    { InputBindingType.GamePadStart, $"{GamePadRoot}start" },
    { InputBindingType.GamePadSelect, $"{GamePadRoot}select" },

    { InputBindingType.Escape, $"{Keyboard}escape" },
    { InputBindingType.Tab, $"{Keyboard}tab" },
    { InputBindingType.Shift, $"{Keyboard}shift" },
    { InputBindingType.Space, $"{Keyboard}space" },
    { InputBindingType.Control, $"{Keyboard}ctrl" },
    { InputBindingType.Alt, $"{Keyboard}alt" },
    { InputBindingType.CapsLock, $"{Keyboard}capsLock" },
    { InputBindingType.Backspace, $"{Keyboard}backspace" },
    { InputBindingType.Enter, $"{Keyboard}enter" },
    { InputBindingType.LeftShift, $"{Keyboard}leftShift" },
    { InputBindingType.RightShift, $"{Keyboard}rightShift" },
    { InputBindingType.LeftControl, $"{Keyboard}leftCtrl" },
    { InputBindingType.RightControl, $"{Keyboard}rightCtrl" },
    { InputBindingType.LeftAlt, $"{Keyboard}leftAlt" },
    { InputBindingType.RightAlt, $"{Keyboard}rightAlt" },

    { InputBindingType.W, $"{Keyboard}w" },
    { InputBindingType.A, $"{Keyboard}a" },
    { InputBindingType.S, $"{Keyboard}s" },
    { InputBindingType.D, $"{Keyboard}d" },
    { InputBindingType.R, $"{Keyboard}r" },
    { InputBindingType.Q, $"{Keyboard}q" },
    { InputBindingType.Z, $"{Keyboard}z" },
    { InputBindingType.X, $"{Keyboard}x" },
    { InputBindingType.C, $"{Keyboard}c" },
    { InputBindingType.E, $"{Keyboard}e" },
    { InputBindingType.T, $"{Keyboard}t" },
    { InputBindingType.F, $"{Keyboard}f" },
    { InputBindingType.B, $"{Keyboard}b" },
    { InputBindingType.G, $"{Keyboard}g" },
    { InputBindingType.H, $"{Keyboard}h" },
    { InputBindingType.I, $"{Keyboard}i" },
    { InputBindingType.J, $"{Keyboard}j" },
    { InputBindingType.K, $"{Keyboard}k" },
    { InputBindingType.L, $"{Keyboard}l" },
    { InputBindingType.M, $"{Keyboard}m" },
    { InputBindingType.N, $"{Keyboard}n" },
    { InputBindingType.O, $"{Keyboard}o" },
    { InputBindingType.P, $"{Keyboard}p" },
    { InputBindingType.U, $"{Keyboard}u" },
    { InputBindingType.V, $"{Keyboard}v" },
    { InputBindingType.Y, $"{Keyboard}y" },

    { InputBindingType.Number1, $"{Keyboard}1" },
    { InputBindingType.Number2, $"{Keyboard}2" },
    { InputBindingType.Number3, $"{Keyboard}3" },
    { InputBindingType.Number4, $"{Keyboard}4" },
    { InputBindingType.Number5, $"{Keyboard}5" },
    { InputBindingType.Number6, $"{Keyboard}6" },
    { InputBindingType.Number7, $"{Keyboard}7" },
    { InputBindingType.Number8, $"{Keyboard}8" },
    { InputBindingType.Number9, $"{Keyboard}9" },
    { InputBindingType.Number0, $"{Keyboard}0" },

    { InputBindingType.Function1, $"{Keyboard}f1" },
    { InputBindingType.Function2, $"{Keyboard}f2" },
    { InputBindingType.Function3, $"{Keyboard}f3" },
    { InputBindingType.Function4, $"{Keyboard}f4" },
    { InputBindingType.Function5, $"{Keyboard}f5" },
    { InputBindingType.Function6, $"{Keyboard}f6" },
    { InputBindingType.Function7, $"{Keyboard}f7" },
    { InputBindingType.Function8, $"{Keyboard}f8" },
    { InputBindingType.Function9, $"{Keyboard}f9" },
    { InputBindingType.Function10, $"{Keyboard}f10" },
    { InputBindingType.Function11, $"{Keyboard}f11" },
    { InputBindingType.Function12, $"{Keyboard}f12" },

    { InputBindingType.Semicolon, $"{Keyboard}semicolon" },
    { InputBindingType.Comma, $"{Keyboard}comma" },
    { InputBindingType.Period, $"{Keyboard}period" },
    { InputBindingType.Slash, $"{Keyboard}slash" },

    { InputBindingType.Insert, $"{Keyboard}insert" },
    { InputBindingType.Delete, $"{Keyboard}delete" },
    { InputBindingType.Home, $"{Keyboard}home" },
    { InputBindingType.End, $"{Keyboard}end" },
    { InputBindingType.PageUp, $"{Keyboard}pageUp" },
    { InputBindingType.PageDown, $"{Keyboard}pageDown" },
    { InputBindingType.ArrowUp, $"{Keyboard}upArrow" },
    { InputBindingType.ArrowDown, $"{Keyboard}downArrow" },
    { InputBindingType.ArrowLeft, $"{Keyboard}leftArrow" },
    { InputBindingType.ArrowRight, $"{Keyboard}rightArrow" },

    { InputBindingType.NumLock, $"{Keyboard}numLock" },
    { InputBindingType.NumPadDivide, $"{Keyboard}numpadDivide" },
    { InputBindingType.NumPadMultiply, $"{Keyboard}numpadMultiply" },
    { InputBindingType.NumPadMinus, $"{Keyboard}numpadMinus" },
    { InputBindingType.NumPadPlus, $"{Keyboard}numpadPlus" },
    { InputBindingType.NumPadEnter, $"{Keyboard}numpadEnter" },
    { InputBindingType.NumPad0, $"{Keyboard}numpad0" },
    { InputBindingType.NumPad1, $"{Keyboard}numpad1" },
    { InputBindingType.NumPad2, $"{Keyboard}numpad2" },
    { InputBindingType.NumPad3, $"{Keyboard}numpad3" },
    { InputBindingType.NumPad4, $"{Keyboard}numpad4" },
    { InputBindingType.NumPad5, $"{Keyboard}numpad5" },
    { InputBindingType.NumPad6, $"{Keyboard}numpad6" },
    { InputBindingType.NumPad7, $"{Keyboard}numpad7" },
    { InputBindingType.NumPad8, $"{Keyboard}numpad8" },
    { InputBindingType.NumPad9, $"{Keyboard}numpad9" },

    { InputBindingType.LeftClick, $"{Mouse}leftButton" },
    { InputBindingType.RightClick, $"{Mouse}rightButton" },
    { InputBindingType.MiddleClick, $"{Mouse}middleButton" },
    { InputBindingType.MousePointer, $"{Mouse}position" },
    { InputBindingType.MouseScrollWheel, $"{Mouse}scroll" },
  };

  public static string Get(InputBindingType inputBindingType) {
    return BindingDictionary.TryGetValue(inputBindingType, out string path) ? path : string.Empty;
  }

  public static InputBindingType From(string inputBindingPath) {
    foreach ((var type, string path) in BindingDictionary) {
      if (path == inputBindingPath) return type;
    }

    Assertion.Assert(false, inputBindingPath);
    return default;
  }

  public static string GetAnyGamepadButtonPath() => $"{GamePadRoot}<Button>";
  public static string GetAnyGamepadPath() => $"{GamePadRoot}*";
  public static string GetAnyKeyboardKeyPath() => $"{Keyboard}anyKey";
}

}