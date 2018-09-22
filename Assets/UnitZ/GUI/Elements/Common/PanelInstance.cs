//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class PanelInstance : MonoBehaviour {
	// use for adding to panel. to working with PanelManager
	public PanelInstance PanelBefore;
	public RuntimeAnimatorController runtimeAnim;
    public bool LockMouse;
    public bool isClosed;
	private bool hasAnimator;

	public void OpenPanel(bool active){
		isClosed = !active;
		this.gameObject.SetActive (active);

		Animator animator = this.GetComponent<Animator> ();
		if (animator != null && animator.isActiveAndEnabled) {
			animator.SetBool ("Open", active);
		}
	}

	void OnDisable()
	{
		Animator animator = this.GetComponent<Animator> ();
		if(animator)
		Destroy(animator);
	}

	void OnEnable()
	{
		if (hasAnimator) {
			Animator animator = this.GetComponent<Animator> ();
			if (animator == null) {
				animator = gameObject.AddComponent<Animator> ();
				animator.runtimeAnimatorController = runtimeAnim;

			}
		}
	}

	public void Closed(){
		isClosed = true;
	}


	void Start () {
		Animator animator = this.GetComponent<Animator> ();
		if (animator) {
			runtimeAnim = animator.runtimeAnimatorController;
			hasAnimator = true;
		} else {
			hasAnimator = false;
		}
	}


}
