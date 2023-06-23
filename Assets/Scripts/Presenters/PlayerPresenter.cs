using System.Collections.Generic;
using PlayerWeapons;
using UnityEngine;
using Views;

namespace Presenters
{
    public class PlayerPresenter : BasePresenter<Player>
    {
        private readonly PlayerView _playerView;

        public PlayerPresenter(Player model, PlayerView view)
            : base(model, view.gameObject)
        {
            _playerView = view;

            Model.OnChangedHealth += _playerView.UpdateHealth;
            Model.OnChangedSelectedWeaponIndex += _playerView.UpdateSelectedWeaponIndex;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Model.OnChangedHealth -= _playerView.UpdateHealth;
            Model.OnChangedSelectedWeaponIndex -= _playerView.UpdateSelectedWeaponIndex;
        }

        public void TryReadInputAndHandle(float deltaTimeSec, Rect mapRect, float playerMoveSpeedPerSec,
            IForWeaponUse iForWeaponUse)
        {
            TryReadInputAndAssignDeltaForMovePlayer(deltaTimeSec, mapRect, playerMoveSpeedPerSec);
            TryReadInputForChangeSelectedWeaponThroughMouseWheel();
            TryReadInputForChangeSelectedWeaponThroughKeyCodes();
            TryUsePlayerWeapon(iForWeaponUse);
        }

        private void TryReadInputAndAssignDeltaForMovePlayer(float deltaTimeSec,
            Rect mapRect, float playerMoveSpeedPerSec)
        {
            Vector2 direction = Vector2.zero;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                direction.y += 1.0f;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                direction.y -= 1.0f;
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                direction.x -= 1.0f;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                direction.x += 1.0f;
            }

            // (set the delta in any case, so that there is no left from the previous frame)
            Model.DeltaPosPerSec = direction.normalized * playerMoveSpeedPerSec;

            if (direction != Vector2.zero)
            {
                // we prohibit going out of bounds by checking the future position
                // and ignoring input in case of out of bounds
                // (we check exactly the center and not each point of the collider)
                Vector2 futurePos = Model.GetFuturePos(deltaTimeSec);
                bool needIgnoreInputDueToOutOfBounds = !mapRect.Contains(futurePos);
                if (needIgnoreInputDueToOutOfBounds)
                {
                    //Debug.LogWarning("needIgnoreInputDueToOutOfBounds");
                    Model.DeltaPosPerSec = Vector2.zero;
                }
            }
        }

        private void TryReadInputForChangeSelectedWeaponThroughMouseWheel()
        {
            if (Mathf.Approximately(Input.mouseScrollDelta.y, 0)) return;

            if (Input.mouseScrollDelta.y < 0)
            {
                Model.SelectedWeaponIndex--;
            }
            else
            {
                Model.SelectedWeaponIndex++;
            }

            Debug.Log("ChangeSelectedWeapon: SelectedWeaponIndex = " + Model.SelectedWeaponIndex);
        }

        private readonly List<KeyCode> _keyCodesForChangeSelectedWeaponIndex = new List<KeyCode>()
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9
        };
        //
        private void TryReadInputForChangeSelectedWeaponThroughKeyCodes()
        {
            for (int i = 0; i < _keyCodesForChangeSelectedWeaponIndex.Count; i++)
            {
                if (Input.GetKeyDown(_keyCodesForChangeSelectedWeaponIndex[i]))
                {
                    Model.SelectedWeaponIndex = i;
                    Debug.Log("ChangeSelectedWeapon: SelectedWeaponIndex = " + Model.SelectedWeaponIndex);
                }
            }
        }

        private void TryUsePlayerWeapon(IForWeaponUse iForWeaponUse)
        {
            if (!Input.GetMouseButton(0)) return; // (only while the left mouse button is pressed)

            PlayerWeapon selectedWeapon = Model.GetSelectedWeapon();
            Vector2 mousePos = Input.mousePosition;

            //Debug.Log("Use Weapon: SelectedWeaponIndex = " + Model.SelectedWeaponIndex);
            //Debug.Log("mousePos.x = " + mousePos.x + "; mousePos.y = " + mousePos.y);
            //Debug.Log("Model.Pos.x = " + Model.Pos.x + "; Model.Pos.y = " + Model.Pos.y);

            selectedWeapon.TryUse(iForWeaponUse, Model.Pos, mousePos);
        }

        public void UpdateCooldownOfAllWeapons(float deltaTimeSec)
        {
            Model.UpdateCooldownOfAllWeapons(deltaTimeSec);
        }
    }
}