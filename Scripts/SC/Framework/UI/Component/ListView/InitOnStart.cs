using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SCFramework.LoopScrollRect))]
[DisallowMultipleComponent]
public class InitOnStart : MonoBehaviour {
	void Start () {
        GetComponent<SCFramework.LoopScrollRect>().RefillCells();
	}
}
