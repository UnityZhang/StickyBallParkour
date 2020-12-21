using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BallGroup : MonoBehaviour
{
    public int XCount;
    public int YCount;
    public int HCount;

    public List<ball> m_Balls;
    BoxCollider m_collider;

    private void Awake()
    {
        m_collider = GetComponent<BoxCollider>();
    }
    /// <summary>
    /// 在游戏中用对象池重新生成
    /// </summary>
    public void ReCreateBallInGame()
    {
        ball[] b = transform.GetComponentsInChildren<ball>();
        for (int i = 0; i < b.Length; i++)
        {
            DestroyImmediate(b[i].gameObject);
        }
        CreateBall();
    }


    void CreateBall()
    {
        m_collider = GetComponent<BoxCollider>();
        m_Balls = new List<ball>();
        float space = GameMgr.instance.CreateBallSpace;
        GameObject prefab = BallPoolMgr.instance.BallPrefab;
        for (int i = 0; i < XCount; i++)
        {
            for (int j = 0; j < YCount; j++)
            {
                for (int k = 0; k < HCount; k++)
                {
                    ball ball = BallPoolMgr.instance.Spawn(-1);
                    ball.transform.SetParent(transform);
                    ball.rig.isKinematic = true;
                    m_Balls.Add(ball);

                    ball.transform.localPosition = new Vector3(j * space, k * space, i * space);
                }
            }
        }
    }
    [ContextMenu("生成Balls")]
    void CreateNewBall()
    {
        ball[] b = transform.GetComponentsInChildren<ball>();
        for (int i = 0; i < b.Length; i++)
        {
            DestroyImmediate(b[i].gameObject);
        }
        m_collider = GetComponent<BoxCollider>();
        m_Balls = new List<ball>();
        float space = GameMgr.instance.CreateBallSpace;
        GameObject prefab = BallPoolMgr.instance.BallPrefab;
        for (int i = 0; i < XCount; i++)
        {
            for (int j = 0; j < YCount; j++)
            {
                for (int k = 0; k < HCount; k++)
                {
                    GameObject obj = Instantiate(prefab, transform);
                    ball ball = obj.GetComponent<ball>();
                    ball.rig.isKinematic = true;
                    m_Balls.Add(ball);

                    obj.transform.localPosition = new Vector3(j * space, k * space, i * space);
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            m_collider.enabled = false;
            for (int i = 0; i < m_Balls.Count; i++)
            {
                m_Balls[i].rig.isKinematic = false;
                //给一个水平方向的力，不然散不开
                //Vector3 force = new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                Vector3 force = (other.transform.position - m_Balls[i].transform.position).normalized * 0.1f;
                m_Balls[i].rig.AddForce(force, ForceMode.Impulse);
            }
            GameMgr.instance.m_AllNotAdsorbBalls.AddRange(m_Balls);
        }
       
    }
}
