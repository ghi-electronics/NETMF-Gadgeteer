using System;
using System.Collections;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A Tunes module for Microsoft .NET Gadgeteer</summary>
	public class Tunes : GTM.Module {
		private GTI.PwmOutput pwm;
		private Queue playlist;
		private Thread worker;
		private object syncRoot;

		/// <summary>Whether or not there is something being played.</summary>
		public bool IsPlaying {
			get {
				lock (this.playlist)
					return this.playlist.Count != 0;
			}
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Tunes(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('P', this);

			this.pwm = GTI.PwmOutputFactory.Create(socket, Socket.Pin.Nine, false, this);
			this.playlist = new Queue();
			this.syncRoot = new object();
		}

		/// <summary>Plays the given frequency indefinitely.</summary>
		/// <param name="frequency">The frequency to play.</param>
		public void Play(int frequency) {
			this.Play(new Tone(frequency));
		}

		/// <summary>Plays the given tone indefinitely.</summary>
		/// <param name="tone">The tone to play.</param>
		public void Play(Tone tone) {
			this.Play(new MusicNote(tone, Timeout.Infinite));
		}

		/// <summary>Plays the given frequency for the given duration.</summary>
		/// <param name="frequency">The frequency to play.</param>
		/// <param name="duration">How long to play for.</param>
		public void Play(int frequency, int duration) {
			this.Play(new MusicNote(new Tone(frequency), duration));
		}

		/// <summary>Plays the given note.</summary>
		/// <param name="note">The note to play.</param>
		public void Play(MusicNote note) {
			this.Play(new Melody(note));
		}

		/// <summary>Plays the given notes.</summary>
		/// <param name="notes">The notes to play.</param>
		public void Play(params MusicNote[] notes) {
			this.Play(new Melody(notes));
		}

		/// <summary>Plays the melody.</summary>
		/// <param name="melody">The melody to play.</param>
		public void Play(Melody melody) {
			this.Stop();

			foreach (var i in melody.list)
				this.playlist.Enqueue(i);

			this.worker = new Thread(this.DoWork);
			this.worker.Start();
		}

		/// <summary>Adds a note to the playlist.</summary>
		/// <param name="note">The note to be added.</param>
		public void AddNote(MusicNote note) {
			lock (this.syncRoot)
				this.playlist.Enqueue(note);
		}

		/// <summary>Stops playback.</summary>
		public void Stop() {
			if (!this.IsPlaying)
				return;

			lock (this.syncRoot)
				this.playlist.Clear();

			this.worker.Join(250);

			if (this.worker != null && this.worker.IsAlive)
				this.worker.Abort();

			this.pwm.Set(100, 0);
		}

		private void DoWork() {
			MusicNote note = null;

			while (true) {
				lock (this.syncRoot) {
					if (this.playlist.Count == 0)
						break;

					note = (MusicNote)this.playlist.Dequeue();
				}

				if (note.Tone.Frequency != 0.0) {
					this.pwm.IsActive = true;

					this.pwm.Set((int)note.Tone.Frequency, 0.5);
				}
				else {
					this.pwm.IsActive = false;
				}

				Thread.Sleep(note.Duration);
			}

			this.pwm.Set(100.0, 0.0001);
		}
		/// <summary>Represents a list of notes to play in sequence.</summary>
		public class Melody {
			internal Queue list;

			/// <summary>Constructs a new instance.</summary>
			public Melody() {
				this.list = new Queue();
			}

			/// <summary>Constructs a new instance.</summary>
			/// <param name="notes">The list of notes to add to the melody.</param>
			public Melody(params MusicNote[] notes)
				: this() {
				foreach (MusicNote i in notes)
					this.Add(i);
			}

			/// <summary>Adds a new note to the list to play.</summary>
			/// <param name="frequency">The frequency of the note.</param>
			/// <param name="duration">The duration of the note in milliseconds.</param>
			public void Add(int frequency, int duration) {
				if (frequency < 0) throw new ArgumentOutOfRangeException("frequency", "frequency must be non-negative.");
				if (duration < 1) throw new ArgumentOutOfRangeException("duration", "duration must be positive.");

				this.Add(new Tone(frequency), duration);
			}

			/// <summary>Adds a new note to the list to play.</summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration of the note.</param>
			public void Add(Tone tone, int duration) {
				if (duration < 1) throw new ArgumentOutOfRangeException("duration", "duration must be positive.");

				this.Add(new MusicNote(tone, duration));
			}

			/// <summary>Adds an existing note to the list to play.</summary>
			/// <param name="note">The note to add.</param>
			public void Add(MusicNote note) {
				this.list.Enqueue(note);
			}

			/// <summary>Adds notes to the list to play.</summary>
			/// <param name="notes">The list of notes to add to the melody.</param>
			public void Add(params MusicNote[] notes) {
				foreach (MusicNote i in notes)
					this.Add(i);
			}
		}

		/// <summary>Class that holds and manages notes that can be played.</summary>
		public class Tone {

			/// <summary>A rest note</summary>
			public static readonly Tone Rest = new Tone(0.0);

			/// <summary>C in the 2nd octave.</summary>
			public static readonly Tone C2 = new Tone(65.41);

			/// <summary>D in the 2nd octave.</summary>
			public static readonly Tone D2 = new Tone(73.42);

			/// <summary>E in the 2nd octave.</summary>
			public static readonly Tone E2 = new Tone(82.41);

			/// <summary>F in the 2nd octave.</summary>
			public static readonly Tone F2 = new Tone(87.31);

			/// <summary>G in the 2nd octave.</summary>
			public static readonly Tone G2 = new Tone(98.00);

			/// <summary>A in the 2nd octave.</summary>
			public static readonly Tone A2 = new Tone(110.00);

			/// <summary>B in the 2nd octave.</summary>
			public static readonly Tone B2 = new Tone(123.47);

			/// <summary>C in the 3rd octave.</summary>
			public static readonly Tone C3 = new Tone(130.81);

			/// <summary>D in the 3rd octave.</summary>
			public static readonly Tone D3 = new Tone(146.83);

			/// <summary>E in the 3rd octave.</summary>
			public static readonly Tone E3 = new Tone(164.81);

			/// <summary>F in the 3rd octave.</summary>
			public static readonly Tone F3 = new Tone(174.61);

			/// <summary>G in the 3rd octave.</summary>
			public static readonly Tone G3 = new Tone(196.00);

			/// <summary>A in the 3rd octave.</summary>
			public static readonly Tone A3 = new Tone(220.00);

			/// <summary>B in the 3rd octave.</summary>
			public static readonly Tone B3 = new Tone(246.94);

			/// <summary>C in the 4th octave.</summary>
			public static readonly Tone C4 = new Tone(261.626);

			/// <summary>D in the 4th octave.</summary>
			public static readonly Tone D4 = new Tone(293.665);

			/// <summary>E in the 4th octave.</summary>
			public static readonly Tone E4 = new Tone(329.628);

			/// <summary>F in the 4th octave.</summary>
			public static readonly Tone F4 = new Tone(349.228);

			/// <summary>G in the 4th octave.</summary>
			public static readonly Tone G4 = new Tone(391.995);

			/// <summary>A in the 4th octave.</summary>
			public static readonly Tone A4 = new Tone(440);

			/// <summary>B in the 4th octave.</summary>
			public static readonly Tone B4 = new Tone(493.883);

			/// <summary>C in the 5th octave.</summary>
			public static readonly Tone C5 = new Tone(523.251);

			/// <summary>D in the 5th octave.</summary>
			public static readonly Tone D5 = new Tone(587.33);

			/// <summary>E in the 5th octave.</summary>
			public static readonly Tone E5 = new Tone(659.25);

			/// <summary>F in the 5th octave.</summary>
			public static readonly Tone F5 = new Tone(698.46);

			/// <summary>G in the 5th octave.</summary>
			public static readonly Tone G5 = new Tone(783.99);

			/// <summary>A in the 5th octave.</summary>
			public static readonly Tone A5 = new Tone(880.00);

			/// <summary>B in the 5th octave.</summary>
			public static readonly Tone B5 = new Tone(987.77);

			/// <summary>C in the 6th octave.</summary>
			public static readonly Tone C6 = new Tone(1046.50);

			/// <summary>Frequency of the note in hertz</summary>
			public double Frequency { get; set; }

			/// <summary>Constructs a new instance.</summary>
			/// <param name="frequency">The frequency of the tone.</param>
			public Tone(double frequency) {
				if (frequency < 0) throw new ArgumentOutOfRangeException("frequency", "frequency must be non-negative.");

				this.Frequency = frequency;
			}
		}

		/// <summary>Class that describes a musical note, containing a tone and a duration.</summary>
		public class MusicNote {

			/// <summary>The tone of the note.</summary>
			public Tone Tone { get; set; }

			/// <summary>The duration of the note.</summary>
			public int Duration { get; set; }

			/// <summary>Constructs a new instance.</summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration that the note should be played in milliseconds.</param>
			public MusicNote(Tone tone, int duration) {
				if (duration < 1 && duration != Timeout.Infinite) throw new ArgumentOutOfRangeException("duration", "duration must be positive.");

				this.Tone = tone;
				this.Duration = duration;
			}
		}
	}
}