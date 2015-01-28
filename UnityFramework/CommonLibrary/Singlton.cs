using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLibrary
{
	public class Singlton<T> where T : Singlton<T>
	{
		private static T _Instance;

		public static T Instance
		{
			get
			{
				if (null == _Instance)
				{
					_Instance = (T)Activator.CreateInstance(typeof(T),true);
					_Instance.Initializer();
				}
				return _Instance;
			}
		}

		protected virtual void Initializer()
		{

		}
	}
}
