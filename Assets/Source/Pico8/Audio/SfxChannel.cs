using PicoUnity.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PicoUnity
{
    internal class SfxChannel
    {
        private int       sfxPointer = -1;
        private int       sfxOffset;
        private double    time;
        private float     noteLength;
        private AudioNote note;
        private AudioNote lastNote;
        private int       loopStart;
        private int       loopEnd;
        private int       length;

        private MemoryModule mem;

        private SfxInstrument[] instruments;

        public bool IsPlaying => sfxPointer > 0;
        public bool Looping { get; set; }

        public SfxChannel(MemoryModule mem)
        {
            this.mem = mem;

            instruments = new SfxInstrument[8];

            instruments[0] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Triangle};
            instruments[1] = new SfxInstrument() { Waveform = AudioSynth.Waveform.TiltedTriangle};
            instruments[2] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Sawtooth};
            instruments[3] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Square};
            instruments[4] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Pulse};
            instruments[5] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Organ};
            instruments[6] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Noise};
            instruments[7] = new SfxInstrument() { Waveform = AudioSynth.Waveform.Phaser};

        }

        public void Sfx(int n, int offset = 0, int length = 0)
        {
            if (n == -2)
            {
                Looping = false;
                return;
            }

            for (int i = 0; i < instruments.Length; i++)
            {
                instruments[i].Stop();
            }

            if (n == -1)
            {
                sfxPointer = -1;
                return;
            }

            sfxPointer = MemoryModule.ADDR_SOUND + 68 * n;
            sfxOffset = offset * 2;
            time = 0;
            noteLength = mem.Peek(sfxPointer + 65) / 128f;
            loopStart = mem.Peek(sfxPointer + 66);
            loopEnd = mem.Peek(sfxPointer + 67);

            Debug.Log($"Loop: {loopStart} - {loopEnd}");

            lastNote = note = AudioNote.Default;

            Looping = loopStart != loopEnd;

            this.length = length;

            if (note.isCustom) Debug.LogWarning("Sfx: custom instruments are not supported");

            PullNode();
        }

        private void PullNode()
        {
            lastNote = note;

            int noteAddr = sfxPointer + sfxOffset;
            note = GetNote(noteAddr); 

            for (int i = 0; i < instruments.Length; i++)
            {
                var waveform = (AudioSynth.Waveform)note.waveform;

                if (instruments[i].Waveform == waveform)
                {
                    //TODO: Effects like arpeggio have to be persistent and don't reset after each note
                    SfxEffect effect = null;
                    switch (note.effect)
                    {
                        case 1: //slide
                            effect = new SfxEffectLerp(lastNote.hz, note.hz, lastNote.Volume01, note.Volume01, noteLength);
                            break;
                        case 2: //vibrato
                            effect = new SfxEffectVibrato(AudioNote.PitchToHz(note.pitch - 0.25f), AudioNote.PitchToHz(note.pitch + 0.25f), 8);
                            break;
                        case 3: //drop
                            effect = new SfxEffectLerp(note.hz, 0, note.Volume01, note.Volume01, noteLength);
                            break;
                        case 4: //Fade In
                            effect = new SfxEffectLerp(note.hz, note.hz, 0, note.Volume01, noteLength);
                            break;
                        case 5: //Fade Out
                            effect = new SfxEffectLerp(note.hz, note.hz, note.Volume01, 0, noteLength);
                            break;
                        case 6: //fast arpeggio
                        case 7: //slow arpeggio
                            int addr = sfxPointer + (sfxOffset >> 3); //sfxOffset >> 2 equals to division by 8 but faster
                            var n0 = GetNote(addr);
                            var n1 = GetNote(addr + 2);
                            var n2 = GetNote(addr + 4);
                            var n3 = GetNote(addr + 6);

                            float hz = mem.Peek(sfxPointer + 65) > 8 ? 128 / 4 : 128 / 2;
                            if (note.effect == 7) hz = hz / 2;

                            effect = new SfxEffectArpeggio(n0.hz, n1.hz, n2.hz, n3.hz, hz);
                            break;
                    }

                    instruments[i].Play(note, effect);
                }
                else
                {
                    instruments[i].Stop();
                }
            }
        }

        private void StepNode()
        {
            if (Looping && (sfxOffset == loopEnd * 2))
            {
                sfxOffset = loopStart * 2 - 2;
            }

            sfxOffset += 2;
            if (sfxOffset > 63)
            {
                sfxPointer = -1;
            }
            else
            {
                time -= noteLength;
                PullNode();
            }
        }

        public float Sample(double delta)
        {
            if (sfxPointer < 0) return 0;

            time = (time + delta) % 10;

            if (time > noteLength)
            {
                StepNode();
            }

            float sample = 0;
            for (int i = 0; i < instruments.Length; i++)
            {
                sample += instruments[i].Sample(delta);
            }

            if (sample > 1) sample = 1;
            if (sample < -1) sample = -1;

            return sample;
        }

        private AudioNote GetNote(int addr)
        {
            byte lo = mem.Peek(addr);
            byte hi = mem.Peek(addr + 1);

            return AudioNote.Decode(lo, hi);
        }
    }
}
