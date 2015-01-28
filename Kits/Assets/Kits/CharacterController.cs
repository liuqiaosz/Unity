using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
	public GameObject Character;
	private Animator Anim;
	private bool IsActive = false;
	private Vector3 TargetPosition;
	private Quaternion TargetRotation;
	private Transform Trans;

	void Start ()
	{
		if(null != Character)
		{
			Trans = Character.transform;
		}
		else
		{
			Trans = transform;
		}

		TargetRotation = Trans.rotation;
		TargetPosition = Trans.position;
		
		Anim = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update ()
	{
		//Quaternion r = transform.rotation;
		//Vector3 f0 = (transform.position  + (r * Vector3.forward) * 5.0f);
		//Debug.DrawLine(transform.position,f0,Color.red);
		
		if(Input.GetMouseButton(0))
		{
			Ray Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit Hit;
			if (Physics.Raycast(Ray, out Hit, 100))
			{
				TargetPosition = new Vector3(Hit.point.x,Trans.position.y,Hit.point.z);
				TargetRotation = Quaternion.LookRotation(TargetPosition - Trans.position,Vector3.up);
				
				if(Vector3.Distance(Trans.position,TargetPosition) < 0.3)
				{
					Trans.position = TargetPosition;
					Trans.rotation = TargetRotation;
				}
				else
				{
					IsActive = true;
				}
			}
		}
		if(IsActive)
		{
			if(Vector3.Distance(Trans.position,TargetPosition) > 0.3f)
			{
				Quaternion Dir = Quaternion.LookRotation(TargetPosition - Trans.position,Vector3.up);
				Trans.rotation = Quaternion.Slerp(Trans.rotation,Dir,10 * Time.deltaTime);
				
				Trans.Translate(Vector3.forward * 5 * Time.deltaTime);
			}
			else
			{
				IsActive = false;
				Trans.position = TargetPosition;
				Trans.rotation = TargetRotation;
			}
		}
	}
}
