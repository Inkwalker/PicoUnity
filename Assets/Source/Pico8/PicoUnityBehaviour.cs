using UnityEngine;

namespace PicoUnity
{
    public class PicoUnityBehaviour : MonoBehaviour
    {
        [SerializeField] ACartridge cartridge = default;
        [SerializeField] Renderer screen = default;
        [SerializeField] int targetFrameRate = 30;

        private PicoEmulator emulator;

        private float frameTime;
        private float timer;
        private bool initialized;

        private void Awake()
        {
            frameTime = 1f / targetFrameRate;

            emulator = new PicoEmulator();
            emulator.AddModule(new InputModule());

            screen.material.SetTexture("_MainTex", emulator.ScreenTexture);

            initialized = true;
        }

        private void Start()
        {
            if (cartridge != null)
            {
                emulator.LoadCartridge(cartridge);
            }
        }

        private void Update()
        {
            if (timer >= frameTime)
            {
                EmulatorUpdate(timer);

                timer = 0;
            }

            timer += Time.deltaTime;

        }

        private void EmulatorUpdate(float dt)
        {
            emulator.Update(dt);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (initialized)
                emulator.FillAudioBuffer(data, channels);
        }
    }
}