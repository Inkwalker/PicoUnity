using UnityEngine;

namespace PicoUnity
{
    public class PicoUnityBehaviour : MonoBehaviour
    {
        [SerializeField] Renderer screen = default;
        [SerializeField] int targetFrameRate = 30;

        private PicoEmulator emulator;

        private float frameTime;
        private float timer;

        private void Awake()
        {
            frameTime = 1f / targetFrameRate;

            emulator = new PicoEmulator();

            screen.material.SetTexture("_MainTex", emulator.ScreenTexture);
        }

        private void Start()
        {
            emulator.Run(@"
                function _draw()
                    print('graphics test \130', 1, 1, 7)
                    
                    for x = 1, 63, 2 do
                        pset(x, 7, 7)
                    end

                    line(63, 127, 63, 63, 5)
                    line(127, 63, 63, 63, 5)
                    line(127, 127, 127, 63, 11)
                    line(127, 127, 63, 63, 11)
                    line(127, 127, 63, 127, 11)

                    circ(95, 31, 31, 8)
                    circfill(95, 31, 23, 2)

                    pal(3, 12)
                    pal(2, 14, 1)

                    o1 = 4
                    o2 = 8
                    rect(0 + o1, 127 - o1, 62 - o1, 63 + o1, 3)
                    rectfill(0 + o2, 127 - o2, 62 - o2, 63 + o2, 3)
                end
            ");
        }

        private void Update()
        {
            if (timer >= frameTime)
            {
                timer = 0;

                EmulatorUpdate();
            }

            timer += Time.deltaTime;
        }

        private void EmulatorUpdate()
        {
            emulator.Update();
        }
    }
}