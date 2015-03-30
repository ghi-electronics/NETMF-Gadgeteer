using Microsoft.SPOT;
using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A MulticolorLED module for Microsoft .NET Gadgeteer</summary>
	public class MulticolorLED : GTM.DaisyLinkModule {
		private const byte GHI_DAISYLINK_MANUFACTURER = 0x10;

		private const byte GHI_DAISYLINK_TYPE = 0x01;

		private const byte GHI_DAISYLINK_VERSION = 0x01;

		private TimeSpan oneSecond;

		private AnimationFinishedEventHandler onAnimationFinished;

		/// <summary>The delegate that is used to handle the animation finished event.</summary>
		/// <param name="sender">The MulticolorLED object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void AnimationFinishedEventHandler(MulticolorLED sender, EventArgs e);

		/// <summary>Raised when the BlinkOnce or FadeOnce animation has finished.</summary>
		public event AnimationFinishedEventHandler AnimationFinished;

		/// <summary>Whether or not the green and blue channels should be swapped.</summary>
		public bool GreenBlueSwapped { get; set; }

		private enum Registers : byte {
			R = 0,
			G = 1,
			B = 2,
			Configuration = 3,
			ResetTimers = 4,
			Color1 = 5,
		}

		private enum Mode : byte {
			Off = 0,
			Constant = 1,
			BlinkOnce = 2,
			BlinkRepeatedly = 3,
			FadeOnce = 4,
			FadeRepeatedly = 5,
			BlinkOnceInt = 6,
			BlinkRepeatedlyInt = 7,
			FadeOnceInt = 8,
			FadeRepeatedlyInt = 9,
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The mainboard socket that has the multi-color LED plugged into it.</param>
		public MulticolorLED(int socketNumber)
			: base(socketNumber, MulticolorLED.GHI_DAISYLINK_MANUFACTURER, MulticolorLED.GHI_DAISYLINK_TYPE, MulticolorLED.GHI_DAISYLINK_VERSION, MulticolorLED.GHI_DAISYLINK_VERSION, 50, "MulticolorLED") {
			this.DaisyLinkInterrupt += (a) => this.OnAnimationFinished(this, null);
			this.oneSecond = new TimeSpan(0, 0, 1);

			this.TurnOff();
		}

		/// <summary>Returns an array of MulticolorLEDs for each hardware module that is physically connected to the specified socket.</summary>
		/// <param name="socketNumber">The socket to get the objects for.</param>
		/// <returns>The array of MulticolorLEDs.</returns>
		public static MulticolorLED[] GetAll(int socketNumber) {
			int chainLength = DaisyLinkModule.GetLengthOfChain(socketNumber);
			if (chainLength == 0)
				return new MulticolorLED[0];

			MulticolorLED[] leds = new MulticolorLED[chainLength];
			for (int i = 0; i < leds.Length; i++)
				leds[i] = new MulticolorLED(socketNumber);

			return leds;
		}

		/// <summary>Changes the color of the LED to blue, stopping a blink or fade if one is in progress.</summary>
		public void TurnBlue() {
			this.SendCommand(Color.Blue, Mode.Constant);
		}

		/// <summary>Changes the color of the LED to red, stopping a blink or fade if one is in progress.</summary>
		public void TurnRed() {
			this.SendCommand(Color.Red, Mode.Constant);
		}

		/// <summary>Changes the color of the LED to green, stopping a blink or fade if one is in progress.</summary>
		public void TurnGreen() {
			this.SendCommand(Color.Green, Mode.Constant);
		}

		/// <summary>Turns the LED off.</summary>
		public void TurnOff() {
			this.SendCommand(Color.Black, Mode.Constant);
		}

		/// <summary>Changes the color of the LED to white, stopping a blink or fade if one is in progress.</summary>
		public void TurnWhite() {
			this.SendCommand(Color.White, Mode.Constant);
		}

		/// <summary>Changes the color of the LED to the specified color, stopping a blink or fade if one is in progress.</summary>
		/// <param name="color">The color to change the LED to.</param>
		public void TurnColor(Color color) {
			this.SendCommand(color, Mode.Constant);
		}

		/// <summary>Sets the red component of the current color (if there is a two-color animation, this affects the first color).</summary>
		/// <param name="intensity">The amount to set for the red intensity, 0 (no red) to 255 (full red).</param>
		public void SetRedIntensity(byte intensity) {
			Color currentColor = this.GetCurrentColor();
			currentColor.R = intensity;
			this.SendCommand(currentColor);
		}

		/// <summary>Sets the red component of the current color (if there is a two-color animation, this affects the first color)</summary>
		/// <param name="intensity">The amount to set for the red intensity.</param>
		public void SetRedIntensity(int intensity) {
			if (intensity < 0) intensity = 0;
			if (intensity > 255) intensity = 255;

			this.SetRedIntensity((byte)intensity);
		}

		/// <summary>Sets the green component of the current color (if there is a two-color animation, this affects the first color)</summary>
		/// <param name="intensity">The amount to set for the green intensity, 0 (no green) to 255 (full green).</param>
		public void SetGreenIntensity(byte intensity) {
			Color currentColor = this.GetCurrentColor();
			currentColor.G = intensity;
			this.SendCommand(currentColor);
		}

		/// <summary>Sets the green component of the current color (if there is a two-color animation, this affects the first color)</summary>
		/// <param name="intensity">The amount to set for the green intensity.</param>
		public void SetGreenIntensity(int intensity) {
			if (intensity < 0) intensity = 0;
			if (intensity > 255) intensity = 255;

			this.SetGreenIntensity((byte)intensity);
		}

		/// <summary>Sets the blue component of the current color (if there is a two-color animation, this affects the first color)</summary>
		/// <param name="intensity">The amount to set for the blue intensity, 0 (no blue) to 255 (full blue).</param>
		public void SetBlueIntensity(byte intensity) {
			Color currentColor = this.GetCurrentColor();
			currentColor.B = intensity;
			this.SendCommand(currentColor);
		}

		/// <summary>Sets the blue component of the current color (if there is a two-color animation, this affects the first color)</summary>
		/// <param name="intensity">The amount to set for the blue intensity.</param>
		public void SetBlueIntensity(int intensity) {
			if (intensity < 0) intensity = 0;
			if (intensity > 255) intensity = 255;

			this.SetBlueIntensity((byte)intensity);
		}

		/// <summary>Adds a full red component to the current color (if there is a two-color animation, this affects the first color).</summary>
		public void AddRed() {
			Color currentColor = this.GetCurrentColor();
			currentColor.R = 255;
			this.SendCommand(currentColor);
		}

		/// <summary>Removes all of the red component from the current color (if there is a two-color animation, this affects the first color).</summary>
		public void RemoveRed() {
			Color currentColor = this.GetCurrentColor();
			currentColor.R = 0;
			this.SendCommand(currentColor);
		}

		/// <summary>Adds a full green component to the current color (if there is a two-color animation, this affects the first color).</summary>
		public void AddGreen() {
			Color currentColor = this.GetCurrentColor();
			currentColor.G = 255;
			this.SendCommand(currentColor);
		}

		/// <summary>Removes all of the green component from the current color (if there is a two-color animation, this affects the first color).</summary>
		public void RemoveGreen() {
			Color currentColor = this.GetCurrentColor();
			currentColor.G = 0;
			this.SendCommand(currentColor);
		}

		/// <summary>Adds a full blue component to the current color (if there is a two-color animation, this affects the first color).</summary>
		public void AddBlue() {
			Color currentColor = this.GetCurrentColor();
			currentColor.B = 255;
			this.SendCommand(currentColor);
		}

		/// <summary>Removes all of the blue component from the current color (if there is a two-color animation, this affects the first color).</summary>
		public void RemoveBlue() {
			Color currentColor = this.GetCurrentColor();
			currentColor.B = 0;
			this.SendCommand(currentColor);
		}

		/// <summary>Gets the current LED color.</summary>
		/// <returns>The current color.</returns>
		public Color GetCurrentColor() {
			byte c1 = this.Read((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Color1));
			byte c2 = this.Read((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Color1 + 1));
			byte c3 = this.Read((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Color1 + 2));

			return this.Swap(new Color(c1, c2, c3));
		}

		/// <summary>Causes the LED to light in the specified color for one second, and then turn off.</summary>
		/// <param name="color">The color to display.</param>
		public void BlinkOnce(Color color) {
			this.SendCommand(color, this.oneSecond, Color.Black, TimeSpan.Zero, Mode.BlinkOnceInt);
		}

		/// <summary>Causes the LED to light in the specified color for the specified duration, and then turn off.</summary>
		/// <param name="color">The color to display.</param>
		/// <param name="blinkTime">The duration before the LED turns off.</param>
		public void BlinkOnce(Color color, TimeSpan blinkTime) {
			this.SendCommand(color, blinkTime, Color.Black, TimeSpan.Zero, Mode.BlinkOnceInt);
		}

		/// <summary>Causes the LED to light in the specified color for the specified duration, and then switch to another color.</summary>
		/// <param name="blinkColor">The color to display until <paramref name="blinkTime" /> elapses.</param>
		/// <param name="blinkTime">The duration before the LED changes from <paramref name="blinkColor" /> to <paramref name="endColor" />.</param>
		/// <param name="endColor">The color to switch to when <paramref name="blinkTime" /> elapses.</param>
		public void BlinkOnce(Color blinkColor, TimeSpan blinkTime, Color endColor) {
			this.SendCommand(blinkColor, blinkTime, endColor, TimeSpan.Zero, Mode.BlinkOnceInt);
		}

		/// <summary>Causes the LED to light in the specified color for one second, turn off, and repeat.</summary>
		/// <param name="color">The color to display.</param>
		public void BlinkRepeatedly(Color color) {
			this.SendCommand(color, this.oneSecond, Color.Black, oneSecond, Mode.BlinkRepeatedly);
		}

		/// <summary>Causes the LED to light in the specified color for the specified duration, switch to the second color for another specified duration, and repeat.</summary>
		/// <param name="color1">The color used for the first part of the blink.</param>
		/// <param name="blinkTime1">The duration before the LED changes from <paramref name="color1" /> to <paramref name="color2" />.</param>
		/// <param name="color2">The color used for the second part of the blink.</param>
		/// <param name="blinkTime2">The duration before the LED changes from <paramref name="color2" /> back to <paramref name="color1" />.</param>
		public void BlinkRepeatedly(Color color1, TimeSpan blinkTime1, Color color2, TimeSpan blinkTime2) {
			this.SendCommand(color1, blinkTime1, color2, blinkTime2, Mode.BlinkRepeatedly);
		}

		/// <summary>Causes the LED to light in the specified color, and then fade to black (off) in one second.</summary>
		/// <param name="color">The color to begin the fade with.</param>
		/// <remarks>The default fade time for this method is one second.</remarks>
		public void FadeOnce(Color color) {
			this.SendCommand(color, this.oneSecond, Color.Black, TimeSpan.Zero, Mode.FadeOnceInt);
		}

		/// <summary>Causes the LED to light in the specified color, and then fade to black (off) in the specified duration.</summary>
		/// <param name="color">The color to begin the fade with.</param>
		/// <param name="fadeTime">The duration of the fade.</param>
		public void FadeOnce(Color color, TimeSpan fadeTime) {
			this.SendCommand(color, fadeTime, Color.Black, TimeSpan.Zero, Mode.FadeOnceInt);
		}

		/// <summary>Causes the LED to light in the specified color, and then fade to another color in the specified duration.</summary>
		/// <param name="fromColor">The color to begin the fade with.</param>
		/// <param name="fadeTime">The duration of the fade.</param>
		/// <param name="toColor">The color to end the fade with.</param>
		public void FadeOnce(Color fromColor, TimeSpan fadeTime, Color toColor) {
			this.SendCommand(fromColor, fadeTime, toColor, TimeSpan.Zero, Mode.FadeOnceInt);
		}

		/// <summary>Causes the LED to light in the specified color, fade to black (off) in one second, and repeat.</summary>
		/// <param name="color">The color to begin the fade with.</param>
		public void FadeRepeatedly(Color color) {
			this.SendCommand(color, this.oneSecond, Color.Black, this.oneSecond, Mode.FadeRepeatedly);
		}

		/// <summary>Cause the LED to repeatedly fade back and forth between two colors</summary>
		/// <param name="color1">The color to begin the fade with.</param>
		/// <param name="fadeTime1">The duration of the fade from <paramref name="color1" /> to <paramref name="color2" />.</param>
		/// <param name="color2">The color of the second part of the fade.</param>
		/// <param name="fadeTime2">The duration of the fade from <paramref name="color2" /> to <paramref name="color1" />.</param>
		public void FadeRepeatedly(Color color1, TimeSpan fadeTime1, Color color2, TimeSpan fadeTime2) {
			SendCommand(color1, fadeTime1, color2, fadeTime2, Mode.FadeRepeatedly);
		}

		private void SendCommand(Color color1, TimeSpan blinkTime1, Color color2, TimeSpan blinkTime2, Mode mode) {
			var time1 = blinkTime1.Ticks / 1000;
			var time2 = blinkTime2.Ticks / 1000;

			color1 = this.Swap(color1);
			color2 = this.Swap(color2);

			this.Write((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Configuration), (byte)Mode.Off, 0x00,
						color1.R, color1.G, color1.B,
						color2.R, color2.G, color2.B,
						(byte)(time1 >> 0), (byte)(time1 >> 8), (byte)(time1 >> 16), (byte)(time1 >> 24),
						(byte)(time2 >> 0), (byte)(time2 >> 8), (byte)(time2 >> 16), (byte)(time2 >> 24));

			this.Write((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Configuration), (byte)mode, 0x1);
		}

		private void SendCommand(Color color, Mode mode) {
			color = this.Swap(color);

			this.Write((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Configuration), (byte)Mode.Off, 0x0, color.R, color.G, color.B);

			this.Write((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Configuration), (byte)mode, 0x1);
		}

		private void SendCommand(Color color) {
			color = this.Swap(color);

			if ((Mode)this.Read((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Configuration)) == Mode.Off) {
				this.Write((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Configuration), (byte)Mode.Constant, 0x1, color.R, color.G, color.B);
			}
			else {
				this.Write((byte)(DaisyLinkModule.DaisyLinkOffset + Registers.Color1), color.R, color.G, color.B);
			}
		}

		private Color Swap(Color color) {
			if (this.GreenBlueSwapped) {
				var temp = color.G;

				color.G = color.B;
				color.B = temp;
			}

			return color;
		}

		private void OnAnimationFinished(MulticolorLED sender, EventArgs e) {
			if (this.onAnimationFinished == null)
				this.onAnimationFinished = this.OnAnimationFinished;

			if (Program.CheckAndInvoke(this.AnimationFinished, this.onAnimationFinished, sender, e))
				this.AnimationFinished(sender, e);
		}
	}
}