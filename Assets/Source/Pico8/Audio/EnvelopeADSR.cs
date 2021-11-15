using UnityEngine;

namespace PicoUnity.Audio
{
    internal class EnvelopeADSR
    {
        public double AttackTime { get; set; }
        public double DecayTime { get; set; }
        public double ReleaseTime { get; set; }
        public float SustainVolume { get; set; }


        private double timer;

        public float Amplitude { get; private set; }
        public StateADSR State { get; private set; }
        public bool Stopped => State == StateADSR.Stopped;
        public bool Triggered => State != StateADSR.Release && State != StateADSR.Stopped;
        public bool Released => State == StateADSR.Release || State == StateADSR.Stopped;


        public EnvelopeADSR(double attack, double decay, float sustain, double release)
        {
            AttackTime = attack;
            DecayTime = decay;
            ReleaseTime = release;

            SustainVolume = sustain;
        }

        public void Trigger()
        {
            timer = 0;
            State = StateADSR.Attack;
            Amplitude = 0;
        }

        public void Release()
        {
            if (State != StateADSR.Stopped && State != StateADSR.Release)
            {
                State = StateADSR.Release;
                timer = 0;
            }
        }

        public double Step(double deltaTime)
        {
            if (Stopped) return 0;

            Amplitude = 1;

            if (State == StateADSR.Attack)
            { 
                if (timer >= AttackTime)
                {
                    State = StateADSR.Decay;
                    timer -= AttackTime;
                }
                else
                    Amplitude = (float)(timer / AttackTime);
            }

            if (State == StateADSR.Decay) 
            {
                if (timer >= DecayTime)
                {
                    State = StateADSR.Sustain;
                    timer = 0;
                }
                else
                {
                    float t = (float)(timer / DecayTime);
                    Amplitude = Mathf.Lerp(1f, SustainVolume, t);
                }
            }

            if (State == StateADSR.Sustain) 
            {
                Amplitude = SustainVolume;

                if (Mathf.Approximately(Amplitude, 0))
                {
                    State = StateADSR.Stopped;
                }
            }

            if (State == StateADSR.Release)
            {
                if (timer >= ReleaseTime)
                {
                    State = StateADSR.Stopped;
                    Amplitude = 0;
                }
                else
                    Amplitude =  1f - (float)(timer / ReleaseTime);
            }

            timer += deltaTime;

            return Amplitude;
        }

        public enum StateADSR
        {
            Stopped,
            Attack,
            Decay,
            Sustain,
            Release
        }
    }
}
