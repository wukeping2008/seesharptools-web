using System.Collections.Concurrent;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 对象池实现
    /// 用于减少对象分配和垃圾回收压力
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly ConcurrentQueue<T> _objects = new();
        private readonly Func<T> _objectGenerator;
        private readonly Action<T>? _resetAction;
        private readonly int _maxSize;
        private int _currentCount = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="objectGenerator">对象生成器</param>
        /// <param name="resetAction">对象重置动作</param>
        /// <param name="maxSize">最大池大小</param>
        public ObjectPool(Func<T> objectGenerator, Action<T>? resetAction = null, int maxSize = 100)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _resetAction = resetAction;
            _maxSize = maxSize;
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        /// <returns>对象实例</returns>
        public T Get()
        {
            if (_objects.TryDequeue(out var item))
            {
                Interlocked.Decrement(ref _currentCount);
                return item;
            }

            return _objectGenerator();
        }

        /// <summary>
        /// 将对象返回到池中
        /// </summary>
        /// <param name="item">要返回的对象</param>
        public void Return(T item)
        {
            if (item == null) return;

            if (_currentCount < _maxSize)
            {
                _resetAction?.Invoke(item);
                _objects.Enqueue(item);
                Interlocked.Increment(ref _currentCount);
            }
        }

        /// <summary>
        /// 获取当前池中对象数量
        /// </summary>
        public int Count => _currentCount;

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            while (_objects.TryDequeue(out _))
            {
                Interlocked.Decrement(ref _currentCount);
            }
        }
    }
}
