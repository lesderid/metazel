using System.Diagnostics;
using Metazel.Library;
using SharpDX.XInput;

namespace Metazel.NES
{
    public class JoypadHandler : IMemoryProvider
    {
        //TODO: Add configuration options.

        private int _readNumber;
        private byte _previousWrite = byte.MaxValue;
        private readonly Controller _controller;
        private State _controllerState;

        public JoypadHandler(int index)
        {
            _controller = new Controller((UserIndex)(index - 1));
        }

        public byte this[int address]
        {
            get
            {
                _readNumber++;

                switch (_readNumber)
                {
                    case 1: //A
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) ? 1 : 0);
                    case 2: //B
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) ? 1 : 0);
                    case 3: //Select
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back) ? 1 : 0);
                    case 4: //Start
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) ? 1 : 0);
                    case 5: //Up
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) ? 1 : 0);
                    case 6: //Down
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) ? 1 : 0);
                    case 7: //Left
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) ? 1 : 0);
                    case 8: //Right
                        return (byte)(_controllerState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) ? 1 : 0);
                    default:
                        return 1;
                }
            }
            set
            {
                if (_controller == null || !_controller.IsConnected)
                    return;

                _controllerState = _controller.GetState();

                _readNumber = 0;

                _previousWrite = value;
            }
        }
    }
}