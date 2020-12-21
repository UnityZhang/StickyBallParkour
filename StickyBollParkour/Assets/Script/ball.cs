using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Collections;

public class ball : MonoBehaviour, IPoolable
{
    public Rigidbody rig;
    public SphereCollider m_collider;

    [HideInInspector]
    public Vector3 originScale;

    public int poolIndex { get; set; }

    private void Awake()
    {
        originScale = transform.localScale;
    }
    
    public void SetScaleOrigin()
    {
        transform.localScale = originScale;
    }

    /// <summary>
    /// 延迟回收
    /// </summary>
    /// <param name="delayTime"></param>
    public void DelayDeSpawn(float delayTime = 0)
    {
        if (delayTime > 0)
        {
            LeanTween.delayedCall(delayTime, () =>
            {
                BallPoolMgr.instance.Despawn(this);
            });
        }
        else
        {
            BallPoolMgr.instance.Despawn(this);
        }

    }
}
