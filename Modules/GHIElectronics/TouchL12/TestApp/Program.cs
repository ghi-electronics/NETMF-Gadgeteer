using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			DisplayDriver driver = new DisplayDriver(touchL12, display_T35);
		}
	}
	
	public class DisplayDriver
	{
		private const int FPS = 10;
		private const int MS_PER_FRAME = 1000 / DisplayDriver.FPS;

		private const uint SLIDER_CENTER_X = 160;
		private const uint SLIDER_CENTER_Y = 120;
		private const uint SLIDER_CAP_WIDTH = 25;

		private const uint TEXT_X = 5;
		private const uint TEXT_Y = 0;
		private const uint TEXT_WIDTH = 150;
		private const uint TEXT_HEIGHT = 12;
		private const uint TEXT_SPACING = 2;

		private Display_T35 display;
		private TouchL12 sensor;
		private GT.Timer renderTimer;

		private TouchL12.Direction direction;
		private double position;
		private bool touched;

		private TouchL12.Direction previousDirection;
		private double previousPosition;
		private bool previousTouched;

		public DisplayDriver(TouchL12 sensor, Display_T35 display)
		{
			this.display = display;
			this.sensor = sensor;

			this.direction = (TouchL12.Direction)(-2);
			this.previousDirection = (TouchL12.Direction)(-1);
			this.position = 1;
			this.previousPosition = 0;
			this.touched = true;
			this.previousTouched = false;

			this.sensor.OnSliderPressed += (sender, state) => { this.touched = state; };
			this.sensor.OnSliderReleased += (sender, state) => { this.touched = state; };
			this.sensor.OnSliderPositionChanged += (sender, position, direction) => { this.position = position; this.direction = direction; };

			this.renderTimer = new GT.Timer(DisplayDriver.MS_PER_FRAME, GT.Timer.BehaviorType.RunContinuously);
			this.renderTimer.Tick += Render;
			this.renderTimer.Start();
		}

		private void Render(GT.Timer timer)
		{
			if (this.position != this.previousPosition || this.touched != this.previousTouched)
			{
				this.display.SimpleGraphics.Clear();
				this.display.SimpleGraphics.DisplayRectangle(GT.Color.Blue, 2, GT.Color.Blue, DisplayDriver.SLIDER_CENTER_X - 6 * DisplayDriver.SLIDER_CAP_WIDTH, DisplayDriver.SLIDER_CENTER_Y, 11 * DisplayDriver.SLIDER_CAP_WIDTH, 5);

				this.DrawCircle(GT.Color.Red, (uint)((this.position - 6) * DisplayDriver.SLIDER_CAP_WIDTH + DisplayDriver.SLIDER_CENTER_X), SLIDER_CENTER_Y + 3, 10);

				this.DrawText(this.position.ToString(), GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(1));
				this.DrawText(this.direction == TouchL12.Direction.Left ? "Left" : "Right", GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(2));
				this.DrawText(this.touched ? "Touched" : "Not Touched", GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(3));

				this.previousDirection = this.direction;
				this.previousPosition = this.position;
				this.previousTouched = this.touched;
			}
		}

		private void DrawCircle(GT.Color color, uint x, uint y, uint radius)
		{
			this.display.SimpleGraphics.DisplayEllipse(color, x, y, radius, radius);
		}

		private void DrawText(string text, GT.Color color, uint x, uint y)
		{
			this.display.SimpleGraphics.DisplayRectangle(GT.Color.Black, 1, GT.Color.Black, x, y, DisplayDriver.TEXT_WIDTH, DisplayDriver.TEXT_HEIGHT);
			this.display.SimpleGraphics.DisplayText(text, Resources.GetFont(Resources.FontResources.small), color, x, y);
		}

		private uint LineToY(uint line)
		{
			return DisplayDriver.TEXT_Y + (DisplayDriver.TEXT_HEIGHT + DisplayDriver.TEXT_SPACING) * (line - 1);
		}
	}
}
