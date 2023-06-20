using System;
using System.Collections.Generic;
using GridForColliders;
using PlayerWeapons;
using UnityEngine;

public class Player : ObjectInGridWithColliderCircle<ObjectTypeInGrid>
{
    public int Health { get; private set; }

    private List<PlayerWeapon> _allPlayerWeapons;

    private int _selectedWeaponIndex;
    //
    public int SelectedWeaponIndex
    {
        get => _selectedWeaponIndex;

        set
        {
            _selectedWeaponIndex = value;
            if (_selectedWeaponIndex < 0)
            {
                _selectedWeaponIndex = 0;
            }
            if (_selectedWeaponIndex > _allPlayerWeapons.Count - 1)
            {
                _selectedWeaponIndex = _allPlayerWeapons.Count - 1;
            }
            OnChangedSelectedWeaponIndex?.Invoke(_selectedWeaponIndex);
        }
    }

    public PlayerWeapon GetSelectedWeapon() => _allPlayerWeapons[_selectedWeaponIndex];

    public event Action<int> OnChangedHealth; // param (int health)

    public event Action<int> OnChangedSelectedWeaponIndex; // param (int selectedWeaponIndex)

    public Player(Vector2 pos, float circleColliderRadius, int healthStart, List<PlayerWeapon> allPlayerWeapons)
        : base(pos, ObjectTypeInGrid.Player,
            circleColliderRadius, CircleApproximationPrecision.Point8 // (8 due to medium object size)
        )
    {
        Health = healthStart;
        _allPlayerWeapons = allPlayerWeapons;
        _selectedWeaponIndex = 0;
    }

    public void UpdateCooldownOfAllWeapons(float deltaTimeSec)
    {
        foreach (var weapon in _allPlayerWeapons)
        {
            weapon.TryUpdateCooldown(deltaTimeSec);
        }
    }

    // todo NOTE: better to use 'default interface methods' \ 'TRAIT' C# 8.0; but I'm afraid to use it because of bugs in new versions of Unity
    public void AddHealth(int delta)
    {
        Health += delta;
        OnChangedHealth?.Invoke(Health);
    }

    public bool IsAlive()
    {
        return Health > 0;
    }
}