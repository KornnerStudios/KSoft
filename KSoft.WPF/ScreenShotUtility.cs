using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace KSoft.WPF
{
	public static class ScreenShotUtility
	{
		public static Bitmap Take()
		{
			int screenX = (int)SystemParameters.VirtualScreenWidth;
			int screenY = (int)SystemParameters.VirtualScreenHeight;
			int screenLeft = (int)SystemParameters.VirtualScreenLeft;
			int screenTop = (int)SystemParameters.VirtualScreenTop;

			var ret = new Bitmap(screenX, screenY, PixelFormat.Format32bppRgb);

			using (var graphics = Graphics.FromImage(ret))
			{
				graphics.CopyFromScreen(
					screenLeft, screenTop,
					0, 0,
					new System.Drawing.Size(screenX, screenY),
					CopyPixelOperation.SourceCopy);
			}

			return ret;
		}
	};
}
