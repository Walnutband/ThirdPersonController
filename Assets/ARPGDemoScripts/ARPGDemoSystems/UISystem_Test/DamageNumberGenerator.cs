using UnityEngine;
using System.Collections.Generic;
using System;

namespace ARPGDemo.UISystem_Test
{

    public class DamageNumberGenerator : SingletonMono<DamageNumberGenerator>
    {
        public Transform mainCamera;
        [Header("预制体引用")]
        [SerializeField] private GameObject damageNumberPrefab;

        [Header("默认字体大小")]
        [SerializeField] private float defaultFontSize = 36f;

        [Header("偏移设置")]
        [SerializeField] private Vector3 positionOffset = new Vector3(0, 1f, 0);

        [Header("对象池设置（可选）")]
        [SerializeField] private bool usePooling = true;
        [SerializeField] private int poolSize = 20;

        private Queue<GameObject> damageNumberPool;
        private Transform poolParent;

        protected override void Start()
        {
            base.Start();
            if (usePooling) 
            {
                InitializePool();
            }
        }

        private void InitializePool()
        {
            damageNumberPool = new Queue<GameObject>();

            // 创建对象池的父对象，放到该生成器下。
            GameObject poolObj = new GameObject("DamageNumberPool");
            poolParent = poolObj.transform;
            poolParent.SetParent(transform);

            // 预先创建对象，以便随后使用。
            for (int i = 0; i < poolSize; i++)
            {
                GameObject damageNumber = Instantiate(damageNumberPrefab, poolParent);
                damageNumber.SetActive(false);
                damageNumberPool.Enqueue(damageNumber);
            }
        }

        /// <summary>
        /// 生成伤害数字
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <param name="isCrit">是否暴击</param>
        public void SpawnDamageNumber(float damage, Vector3 worldPosition, bool isCrit)
        {
            Vector3 spawnPosition = worldPosition + positionOffset;

            if (usePooling && damageNumberPool != null)
            {//尝试从池中取对象。
                SpawnFromPool(damage, spawnPosition, isCrit);
            }
            else
            {
                SpawnNewInstance(damage, spawnPosition, isCrit);
            }
        }

        private void SpawnNewInstance(float damage, Vector3 position, bool isCrit)
        {
            GameObject damageNumberObj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            DamageNumber damageNumber = damageNumberObj.GetComponent<DamageNumber>();

            if (damageNumber == null)
            {
                Debug.LogError("伤害数字预制体必须包含DamageNumber组件！");
                Destroy(damageNumberObj);
                return;
            }

            damageNumber.Initialize(damage, isCrit, defaultFontSize);
        }

        private void SpawnFromPool(float damage, Vector3 position, bool isCrit)
        {
            if (damageNumberPool.Count == 0)
            {
                // 池为空时动态创建
                SpawnNewInstance(damage, position, isCrit);
                return;
            }

            GameObject damageNumberObj = damageNumberPool.Dequeue();
            damageNumberObj.transform.position = position;
            damageNumberObj.SetActive(true);

            DamageNumber damageNumber = damageNumberObj.GetComponent<DamageNumber>();
            damageNumber.Initialize(damage, isCrit, defaultFontSize);

            // 动画结束后自动回收
            StartCoroutine(RecycleAfterAnimation(damageNumberObj));
        }

        private System.Collections.IEnumerator RecycleAfterAnimation(GameObject obj)
        {
            // 等待动画完成（可以根据实际动画时间调整）
            yield return new WaitForSeconds(0.8f);

            if (obj != null)
            {
                obj.SetActive(false);
                damageNumberPool.Enqueue(obj);
            }
        }

        /// <summary>
        /// 批量生成伤害数字
        /// </summary>
        public void SpawnDamageNumbers(DamageInfo[] damageInfos)
        {
            foreach (var info in damageInfos)
            {
                SpawnDamageNumber(info.damage, info.position, info.isCrit);
            }
        }
    }

    // 用于批量生成的数据结构
    [System.Serializable]
    public struct DamageInfo
    {
        public float damage;
        public Vector3 position;
        public bool isCrit;

        public DamageInfo(float damage, Vector3 position, bool isCrit)
        {
            this.damage = damage;
            this.position = position;
            this.isCrit = isCrit;
        }
    }
}