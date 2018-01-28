using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth {
	float CurrentHP { get; }
	float MaxHP { get; }
	void Heal (float amount);
	void Damage (float amount);
}