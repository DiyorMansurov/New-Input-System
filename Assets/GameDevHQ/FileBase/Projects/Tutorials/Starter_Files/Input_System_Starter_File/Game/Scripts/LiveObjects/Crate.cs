using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private PlayerInputActions _input;

        private float _timer = 0f;
        private bool _isHolding = false;
        private int _countToBreak;
        private InteractableZone zone;
        private bool _isCrateZone => zone != null && zone.GetZoneID() == 6;
        private bool _isCrateZoneCompleted = false;
        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private void Awake() {
            _input = new PlayerInputActions();   
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            
            _input.Human.Enable();
            _input.Human.Interact.started += Interact_started;
            _input.Human.Interact.canceled += Interact_canceled;
        }
//-------------------------------------------------------------
        
        private void Interact_started(InputAction.CallbackContext context)
        {
            if (_isReadyToBreak && _isCrateZone)
            {
                _isHolding = true;
            }
        }
        private void Update()
        {
            if (_isReadyToBreak && _isHolding && _brakeOff.Count > 0 && _isCrateZone)
            {
                CalculateBreakForce();
            } else
            {
                UIManager.Instance.DisplayCrateBreakSlider(false);
            }

            CheckCrateDestroyed();
            
        }

        private void CalculateBreakForce()
        {
            if (_isHolding && _timer * 4f < (float)_pieces.Length)
            {
                _timer += Time.deltaTime;
            } 

            _countToBreak = Mathf.FloorToInt(_timer * 4f);

            if (_countToBreak > 0)
            {
                UIManager.Instance.DisplayCrateBreakSlider(true);
            } else
            {
                UIManager.Instance.DisplayCrateBreakSlider(false);
            }
            
            Debug.Log(_timer);
            if (_pieces.Length != 0)
            {
                UIManager.Instance.UpdateCrateBreakSlider(_countToBreak / (float)_pieces.Length);
            }
            
        }   

        private void CheckCrateDestroyed()
        {
            if (_brakeOff.Count > 0 || _isCrateZoneCompleted)
                return;

            _isReadyToBreak = false;
            _crateCollider.enabled = false;
            _interactableZone.CompleteTask(6);
            _isCrateZoneCompleted = true;
            UIManager.Instance.DisplayCrateTutorial(false);
            Debug.Log("Completely Busted");
        }

         private void Interact_canceled(InputAction.CallbackContext context)
        {
            _isHolding = false;

            if (!_isReadyToBreak || !_isCrateZone || _brakeOff.Count <= 0)
                return;

            int breaks = Mathf.Max(1, _countToBreak);
            breaks = Mathf.Min(breaks, _brakeOff.Count);
            for (int i = 0; i < breaks; i++)
            {
                BreakPart();
            }

            StartCoroutine(PunchDelay());

            _timer = 0f;
            _countToBreak = 0;
        }
//-------------------------------------------------------------

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            this.zone = zone;
            
            if (_isReadyToBreak == false && _brakeOff.Count >0 && _isCrateZone)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
                UIManager.Instance.DisplayCrateTutorial(true);
            }

            // if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            // {
            //     if (_brakeOff.Count > 0)
            //     {
            //         BreakPart();
            //         StartCoroutine(PunchDelay());
            //     }
            //     else if(_brakeOff.Count == 0)
            //     {
            //         _isReadyToBreak = false;
            //         _crateCollider.enabled = false;
            //         _interactableZone.CompleteTask(6);
            //         Debug.Log("Completely Busted");
            //     }
            // }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
        }



        public void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        


        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;

            _input.Human.Disable();
            _input.Human.Interact.started -= Interact_started;
            _input.Human.Interact.canceled -= Interact_canceled;
        }
    }
}
