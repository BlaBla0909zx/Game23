using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;


    public class VFXPoolManager : MonoBehaviour
    {
        public static VFXPoolManager Instance { get; private set; }
        private Dictionary<VisualEffectID, ObjectPool<BaseVisualEffect>> _poolMap;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

        }

        private void Start()
        {
            LoadAllVFXPool();
        }


        private void LoadAllVFXPool()
        {
            _poolMap = new();
            foreach (var e in GameDataManager.Instance.VisualEffectDictionary)
            {
                ObjectPool<BaseVisualEffect> pool = null;
                pool = new ObjectPool<BaseVisualEffect>(
                    createFunc: () =>
                    {
                        BaseVisualEffect vfxInstance = Instantiate(e.Value, this.transform);
                        vfxInstance.SetPool(pool); // remember to set the pool
                        return vfxInstance;
                    },
                    actionOnGet: vfx => vfx.gameObject.SetActive(true),
                    actionOnRelease: vfx => vfx.gameObject.SetActive(false),
                    actionOnDestroy: vfx => Destroy(vfx.gameObject),
                    collectionCheck: false,
                    defaultCapacity: 5
                    
                );

                _poolMap.Add(e.Key, pool);
            }
        }

        public BaseVisualEffect GetEffect(VisualEffectID visualEffectID)
        {
            if (_poolMap.TryGetValue(visualEffectID, out var pool))
            {
                return pool.Get();
            }

            Debug.LogWarning($"No VFX pool registered for {visualEffectID}");
            return null;
        }
    }