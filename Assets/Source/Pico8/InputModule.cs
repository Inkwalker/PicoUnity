using System;
using UnityEngine;

namespace PicoUnity
{
    public class InputModule : EmulatorModule
    {
        private ButtonState[] player_0;
        private ButtonState[] player_1;

        public InputModule()
        {
            player_0 = new ButtonState[8];
            player_1 = new ButtonState[8];

            for (int i = 0; i < 8; i++)
            {
                player_0[i] = new ButtonState();
                player_1[i] = new ButtonState();
            }
        }

        private object Btn(int? i, int? p = 0)
        {
            p = p.HasValue ? p : 0;

            if (i.HasValue)
            {
                var btn = p.Value == 0 ? player_0[i.Value] : player_1[i.Value];

                return btn.State;
            }
            else
            {
                int result = 0;

                for (int b = 0; b < 8; b++)
                {
                    var p0_bit = player_0[b].State ? 1 : 0;
                    var p1_bit = player_0[b].State ? 1 : 0;

                    result |= p0_bit << b;
                    result |= p1_bit << b + 8;
                }

                return (ushort)result;
            }
        }

        private object Btnp(int? i, int? p = 0)
        {
            p = p.HasValue ? p : 0;

            if (i.HasValue)
            {
                var btn = p.Value == 0 ? player_0[i.Value] : player_1[i.Value];

                return btn.Pressed;
            }
            else
            {
                int result = 0;

                for (int b = 0; b < 8; b++)
                {
                    var p0_bit = player_0[b].Pressed ? 1 : 0;
                    var p1_bit = player_0[b].Pressed ? 1 : 0;

                    result |= p0_bit << b;
                    result |= p1_bit << b + 8;
                }

                return (ushort)result;
            }
        }

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                {"btn", (Func<int?, int?, object>) Btn },
                {"btnp", (Func<int?, int?, object>) Btnp }
            };
        }

        public override void OnFrameStart(float dt)
        {
            var p0_btn = new bool[8];
            p0_btn[0] = Input.GetAxis("Horizontal") < -0.3f;
            p0_btn[1] = Input.GetAxis("Horizontal") > 0.3f;
            p0_btn[2] = Input.GetAxis("Vertical") > 0.3f;
            p0_btn[3] = Input.GetAxis("Vertical") < -0.3f;
            p0_btn[4] = Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.N) || Input.GetButton("Fire1");
            p0_btn[5] = Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.M) || Input.GetButton("Fire2");

            var p1_btn = new bool[8];
            p1_btn[0] = Input.GetKey(KeyCode.S);
            p1_btn[1] = Input.GetKey(KeyCode.F);
            p1_btn[2] = Input.GetKey(KeyCode.E);
            p1_btn[3] = Input.GetKey(KeyCode.D);
            p1_btn[4] = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Tab);
            p1_btn[5] = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q);

            for (int i = 0; i < 8; i++)
            {
                player_0[i].Update(p0_btn[i], dt);
                player_1[i].Update(p1_btn[i], dt);
            }
        }

        private class ButtonState
        {
            const float RepeatTime = 0.5f;

            public bool Pressed { get; private set; }
            public bool State { get; private set; }

            private bool lastState;
            private float timer;
            private bool fastRepeat;

            public void Update(bool pressed, float dt)
            {
                if (pressed)
                    timer += dt;
                else
                {
                    timer = 0;
                    fastRepeat = false;
                }

                lastState = State;
                State = pressed;

                Pressed = !lastState && State;

                float t = fastRepeat ? RepeatTime / 2 : RepeatTime;

                if (timer > t)
                {
                    timer = 0;
                    fastRepeat = true;

                    Pressed = true;
                }
            }
        }
    }
}
