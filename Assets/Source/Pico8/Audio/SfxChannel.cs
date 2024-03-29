﻿using PicoUnity.Audio;
using UnityEngine;

namespace PicoUnity
{
    internal class SfxChannel
    {
        private int       sfxPointer = -1;
        private int       sfxOffset;
        private int       sfxLength;
        private double    time;
        private float     noteLength;
        private AudioNote note;
        private AudioNote lastNote;
        private int       loopStart;
        private int       loopEnd;
        private int       activeInstrument;

        private MemoryModule mem;

        private SfxInstrument[] instruments;

        private Arpeggiator arpeggiator;

        public bool IsPlaying { get; private set; }
        public bool Looping { get; set; }

        public SfxChannel(MemoryModule mem)
        {
            this.mem = mem;

            instruments = new SfxInstrument[8];

            instruments[0] = new SfxInstrument (AudioSynth.Waveform.Triangle);
            instruments[1] = new SfxInstrument (AudioSynth.Waveform.TiltedTriangle);
            instruments[2] = new SfxInstrument (AudioSynth.Waveform.Sawtooth);
            instruments[3] = new SfxInstrument (AudioSynth.Waveform.Square);
            instruments[4] = new SfxInstrument (AudioSynth.Waveform.Pulse);
            instruments[5] = new SfxInstrument (AudioSynth.Waveform.Organ);
            instruments[6] = new SfxInstrument (AudioSynth.Waveform.Noise);
            instruments[7] = new SfxInstrument (AudioSynth.Waveform.Phaser);

            activeInstrument = -1;

            arpeggiator = new Arpeggiator();
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

            activeInstrument = -1;

            if (n == -1)
            {
                sfxPointer = -1;
                return;
            }

            IsPlaying = true;

            sfxPointer = MemoryModule.ADDR_SOUND + 68 * n;
            sfxOffset = offset * 2;
            time = 0;
            noteLength = mem.Peek(sfxPointer + 65) / 128f;
            loopStart = mem.Peek(sfxPointer + 66);
            loopEnd = mem.Peek(sfxPointer + 67);

            lastNote = note = AudioNote.Default;

            Looping = loopStart != loopEnd;

            int sfxLength = GetSfxLength(n);
            if (length == 0) length = sfxLength;

            if (offset + length > sfxLength)
                this.sfxLength = sfxLength * 2;
            else 
                this.sfxLength = (offset + length) * 2;

            if (note.isCustom) Debug.LogWarning("Sfx: custom instruments are not supported");

            PullNode();
        }

        private void PullNode()
        {
            lastNote = note;

            int noteAddr = sfxPointer + sfxOffset;
            note = GetNote(noteAddr);

            arpeggiator.Active = false;
            activeInstrument = -1;

            for (int i = 0; i < instruments.Length; i++)
            {
                var waveform = (AudioSynth.Waveform)note.waveform;

                if (instruments[i].Waveform == waveform)
                {
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

                            arpeggiator.NoteTime = 1 / hz;
                            arpeggiator.SetNotes(n0.hz, n1.hz, n2.hz, n3.hz);
                            arpeggiator.Active = true;

                            effect = null;
                            break;
                    }

                    if (note.volume == 0)
                    {
                        instruments[i].Stop();
                    }
                    else 
                    {
                        activeInstrument = i;
                        instruments[i].Play(note.hz, note.Volume01, effect);
                    }
                }
                else
                {
                    instruments[i].Stop();
                }
            }

            //reset the arpeggiator if not in use
            if (!arpeggiator.Active) arpeggiator.Reset();
        }

        private void StepNode()
        {
            if (Looping && (sfxOffset == loopEnd * 2))
            {
                sfxOffset = loopStart * 2 - 2;
            }

            sfxOffset += 2;
            if (sfxOffset >= sfxLength)
            {
                sfxPointer = -1;
                activeInstrument = -1;

                for (int i = 0; i < instruments.Length; i++)
                {
                    instruments[i].Stop();
                }
            }
            else
            {
                time -= noteLength;
                PullNode();
            }
        }

        public float Sample(double delta)
        {
            if (sfxPointer < 0 && !IsPlaying) return 0;

            time = (time + delta) % 10;

            if (time > noteLength)
            {
                StepNode();
            }

            if (arpeggiator.Active && activeInstrument > -1)
            {
                instruments[activeInstrument].Frequency = arpeggiator.Step(delta);
            }

            IsPlaying = sfxOffset < sfxLength && sfxPointer > -1;
            float sample = 0;
            for (int i = 0; i < instruments.Length; i++)
            {
                sample += instruments[i].Sample(delta);

                if (instruments[i].Active) IsPlaying = true;
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

        private int GetSfxLength(int n)
        {
            int pointer = MemoryModule.ADDR_SOUND + 68 * n;

            for (int i = 31; i >= 0; i--)
            {
                int addr = pointer + i * 2;

                var note = GetNote(addr);

                if (note.volume > 0) return i + 1;
            }

            return 0;
        }
    }
}
