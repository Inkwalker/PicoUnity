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

        private void Awake()
        {
            frameTime = 1f / targetFrameRate;

            emulator = new PicoEmulator();

            screen.material.SetTexture("_MainTex", emulator.ScreenTexture);

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