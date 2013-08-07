using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using System.Collections;

using System.Threading;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A Tunes module for Microsoft .NET Gadgeteer
	/// </summary>
	public class Tunes : GTM.Module
	{
		private GT.Interfaces.PWMOutput tunePWM;
		private Melody playList;
		private bool running = false;
		private Thread playbackThread;

		/// <summary>
		/// Represents a list of notes to play in sequence.
		/// </summary>
		public class Melody
		{
			private Queue list;

			/// <summary>
			/// Creates a new instance of a melody.
			/// </summary>
			public Melody()
			{
				this.list = new Queue();
			}

			/// <summary>
			/// Adds a new note to the list to play.
			/// </summary>
			/// <param name="frequency">The frequency of the note.</param>
			/// <param name="milliseconds">The duration of the note.</param>
			public void Add(int frequency, int milliseconds)
			{
				this.Add(new Tone(frequency), milliseconds);
			}

			/// <summary>
			/// Adds a new note to the list to play.
			/// </summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration of the note.</param>
			public void Add(Tone tone, int duration)
			{
				this.Add(new MusicNote(tone, duration));
			}

			/// <summary>
			/// Adds an existing note to the list to play.
			/// </summary>
			/// <param name="note">The note to add.</param>
			public void Add(MusicNote note)
			{
				this.list.Enqueue(note);
			}

			/// <summary>
			/// Gets the next note to play from the melody.
			/// </summary>
			/// <returns></returns>
			public MusicNote GetNextNote()
			{
				if (this.list.Count == 0)
					throw new Exception("No notes added.");

				return (MusicNote)this.list.Dequeue();
			}

			/// <summary>
			/// Gets the number of notes left to play in the melody.
			/// </summary>
			public int NotesRemaining
			{
				get
				{
					return this.list.Count;
				}
			}

			/// <summary>
			/// Removes all notes from the melody.
			/// </summary>
			public void Clear()
			{
				this.list.Clear();
			}
		}

		/// <summary>
		/// Class that holds and manages notes that can be played.
		/// </summary>
		public class Tone
		{
			/// <summary>
			/// Frequency of the note in hertz
			/// </summary>
			public double freq;

			/// <summary>
			/// Constructs a new Tone.
			/// </summary>
			/// <param name="freq">The frequency of the tone.</param>
			public Tone(double freq)
			{
				this.freq = freq;
			}

			/// <summary>
			/// A "rest" note, or a silent note.
			/// </summary>
			public static readonly Tone Rest = new Tone(0.0);

			#region 4th Octave
			/// <summary>
			/// C in the 4th octave. Middle C.
			/// </summary>
			public static readonly Tone C4 = new Tone(261.626);

			/// <summary>
			/// D in the 4th octave.
			/// </summary>
			public static readonly Tone D4 = new Tone(293.665);

			/// <summary>
			/// E in the 4th octave.
			/// </summary>
			public static readonly Tone E4 = new Tone(329.628);

			/// <summary>
			/// F in the 4th octave.
			/// </summary>
			public static readonly Tone F4 = new Tone(349.228);

			/// <summary>
			/// G in the 4th octave.
			/// </summary>
			public static readonly Tone G4 = new Tone(391.995);

			/// <summary>
			/// A in the 4th octave.
			/// </summary>
			public static readonly Tone A4 = new Tone(440);

			/// <summary>
			/// B in the 4th octave.
			/// </summary>
			public static readonly Tone B4 = new Tone(493.883);

			#endregion 4th Octave

			#region 5th Octave

			/// <summary>
			/// C in the 5th octave.
			/// </summary>
			public static readonly Tone C5 = new Tone(523.251);

			#endregion 5th Octave
		}

		/// <summary>
		/// Class that describes a musical note, containing a tone and a duration.
		/// </summary>
		public class MusicNote
		{
			/// <summary>
			/// The tone of the note.
			/// </summary>
			public Tone tone;
			/// <summary>
			/// The duration of the note.
			/// </summary>
			public int duration;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration that the note should be played.</param>
			public MusicNote(Tone tone, int duration)
			{
				this.tone = tone;
				this.duration = duration;
			}
		}

		/// <summary></summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Tunes(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			socket.EnsureTypeIsSupported('P', this);

			tunePWM = new GTI.PWMOutput(socket, Socket.Pin.Nine, false, this);

			playList = new Melody();
		}

		/// <summary>
		/// Plays the given frequency indefinitely.
		/// </summary>
		/// <param name="frequency">The frequency to play.</param>
		public void Play(int frequency)
		{
			this.playList.Clear();
			this.playList.Add(frequency, int.MaxValue);
			this.Play();
		}

		/// <summary>
		/// Plays the given tone indefinitely.
		/// </summary>
		/// <param name="tone">The tone to play.</param>
		public void Play(Tone tone)
		{
			this.Play((int)tone.freq);
		}

		/// <summary>
		/// Plays the melody.
		/// </summary>
		/// <param name="melody">The melody to play.</param>
		public void Play(Melody melody)
		{
			this.playList = melody;
			this.Play();
		}

		/// <summary>
		/// Starts note playback of the notes added using AddNote(). Returns if it made any change.
		/// </summary>
		/// <returns>Returns true if notes were not playing and they were started. False if notes were already being played.</returns>
		public bool Play()
		{
            if (this.running)
                this.Stop();

			// Make sure the queue is not empty and we are not currently playing it.
			if (playList.NotesRemaining > 0)
			{
				this.running = true;

				playbackThread = new Thread(PlaybackThread);
				playbackThread.Start();
			}

			return true;
		}

		/// <summary>
		/// The function that runs when the playback thread is started. Returns (ends the thread) when playback is finished or Stop() is called.
		/// </summary>
		private void PlaybackThread()
		{
			while (this.running && playList.NotesRemaining > 0)
			{
				// Get the next note.
				MusicNote currNote = playList.GetNextNote();

				// Set the tone and sleep for the duration
				SetTone(currNote.tone);

				Thread.Sleep(currNote.duration);
			}

			SetTone(Tone.Rest);

			this.running = false;
		}

		/// <summary>
		///  Sets PWM to the tone frequency and starts it.
		/// </summary>
		/// <param name="tone"></param>
		private void SetTone(Tone tone)
		{
			tunePWM.Active = false;

			if (tone.freq == 0)
			{
				tunePWM.Active = false;
				return;
			}

			tunePWM.Set((int)tone.freq, 0.5);
			tunePWM.Active = true;
		}

		/// <summary>
		/// Stops note playback. Returns if it made any change.
		/// </summary>
		public void Stop()
		{
            this.playbackThread.Abort();
            this.running = false;
            this.tunePWM.Active = false;
		}

		/// <summary>
		/// Adds a note to the queue to be played
		/// </summary>
		/// <param name="note">The note to be added, which describes the tone and duration to be played.</param>
		public void AddNote(MusicNote note)
		{
			playList.Add(note);
		}
	}
}
