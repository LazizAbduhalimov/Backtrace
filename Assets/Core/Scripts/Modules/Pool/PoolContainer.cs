using UnityEngine;

namespace PoolSystem.Alternative
{
    public class PoolContainer : MonoBehaviour
    {
        public PoolMono<PoolObject> Pool { get; private set; }

        [SerializeField] private int _poolCount = 5;
        [SerializeField] private bool _autoExpand = true;
        [SerializeField] private PoolObject _poolObject;
        [SerializeField] private Transform _container;

        private void OnValidate()
        {
            if (_container == null)
                _container = transform;
        }

        private void Awake()
        {
            if (_poolObject == null)
                Debug.Log(name);
            Pool = new PoolMono<PoolObject>(_poolObject, _poolCount, _container);
            Pool.AutoExpand = _autoExpand;
        }

        public T GetFromPool<T>(Vector3 position) where T : PoolObject
        {
            var poolObject = Pool.GetFreeElement();
            poolObject.transform.position = position;
            return (T)poolObject;
        }
        
        public PoolObject GetFromPool(Vector3 position)
        {
            var poolObject = Pool.GetFreeElement();
            poolObject.transform.position = position;
            return poolObject;
        }
    }
}
