using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InteractionSystem
{
    public class InputManager : MonoBehaviour
    {
        [Header("INTERACT BUTTON")]
        [Space(10)]
        public KeyCode interactButton;

        [Header("CAMERA")]
        [Space(10)]
        public KeyCode switchCamera;

        [Header("FLASHLIGHT BUTTON")]
        [Space(10)]
        public KeyCode flashlightSwitchV2;
        public KeyCode reloadBattery;

        [Header("GEAR BUTTON")]
        [Space(10)]
        public KeyCode gear;

        [Header("CAMERA BUTTON")]
        [Space(10)]
        public KeyCode camera;
        public KeyCode reloadPistol;

        [Header("TREATMENT BUTTON")]
        [Space(10)]
        public KeyCode treatment;

        [Header("RUNNING BUTTON")]
        [Space(10)]
        public KeyCode running;

        [Header("MESHRENDER BUTTON")]
        [Space(10)]
        public KeyCode meshRender;


        public static InputManager instance;
        public Button flashLightButton;
        private bool canUnclick;
        private void Awake()
        {
            if (instance != null) 
            { 
                Destroy(gameObject); 
            }
            else 
            { 
                instance = this; 
            }

            flashLightButton.onClick.AddListener(FlashLightButton);
        }
        private void Start()
        {
            flashLightButton.onClick.AddListener(FlashLightButton);
        }
        public bool GetFLkeyDown;
        void FlashLightButton()
        {
            Debug.Log("PRESSED FLASHLIGHT");
            GetFLkeyDown = true;
            canUnclick = false;
        }
        private void Update()
        {
            if (canUnclick) { GetFLkeyDown = false; }
        }

        private void LateUpdate()
        {
            canUnclick = true;
        }
    }
}
