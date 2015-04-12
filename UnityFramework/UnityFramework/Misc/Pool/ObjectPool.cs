using System;
using System.Collections.Generic;

using System.Text;
using CommonLibrary;
using UnityFramework.Misc.Pool;

namespace UnityFramework.Misc.Pool
{
    /**
     * 对象池
     * 
     **/
    public class ObjectPool : Singlton<ObjectPool>,IDisposable
    {
        private const int CAPACITY = 20;

        private Dictionary<Type, int> CapacityDict = null;
        private Dictionary<Type, int> ObjectUsedDict = null;
        private Dictionary<Type, Queue<Object>> ObjectDict = null;
        private Dictionary<Type, List<Object>> ObjectUsedRef = null;
        private ObjectPool()
        {
            ObjectDict = new Dictionary<Type, Queue<object>>();
            ObjectUsedDict = new Dictionary<Type, int>();
            CapacityDict = new Dictionary<Type, int>();
            ObjectUsedRef = new Dictionary<Type, List<object>>();
        }
        
        /**
         * 获取对象实例
         **/
        public T GetObjectInstance<T>() where T : IPoolable,new()
        {
            T Instance = default(T);
            Type Prototype = typeof(T);
            if (!ObjectDict.ContainsKey(Prototype))
            {
                CreateObjectPool<T>();
            }

            List<Object> Used;
            Queue<Object> Objects;
            bool Result = ObjectDict.TryGetValue(Prototype, out Objects);

            if (Result)
            {
                ObjectUsedRef.TryGetValue(Prototype,out Used);
                int UsedCount = 0;
                //int Capacity = 0;
                ObjectUsedDict.TryGetValue(Prototype, out UsedCount);
                //CapacityDict.TryGetValue(Prototype, out Capacity);

                if (Objects.Count > 0)
                {
                    //返回第一个
                    Instance = (T)Objects.Dequeue();
                    //更新使用计数
                    UsedCount++;
					ObjectUsedDict[Prototype] = UsedCount;
                    //添加到使用中的队列
                    Used.Add(Instance);
                }
                else
                {
                    //if (UsedCount < Capacity)
                    //{
                        //创建新的实例
                        Instance = new T();
                        //更新使用计数
                        UsedCount++;
						ObjectUsedDict[Prototype] = UsedCount;
						
                        //添加到使用中的队列
                        Used.Add(Instance);
                    //}
                }
            }
            return Instance;
        }

        /**
         * 对象入池
         **/
        public void ReturnObject<T>(T Instance) where T : IPoolable, new()
        {
            Type Prototype = typeof(T);
            if (ObjectDict.ContainsKey(Prototype))
            {
                Queue<Object> Objects;
                List<Object> Used;
                bool Result = ObjectDict.TryGetValue(Prototype, out Objects);
                if (Result)
                {
                    ObjectUsedRef.TryGetValue(Prototype, out Used);
					int Capacity = 0;
					CapacityDict.TryGetValue(Prototype, out Capacity);
                    if(Used.Contains(Instance))
                    {
                        //更新使用引用计数
                        int UsedCount = 0;
                        ObjectUsedDict.TryGetValue(Prototype, out UsedCount);
                        UsedCount--;
						ObjectUsedDict[Prototype] = UsedCount;

                        //从使用队列删除,并且将对象加入未使用队列
                        Used.Remove(Instance);

						if (Objects.Count >= Capacity)
						{
							//空闲对象到达阀值则将对象直接丢弃等待垃圾回收
							((IDisposable)Instance).Dispose();
						}
						else
						{
							((IPoolable)Instance).Reset();
							Objects.Enqueue(Instance);
						}
                    }
                }
            }
        }

        /**
         * 创建对象池
         **/
        public void CreateObjectPool<T>(int CapacitySize = CAPACITY) where T : IPoolable,new()
        {
            Type Prototype = typeof(T);
            if (!ObjectDict.ContainsKey(Prototype))
            {
                Queue<Object> Pool = new Queue<Object>(CAPACITY);
                ObjectDict.Add(Prototype, Pool);
                ObjectUsedRef.Add(Prototype, new List<Object>());
                ObjectUsedDict.Add(Prototype, 0);
                CapacityDict.Add(Prototype, CapacitySize);
            }
        }

		/**
		 * 获得指定类型对象池的状态
		 * 
		 **/
		public string DumpPoolStatus<T>()
		{
			StringBuilder Result = new StringBuilder();
			Type Prototype = typeof(T);
			if (ObjectDict.ContainsKey(Prototype))
			{
				int Capacity = 0;
				CapacityDict.TryGetValue(Prototype, out Capacity);
				Result.AppendLine("Capacity = " + Capacity);
				Queue<Object> Pool;
				ObjectDict.TryGetValue(Prototype, out Pool);
				Result.AppendLine("Idle Object = " + Pool.Count);
				int Used = 0;
				ObjectUsedDict.TryGetValue(Prototype, out Used);
				Result.AppendLine("Used=" + Used);
			}
			return Result.ToString();
		}

        public void Dispose()
        {

        }
    }
}
