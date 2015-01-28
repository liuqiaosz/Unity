using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonLibrary
{
	/**
	 * 向量各种运算工具类
	 **/
	public class VectorMathTools
	{
		/**
		 * 计算向量间的角度
		 **/
		public static float CalVectorAngle(Vector3 Start, Vector3 End)
		{
			float Angle = Mathf.Acos(Vector3.Dot(Start.normalized, End.normalized)) * Mathf.Rad2Deg;
			return Angle;
		}

		/**
		 * 计算指定角度,距离的位置向量
		 **/
		public static Vector3 CalVectorTarget(Quaternion StartRotation, Quaternion TargetRotation, Vector3 Distance)
		{
			return TargetRotation * StartRotation * Distance;
		}

		/**
		 * 屏幕坐标转世界坐标(射线方式)
		 **/
		public static Vector3 ScreenPointToWorldPointByRaycast(Vector3 ScreenPoint,Camera ViewCamera,int LayerMask)
		{
			Ray Ray = ViewCamera.ScreenPointToRay(ScreenPoint);
			RaycastHit Hit;
			if (Physics.Raycast(Ray, out Hit, 100, LayerMask))
			{
				return Hit.point;
			}
			return Vector3.zero;
		}
	}
}
