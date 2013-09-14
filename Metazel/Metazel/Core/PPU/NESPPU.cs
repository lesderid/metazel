using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Metazel
{
	public class NESPPU
	{
		public readonly byte[] OAMData = new byte[256];
		public readonly byte[] PPUPaletteData = new byte[0x20];
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
			//TODO: Correctly implement PPU.

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

				CacheSprites();
			}
			else if (_scanLine >= 0 && _scanLine < 240 && _dot >= 0 && _dot < 256)
			{
				var i = Math.Abs(_frameData.Stride) * _scanLine + _dot * 3;

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

		private void CacheSprites()
		{
			for (var j = 0; j < 256; j += 4)
			{
				var y = OAMData[j] + 1;
				var tileIndex = OAMData[j + 1];
				var attributes = OAMData[j + 2];
				var x = OAMData[j + 3];

				if (y >= 0xEF || x >= 0xF9)
					continue;

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
							throw new NotImplementedException();
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
					}

				}
			}
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
			var tileNumber = _dot / 8 + (_scanLine / 8) * 32;
			var tileId = Memory[Registers.NameTableAddress + tileNumber];
			var tileX = _dot % 8;
			var tileY = _scanLine % 8;
			var byte1 = Memory[Registers.BackgroundPatternTableAddress + tileId * 16 + tileY];
			var byte2 = Memory[Registers.BackgroundPatternTableAddress + tileId * 16 + tileY + 8];
			var firstBit = byte1.GetBit(7 - tileX);
			var secondBit = byte2.GetBit(7 - tileX);

			var hyperTileX = _dot % 32;
			var hyperTileY = _scanLine % 32;

			int startBit;

			if (hyperTileX >= 16)
				startBit = hyperTileY >= 16 ? 6 : 2;
			else
				startBit = hyperTileY >= 16 ? 4 : 0;

			var attributeNumber = _dot / 32 + (_scanLine / 32) * 8;
			var attributePaletteByte = Memory[Registers.NameTableAddress + 32 * 30 + attributeNumber];

			var paletteBit0 = attributePaletteByte.GetBit(startBit);
			var paletteBit1 = attributePaletteByte.GetBit(startBit + 1);

			var color = _palette[Memory[0x3F00 + (firstBit || secondBit ? ((byte) 0)
																			  .SetBit(0, firstBit)
																			  .SetBit(1, secondBit)
																			  .SetBit(2, paletteBit0)
																			  .SetBit(3, paletteBit1) : 0)]];

			_frameBytes[i] = color.B; //B
			_frameBytes[i + 1] = color.G; //G
			_frameBytes[i + 2] = color.R; //R
		}
	}
}