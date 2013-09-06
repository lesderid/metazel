using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;

namespace Metazel
{
	public class JoypadHandler : IMemoryProvider
	{
		//TODO: Add configuration options.

		private int _readNumber;
		private byte _previousWrite = byte.MaxValue;
		private readonly Joystick _joypad;
		private JoystickState _state;

		public JoypadHandler(int index)
		{
			var directInput = new DirectInput();

			var device = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices).Skip(index - 1).FirstOrDefault();

			if (device == null)
				return;

			var guid = device.InstanceGuid;

			_joypad = new Joystick(directInput, guid);
			_joypad.Properties.BufferSize = 128;
			_joypad.Acquire();
		}

		public byte this[int address]
		{
			get
			{
				_readNumber++;

				if (_state == null)
					return 0;

				switch (_readNumber)
				{
					case 1:
						return (byte) (_state.Buttons[0] ? 1 : 0);
					case 2:
						return (byte) (_state.Buttons[1] ? 1 : 0);
					case 3:
						return (byte) (_state.Buttons[8] ? 1 : 0);
					case 4:
						return (byte) (_state.Buttons[9] ? 1 : 0);
					case 5:
						return (byte) (_state.Y == 256 ? 1 : 0);
					case 6:
						return (byte) (_state.Y == 65535 ? 1 : 0);
					case 7:
						return (byte) (_state.X == 256 ? 1 : 0);
					case 8:
						return (byte) (_state.X == 65535 ? 1 : 0);
					default:
						return 1;
				}
			}
			set
			{
				if (_joypad == null)
					return;

				_joypad.Poll();

				_state = _joypad.GetCurrentState();
				_readNumber = 0;

				if (_state == null)
					Debugger.Break();

				_previousWrite = value;
			}
		}
	}
}