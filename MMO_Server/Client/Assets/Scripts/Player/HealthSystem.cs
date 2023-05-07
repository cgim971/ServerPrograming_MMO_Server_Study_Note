using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour {

    private Slider _healthSlider;
    int _healthAmount;
    int _healthAmountMax;

    public event EventHandler OnDamaged;
    public event EventHandler OnDied;
    public event EventHandler OnRevive;

    public void Init(int maxHealth) {
        _healthSlider = GetComponent<Slider>();
        _healthSlider.maxValue = maxHealth;
        _healthAmountMax = maxHealth;
        SetHealth(maxHealth);
    }

    public void Die() {
        OnDied?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDead() {
        return _healthAmount == 0;
    }

    public int GetHealthAmount() {
        return _healthAmount;
    }

    public float GetHealthAmountNormalized() {
        return (float)_healthAmount / _healthAmountMax;
    }

    public bool IsFullHealth() {
        return _healthAmount == _healthAmountMax;
    }
    public int GetHealthAmounMax() {
        return _healthAmountMax;
    }

    public void SetHealth(int healthAmount) {
        _healthAmount = healthAmount;
        _healthAmount = Mathf.Clamp(_healthAmount, 0, _healthAmountMax);
        _healthSlider.value = _healthAmount;
    }

    public void Revive(int healthAmount) {
        SetHealth(healthAmount);
        OnRevive?.Invoke(this, EventArgs.Empty);    
    }

    public void Damage(int healthAmount) {
        SetHealth(healthAmount);
        OnDamaged?.Invoke(this, EventArgs.Empty);
    }
}
