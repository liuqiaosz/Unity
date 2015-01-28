using UnityEngine;
using System.Collections;

namespace Kits
{
	/**
	 * 摄像机跟随,这里只支持主摄像机的跟随
	 * 脚本挂在那个对象上面就跟随那个对象
	 **/
	public class CameraFollow : MonoBehaviour 
	{
		public int MaxFollowDistance = 10;	//距离跟随目标的最大距离
		public int FollowDistance = 10;		//跟随目标的距离

		private Camera FollowCamera;

		public bool IsAutoFollow = true;

		//手动控制视角的参数配置
		public float YMin = 10;  
		public float YMax = 90;  
		private float X;
		private float Y;
		//private float DistanceLerp;  
		private bool IsActived = false;  

		void Awake()
		{
			//获取主摄像机的引用
			FollowCamera = Camera.main;
			if(null != FollowCamera)
			{
				if(!IsAutoFollow)
				{
					Y = FollowCamera.transform.eulerAngles.x;  
					X = FollowCamera.transform.eulerAngles.y; 
					CalDistance();
				}
			}
		}

		void Start () 
		{
		
		}

		void Update () 
		{
			
		}

		void LateUpdate()
		{
			if(null != FollowCamera)
			{
				if(IsAutoFollow)
				{
					//自动跟随
					//Quaternion Rotation = Quaternion.Euler(new Vector3(
						//transform.rotation.eulerAngles.x + 30,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z));
					//FollowCamera.transform.rotation = Rotation;
					FollowCamera.transform.position = FollowCamera.transform.rotation * (new Vector3(0,0,-10)) + transform.position;
				}
				else
				{
					if (Input.GetMouseButtonDown(1))  
					{  
						IsActived = true;  
					}  
					
					if (Input.GetMouseButtonUp(1))   
					{  
						IsActived = false;  
					}  

					if (IsActived)  
					{  
						Y -= Input.GetAxis("Mouse Y") * 3;  
						X += Input.GetAxis("Mouse X") * 3;  

						//限制角色控制上下镜头的范围  
						Y = ClampAngle(Y, YMin, YMax);  
					}  
					
					CalDistance();
				}
			}
		}

		/** 
	     * 计算镜头和角色的距离和角度并且重置镜头位置和角度 
	     **/  
		private void CalDistance()  
		{  
			Quaternion Rotation = Quaternion.Euler(Y, X, 0);  
			Vector3 DistancePos = new Vector3(0, 0, -FollowDistance);  
			
			//CameraPosition = Rotation * DistancePos + transform.position;  
			
			FollowCamera.transform.rotation = Rotation;  
			FollowCamera.transform.position = Rotation * DistancePos + transform.position;  
		}  

		private float ClampAngle(float angle, float min, float max)  
		{  
			if (angle < -360)  
			{  
				angle += 360;  
			}  
			if (angle > 360)  
			{  
				angle -= 360;  
			}  
			return Mathf.Clamp(angle, min, max);  
		}  
	}
}