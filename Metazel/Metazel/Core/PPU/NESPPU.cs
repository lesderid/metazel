using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Metazel
{
	public class NESPPU
	{
		public readonly byte[] OAMData = new byte[256];
		public readonly PPUPaletteData PPUPaletteData = new PPUPaletteData();
		public readonly PPURegisters Registers;
		private readonly NESEngine _engine;

		private readonly Rectangle _frameRectangle = new Rectangle(0, 0, 256, 240);
		private readonly Color[] _palette;
		private int _dot;

		private Bitmap _frame;
		private byte[] _frameBytes;
		private BitmapData _frameData;

		private int _scanLine = 241;

		private Color?[,] _spriteCacheBack;
		private Color?[,] _spriteCacheFront;

		public NESPPU(NESEngine engine)
		{
			_engine = engine;

			Registers = new PPURegisters(this, _engine);

			int[] colorPalette =
				{
					0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400,
					0x503000, 0x007800, 0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000,
					0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
					0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000, 0x000000, 0x000000,
					0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
					0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
					0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8,
					0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8, 0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
				};

			_palette = new Color[colorPalette.Length];

			for (var i = 0; i < colorPalette.Length; i++)
				_palette[i] = Color.FromArgb(colorPalette[i]);
		}

		public MemoryMap Memory
		{
			get { return _engine.PPUMemoryMap; }
		}

		public void DoCycle()
		{
			//TODO: Correctly implement PPU. (Is this necessary?)

			if (_scanLine == 240 && _dot == 0)
			{
				Marshal.Copy(_frameBytes, 0, _frameData.Scan0, Math.Abs(_frameData.Stride) * _frame.Height);

				_frame.UnlockBits(_frameData);

				_engine.SetNewFrame(_frame);
			}
			else if (_scanLine == 241 && _dot == 1)
			{
				Registers.VBlank = true;

				if (Registers.VBlankNMI)
					_engine.CPU.TriggerInterrupt(Interrupt.NMI);
			}
			else if (_scanLine == -1 && _dot == 1)
				Registers.VBlank = Registers.Sprite0Hit = Registers.SpriteOverflow = false;
			else if (_scanLine == -1 && _dot == 340)
			{
				_frameBytes = new byte[256 * 240 * 3];

				_frame = new Bitmap(256, 240, PixelFormat.Format24bppRgb);
				_frameData = _frame.LockBits(_frameRectangle, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

				_spriteCacheBack = new Color?[256, 240];
				_spriteCacheFront = new Color?[256, 240];

				_sprite0Coordinates = new List<Tuple<int, int>>();

				CacheSprites();
			}
			else if (_scanLine >= 0 && _scanLine < 240 && _dot >= 0 && _dot < 256)
			{
				var i = Math.Abs(_frameData.Stride) * _scanLine + _dot * 3;

				if (_dot == 0)
				{
					_horizontalScroll = Registers.HorizontalScroll;
					_verticalScroll = Registers.VerticalScroll;
				}

				if (Registers.SpritesVisible)
					DrawSpritesBack(i);

				if (Registers.BackgroundVisible)
					DrawBackground(i);

				if (Registers.SpritesVisible)
					DrawSpritesFront(i);

				if (_dot >= 257 && _dot <= 320)
					Registers.OAMAddress = 0;
			}

			if (_dot != 340)
				_dot++;
			else
			{
				if (_scanLine == 260)
					_scanLine = -1;
				else
					_scanLine++;

				_dot = 0;
			}
		}

		private List<Tuple<int, int>> _sprite0Coordinates;
		
		private byte _horizontalScroll;
		private byte _verticalScroll;

		private void CacheSprites()
		{
			Parallel.For(0, 64, attributeJ =>
			{
				var j = attributeJ * 4;

				var y = OAMData[j] + 1;
				var tileIndex = OAMData[j + 1];
				var attributes = OAMData[j + 2];
				var x = OAMData[j + 3];

				if (y >= 0xEF || x >= 0xF9)
					return;

				for (var dot = x; dot < x + 8 && dot >= x; dot++)
				{
					for (var scanLine = y; scanLine < y + 8 && scanLine < 240; scanLine++)
					{
						var tileX = (dot - x) % 8;
						var tileY = (scanLine - y) % 8;

						if (dot < x)
							continue;

						int patternTableAddress;
						if (Registers.LargeSprites)
							continue;
						else
							patternTableAddress = Registers.SpritePatternTableAddress;

						if (attributes.GetBit(7))
							tileY = 7 - tileY;

						var byte1 = Memory[patternTableAddress + tileIndex * 16 + tileY];
						var byte2 = Memory[patternTableAddress + tileIndex * 16 + tileY + 8];

						if (attributes.GetBit(6))
						{
							byte1 = byte1.ReverseBits();
							byte2 = byte2.ReverseBits();
						}

						var firstBit = byte1.GetBit(7 - tileX);
						var secondBit = byte2.GetBit(7 - tileX);

						if (!(firstBit || secondBit))
							continue;

						var color = _palette[Memory[0x3F10 + ((byte) 0)
																 .SetBit(0, firstBit)
																 .SetBit(1, secondBit)
																 .SetBit(2, attributes.GetBit(0))
																 .SetBit(3, attributes.GetBit(1))]];

						if (attributes.GetBit(5))
							_spriteCacheBack[dot, scanLine] = color;
						else
							_spriteCacheFront[dot, scanLine] = color;

						if (j == 0)
							_sprite0Coordinates.Add(new Tuple<int, int>(dot, scanLine));
					}

				}
			});
		}

		private void DrawSpritesBack(int i)
		{
			var color = _spriteCacheBack[_dot, _scanLine];

			if (color == null)
				return;

			_frameBytes[i] = ((Color) color).B; //B
			_frameBytes[i + 1] = ((Color) color).G; //G
			_frameBytes[i + 2] = ((Color) color).R; //R
		}

		private void DrawSpritesFront(int i)
		{
			var color = _spriteCacheFront[_dot, _scanLine];

			if (color == null)
				return;

			_frameBytes[i] = ((Color) color).B; //B
			_frameBytes[i + 1] = ((Color) color).G; //G
			_frameBytes[i + 2] = ((Color) color).R; //R
		}

		private void DrawBackground(int i)
		{
			var x = _dot + _horizontalScroll;
			var y = _scanLine + _verticalScroll;

			var nextNameTableX = new Func<int, int>(address => (address - 0x2000 + 0x400) % 0x800 + 0x2000);
			var nextNameTableY = new Func<int, int>(address => (address - 0x2000 + 0x800) % 0x1000 + 0x2000);
			var nameTableAddress = Registers.NameTableAddress;

			for (var j = 0; j < x / 256; j++)
				nameTableAddress = nextNameTableX(nameTableAddress);

			for (var j = 0; j < y / 240; j++)
				nameTableAddress = nextNameTableY(nameTableAddress);

			x %= 256;
			y %= 240;

			var tileNumber = x / 8 + (y / 8) * 32;
			var tileId = Memory[nameTableAddress + tileNumber];
			var tileX = x % 8;
			var tileY = y % 8;
			var byte1 = Memory[Registers.BackgroundPatternTableAddress + tileId * 16 + tileY];
			var byte2 = Memory[Registers.BackgroundPatternTableAddress + tileId * 16 + tileY + 8];
			var firstBit = byte1.GetBit(7 - tileX);
			var secondBit = byte2.GetBit(7 - tileX);

			var hyperTileX = x % 32;
			var hyperTileY = y % 32;

			int startBit;

			if (hyperTileX >= 16)
				startBit = hyperTileY >= 16 ? 6 : 2;
			else
				startBit = hyperTileY >= 16 ? 4 : 0;

			var attributeNumber = x / 32 + (y / 32) * 8;
			var attributePaletteByte = Memory[nameTableAddress + 32 * 30 + attributeNumber];

			var paletteBit0 = attributePaletteByte.GetBit(startBit);
			var paletteBit1 = attributePaletteByte.GetBit(startBit + 1);

			var blah = Memory[0x3F00 + (firstBit || secondBit ? ((byte) 0)
				                                                    .SetBit(0, firstBit)
				                                                    .SetBit(1, secondBit)
				                                                    .SetBit(2, paletteBit0)
				                                                    .SetBit(3, paletteBit1) : 0)];
			var color = _palette[blah & 0x3F];

			_frameBytes[i] = color.B; //B
			_frameBytes[i + 1] = color.G; //G
			_frameBytes[i + 2] = color.R; //R

			if (_sprite0Coordinates != null && (firstBit || secondBit))
			{
				for (var j = 0; j < _sprite0Coordinates.Count; j++)
				{
					if (_sprite0Coordinates[j].Item1 == _dot && _sprite0Coordinates[j].Item2 == _scanLine)
					{
						Registers.Sprite0Hit = true;

						_sprite0Coordinates = null;

						break;
					}
				}
			}
		}
	}
}