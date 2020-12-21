using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutObstacle : MonoBehaviour
{
    BoxCollider m_collider;

    public int LimitLevel = 1;

    private void Awake()
    {
        m_collider = GetComponent<BoxCollider>();
    }
    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            GameMgr.instance.TriggerCutObstacle(LimitLevel);
            m_collider.enabled = false;
        }
    }
}
