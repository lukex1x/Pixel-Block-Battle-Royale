using UnityEngine;

public class ObjectTrigger : MonoBehaviour {
	public string ActiveText = "Interactive";
	public float DistanceLimit = 2;
	public Vector3 Offset;
	protected GameObject characterTemp;

    public virtual void Start () {
	
	}

	void Update () {
		UpdateFunction();
	}
	
	protected void UpdateFunction(){
		if(characterTemp){
			if(Vector3.Distance(this.transform.position,characterTemp.transform.position + Offset) > DistanceLimit){
				OnExit();
			}else{
				OnStay();	
			}
		}
	}
	
	public virtual void OnStay(){
		
	}
	
	public virtual void OnExit(){
		characterTemp = null;
	}

	public virtual void Pickup (GameObject character)
	{
		characterTemp = character;
	}
    public void GetInfo()
    {
        string info = ActiveText;
        UnitZ.Hud.ShowInfo(info, this.transform.position);
    }

}
