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
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Tunes.MusicNote note = new Tunes.MusicNote(Tunes.Tone.C4, 400);

            tunes.AddNote(note);

            // up
            PlayNote(Tunes.Tone.C4);
            PlayNote(Tunes.Tone.D4);
            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.F4);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.A4);
            PlayNote(Tunes.Tone.B4);
            PlayNote(Tunes.Tone.C5);

            // back down
            PlayNote(Tunes.Tone.B4);
            PlayNote(Tunes.Tone.A4);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.F4);
            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.D4);
            PlayNote(Tunes.Tone.C4);

            // arpeggio
            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.C5);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.C4);
            
            tunes.Play();

            Thread.Sleep(100);

            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.C5);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.C4);

            tunes.Play();

			Thread.Sleep(5000);

			Tunes.Melody melody = new Tunes.Melody();

			// up
			melody.Add(Tunes.Tone.C4, 200);
			melody.Add(Tunes.Tone.D4, 200);
			melody.Add(Tunes.Tone.E4, 200);
			melody.Add(Tunes.Tone.F4, 200);
			melody.Add(Tunes.Tone.G4, 200);
			melody.Add(Tunes.Tone.A4, 200);
			melody.Add(Tunes.Tone.B4, 200);
			melody.Add(Tunes.Tone.C5, 200);
			melody.Add(Tunes.Tone.B4, 200);
			melody.Add(Tunes.Tone.A4, 200);
			melody.Add(Tunes.Tone.G4, 200);
			melody.Add(Tunes.Tone.F4, 200);
			melody.Add(Tunes.Tone.E4, 200);
			melody.Add(Tunes.Tone.D4, 200);
			melody.Add(Tunes.Tone.C4, 200);
			melody.Add(Tunes.Tone.E4, 200);
			melody.Add(Tunes.Tone.G4, 200);
			melody.Add(Tunes.Tone.C5, 200);
			melody.Add(Tunes.Tone.G4, 200);
			melody.Add(Tunes.Tone.E4, 200);
			melody.Add(Tunes.Tone.C4, 200);

			tunes.Play(melody);
        }

        void PlayNote(Tunes.Tone tone)
        {
            Tunes.MusicNote note = new Tunes.MusicNote(tone, 200);

            tunes.AddNote(note);
        }
    }
}
