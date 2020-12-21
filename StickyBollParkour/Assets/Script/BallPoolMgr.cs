using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Collections;

public class BallPoolMgr : SingletonMonoBehaviour<BallPoolMgr>
{
    public GameObject BallPrefab;

    [HideInInspector]
    public GameObject _ballPool;
    ObjectPool<ball> _pool;

    
    public void InitPool()
    {
        if (_ballPool == null)
        {
            _ballPool = new GameObject("BallPool");
            //_ballPool.transform.SetParent(GameDriver.instance.transform);
        }

        CreatePool(2000);

    }

    void CreatePool(int initialSize)
    {
        _pool = new ObjectPool<ball>(initialSize, (pool) =>
        {
            GameObject gameObject = GameObject.Instantiate(BallPrefab, _ballPool.transform);

            gameObject.SetActive(false);
            ball b = gameObject.GetComponent<ball>();

            return b;
        }, (b) =>
        {
            b.gameObject.SetActive(true);
        }, (b) =>
        {
            if (LeanTween.isTweening(b.gameObject))
                LeanTween.cancel(b.gameObject);

            if (_ballPool != null)
                b.transform.SetParent(_ballPool.transform);
            b.SetScaleOrigin();
            b.gameObject.SetActive(false);
        }, false);
    }


    /// <summary>
    /// 拿取
    /// </summary>
    /// <param name="name"></param>
    /// <param name="delayDespawnTime"></param>
    /// <returns></returns>
    public ball Spawn(float delayDespawnTime = 5)
    {
        ball b = _pool.Acquire();
        if (delayDespawnTime != -1)
        {
            b.DelayDeSpawn(delayDespawnTime);
        }
        return b;
    }

    /// <summary>
    /// 回收
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public void Despawn(ball obj)
    {
        _pool.Free(obj);
    }

    //bool FreeCondition(ball obj)
    //{
    //    return true;
    //}

    ///// <summary>
    ///// 将所有正在活动的对象放入池中
    ///// </summary>
    //public void FreeAll()
    //{
    //    _pool.FreeAll(FreeCondition);
    //}
    
}
