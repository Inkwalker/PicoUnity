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
                    pset(0, 0, 7)
                    pset(2, 0, 7)
                    pset(0, 2, 7)
                    pset(2, 2, 7)
                    pset(1, 1, 8)
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