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
			DisplayDriver driver = new DisplayDriver(touchC8, display_T35);
		}
	}
	
	public class DisplayDriver
	{
		private const int FPS = 10;
		private const int MS_PER_FRAME = 1000 / DisplayDriver.FPS;

		private const uint WHEEL_DOT_RADIUS = 5;
		private const uint WHEEL_CENTER_X = 175;
		private const uint WHEEL_CENTER_Y = 120;
		private const uint WHEEL_RADIUS = 50;

		private const uint BUTTON_CENTER_X = 15;
		private const uint BUTTON_CENTER_Y = 120;
		private const uint BUTTON_SPACING_X = 0;
		private const uint BUTTON_SPACING_Y = 45;
		private const uint BUTTON_RADIUS = 15;

		private const uint TEXT_X = 0;
		private const uint TEXT_Y = 0;
		private const uint TEXT_WIDTH = 150;
		private const uint TEXT_HEIGHT = 12;
		private const uint TEXT_SPACING = 2;

		private Display_T35 display;
		private TouchC8 sensor;
		private GT.Timer renderTimer;
		private GT.Timer tickTimer;

		private TouchC8.Direction direction;
		private double count;
		private double position;
		private bool wheelTouched;
		private bool proximity;
		private bool buttonATouched;
		private bool buttonBTouched;
		private bool buttonCTouched;

		private TouchC8.Direction previousDirection;
		private double previousCount;
		private double previousPosition;
		private bool previousWheelTouched;
		private bool previousProximity;

		public DisplayDriver(TouchC8 sensor, Display_T35 display)
		{
			this.display = display;
			this.sensor = sensor;

			this.direction = (TouchC8.Direction)(-1);
			this.previousDirection = (TouchC8.Direction)(-1);
			this.position = 0;
			this.previousPosition = 0;
			this.wheelTouched = false;
			this.previousWheelTouched = false;
			this.proximity = false;
			this.previousProximity = false;
			this.buttonATouched = false;
			this.buttonBTouched = false;
			this.buttonCTouched = false;

			this.sensor.OnProximityEnter += (sender, state) => { this.proximity = state; };
			this.sensor.OnProximityExit += (sender, state) => { this.proximity = state; };
			this.sensor.OnButtonPressed += (sender, button, state) => { switch (button) { case TouchC8.Buttons.Up: this.buttonATouched = state; break; case TouchC8.Buttons.Middle: this.buttonBTouched = state; break; case TouchC8.Buttons.Down: this.buttonCTouched = state; break; }; };
			this.sensor.OnButtonReleased += (sender, button, state) => { switch (button) { case TouchC8.Buttons.Up: this.buttonATouched = state; break; case TouchC8.Buttons.Middle: this.buttonBTouched = state; break; case TouchC8.Buttons.Down: this.buttonCTouched = state; break; }; };
			this.sensor.OnWheelPressed += (sender, state) => { this.wheelTouched = state; };
			this.sensor.OnWheelReleased += (sender, state) => { this.wheelTouched = state; };
			this.sensor.OnWheelPositionChanged += (sender, position, direction) => { this.position = position; this.direction = direction; };

			this.renderTimer = new GT.Timer(DisplayDriver.MS_PER_FRAME, GT.Timer.BehaviorType.RunContinuously);
			this.renderTimer.Tick += Render;
			this.renderTimer.Start();

			double lastPosition = 0;
			this.count = 0;
			this.previousCount = 0;
			this.tickTimer = new GT.Timer(5, GT.Timer.BehaviorType.RunContinuously);
			this.tickTimer.Tick += (timer) =>
			{
				this.count += System.Math.Abs(this.position - lastPosition) * (this.direction == TouchC8.Direction.Counterclockwise ? -1 : 1);
				lastPosition = this.position;
			};
			//this.tickTimer.Start();
		}

		private void Render(GT.Timer timer)
		{
			if (this.count != this.previousCount)
			{
				this.DrawText(this.count.ToString(), GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(4));
				this.previousCount = this.count;
			}

			if (this.direction != this.previousDirection)
			{
				this.DrawText(this.direction == TouchC8.Direction.Clockwise ? "Clockwise" : "Counter-Clockwise", GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(1));
				this.previousDirection = this.direction;
			}

			if (this.proximity != this.previousProximity)
			{
				this.DrawText(this.proximity ? "Proximity detected" : "Proximity not detected", GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(3));
				this.previousProximity = this.proximity;
			}

			if (this.position != this.previousPosition || this.wheelTouched != this.previousWheelTouched)
			{
				double x = 0;
				double y = 0;

				this.GetWheelCoordinate(this.previousPosition, out x, out y);
				this.DrawCircle(GT.Color.Black, (uint)x, (uint)y, DisplayDriver.WHEEL_DOT_RADIUS);

				this.DrawCircle(GT.Color.White, DisplayDriver.WHEEL_CENTER_X, DisplayDriver.WHEEL_CENTER_Y, DisplayDriver.WHEEL_RADIUS);

				this.GetWheelCoordinate(this.position, out x, out y);
				this.DrawCircle(this.wheelTouched ? GT.Color.Blue : GT.Color.Red, (uint)x, (uint)y, DisplayDriver.WHEEL_DOT_RADIUS);

				this.DrawText(((int)this.position).ToString(), GT.Color.White, DisplayDriver.TEXT_X, this.LineToY(2));

				this.previousPosition = this.position;
				this.previousWheelTouched = this.wheelTouched;
			}

			this.DrawCircle(this.buttonATouched ? GT.Color.Blue : GT.Color.Red, DisplayDriver.BUTTON_CENTER_X - DisplayDriver.BUTTON_SPACING_X, DisplayDriver.BUTTON_CENTER_Y - DisplayDriver.BUTTON_SPACING_Y, DisplayDriver.BUTTON_RADIUS);
			this.DrawCircle(this.buttonBTouched ? GT.Color.Blue : GT.Color.Red, DisplayDriver.BUTTON_CENTER_X, DisplayDriver.BUTTON_CENTER_Y, DisplayDriver.BUTTON_RADIUS);
			this.DrawCircle(this.buttonCTouched ? GT.Color.Blue : GT.Color.Red, DisplayDriver.BUTTON_CENTER_X + DisplayDriver.BUTTON_SPACING_X, DisplayDriver.BUTTON_CENTER_Y + DisplayDriver.BUTTON_SPACING_Y, DisplayDriver.BUTTON_RADIUS);
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

		private void GetWheelCoordinate(double position, out double x, out double y)
		{
			double radians = (position - 90) * (System.Math.PI / 180);
			x = DisplayDriver.WHEEL_RADIUS * System.Math.Cos(radians) + DisplayDriver.WHEEL_CENTER_X;
			y = DisplayDriver.WHEEL_RADIUS * System.Math.Sin(radians) + DisplayDriver.WHEEL_CENTER_Y;
		}
	}
}
