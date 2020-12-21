using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

public class GameMgr : SingletonMonoBehaviour<GameMgr>
{
   
    public Transform Player;
    //public GameObject BallPrefab;
    

    public float PlayerLeftRightSpeed = 1f;
    public float PlayerForwardSpeed = 1f;
    public float PlayerLeftRightRotateSpeed = 0.1f;
    public float PlayerForwardRotateSpeed = 0.1f;
    public float PlayerJumpHeight = 0.4f;
    
    public float CreateBallSpace = 0.1f;

    public List<BallGroup> BallGroups;
    public List<LevelState> LevelStates;

    [SerializeField]
    int m_CurrentLevel;
    bool GameStart = false;
    bool NextAddAnimCompleteGetBigger = false;

    [HideInInspector]
    public List<ball> m_AllNotAdsorbBalls = new List<ball>();
    List<Vector3> m_tempList = new List<Vector3>();
    //当前player身上拥有的球
    List<ball> m_CurrentAdsorbBalls = new List<ball>();

    Vector3 m_PlayerOriginScale;
    /// <summary>
    /// 碰到障碍物
    /// </summary>
    public void TriggerObstacle()
    {
        FixedFollowCamera.Instance.Shake();
        //起跳
        float originY = Player.transform.position.y;
        LeanTween.moveLocalY(Player.gameObject,originY + PlayerJumpHeight,0.3f).setEaseOutQuad().setOnComplete(()=> 
        {
            LeanTween.moveLocalY(Player.gameObject, originY, 0.3f).setEaseInQuad();
        });
        //减去身上的球
        RemoveBallToSmooth();
    }

    public void TriggerCutObstacle(int limitLv)
    {
        if (m_CurrentLevel >= limitLv)
        {
            if (m_CurrentLevel > limitLv || LevelStates[m_CurrentLevel].CurrentBallCount > 0)
                FixedFollowCamera.Instance.Shake();

            if (LevelStates[m_CurrentLevel].CurrentBallCount == 0)
            {
                for (int i = m_CurrentLevel; i > limitLv; i--)
                {
                    RemoveBallToSmooth();
                }
            }
            else
            {
                for (int i = m_CurrentLevel; i >= limitLv; i--)
                {
                    RemoveBallToSmooth();
                }
            }
        }
        //Debug.LogError(m_CurrentLevel + "  " + limitLv);
    }

    void Start()
    {
        BallPoolMgr.instance.InitPool();
        StartGame();
    }

    void StartGame()
    {
        LeanTween.init(2000);
        m_PlayerOriginScale = Player.localScale;
        m_CurrentLevel = 0;
        foreach (var item in BallGroups)
        {
            item.ReCreateBallInGame();
        }
        for (int i = 0; i < LevelStates.Count; i++)
        {
            CalualteRandomPoint(LevelStates[i]);
        }
        GameStart = true;
    }


    private void Update()
    {
        if (GameStart)
        {
            float speed = PlayerForwardSpeed * Time.deltaTime;
            Player.Translate(Vector3.right * speed, Space.World);
            Player.Rotate(-Vector3.forward, PlayerForwardRotateSpeed * speed, Space.World);

            if (Input.GetMouseButton(0))
            {
                float offset = Input.GetAxis("Mouse X");//获取鼠标x轴的偏移量
                float speed1 = offset * PlayerLeftRightSpeed * Time.deltaTime;
                Player.Translate(-Vector3.forward * speed1, Space.World);
                if (Player.position.z >= 1.1f)
                {
                    Player.position = new Vector3(Player.position.x, Player.position.y, 1.1f);
                }
                else if (Player.position.z <= -1.1f)
                {
                    Player.position = new Vector3(Player.position.x, Player.position.y, -1.1f);
                }
                else
                {
                    Player.Rotate(-Vector3.right * speed1 * PlayerLeftRightRotateSpeed, Space.World);
                }
            }

            for (int i = m_AllNotAdsorbBalls.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(Player.position, m_AllNotAdsorbBalls[i].transform.position) <= LevelStates[m_CurrentLevel].AdsorbDis)
                {
                    m_AllNotAdsorbBalls[i].rig.isKinematic = true;
                    m_AllNotAdsorbBalls[i].m_collider.enabled = false;
                    AdsorbBall(m_AllNotAdsorbBalls[i]);
                    m_AllNotAdsorbBalls.RemoveAt(i);
                }
                //将不可能收集到的球移除
                else if ((Player.position.x - m_AllNotAdsorbBalls[i].transform.position.x) > 6)
                {
                    m_AllNotAdsorbBalls[i].DelayDeSpawn();
                    m_AllNotAdsorbBalls.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 吸附一个球
    /// </summary>
    /// <param name="ball"></param>
    void AdsorbBall(ball ball)
    {
        if (LeanTween.isTweening(ball.gameObject))
            LeanTween.cancel(ball.gameObject);

        //这里可能会出现球正在吸附过程中碰到障碍物又将球打掉的问题（未处理）
        Vector3 pos = GetBallRandomPos(m_CurrentLevel);
        AddBall(ball, pos);
        //吸附的tween
        //缩放换算 保持大小 用当前的等级计算，否则因为tween导致计算延迟大小错误
        float curScale = ball.originScale.x / (m_PlayerOriginScale.x * Mathf.Pow(1 + rate, m_CurrentLevel));
        Vector3 scale = Vector3.one * curScale;

        LeanTween.moveLocal(ball.gameObject, pos, 0.8f).setEaseOutExpo().setOnComplete(CheckGetBigger);
    }
    
    /// <summary>
    /// 增加球
    /// </summary>
    void AddBall(ball ball, Vector3 pos)
    {
        m_CurrentAdsorbBalls.Add(ball);
        ball.transform.SetParent(Player);
        LevelStates[m_CurrentLevel].CurrentBallCount++;
        LevelStates[m_CurrentLevel].DicRandomPos[pos] = ball;
        //升级
        if (LevelStates[m_CurrentLevel].CurrentBallCount >= LevelStates[m_CurrentLevel].BallCount)
        {
            m_CurrentAdsorbBalls.Clear();
            m_CurrentLevel++;
            NextAddAnimCompleteGetBigger = true;
        }
    }

    List<Vector3> m_TempList = new List<Vector3>();
    /// <summary>
    /// 减球 减到光滑（完整的一层），如果本身就是光滑球，就减去一整层  
    /// </summary>
    void RemoveBallToSmooth()
    {
        //Debug.LogError(LevelStates[m_CurrentLevel].CurrentBallCount);
        m_TempList.Clear();
        if (LevelStates[m_CurrentLevel].CurrentBallCount == 0)
        {
            m_CurrentLevel--;
            if (m_CurrentLevel < 0)
            {
                GameOver();
                return;
            }
            ChangeBody(false);
        }
        foreach (var item in LevelStates[m_CurrentLevel].DicRandomPos)
        {
            if (item.Value.IsNotNull())
            {
                item.Value.rig.isKinematic = false;
                item.Value.m_collider.enabled = true;
                m_TempList.Add(item.Key);
            }
        }

        foreach (var item in m_TempList)
        {
            ball b = LevelStates[m_CurrentLevel].DicRandomPos[item];
            if (!b.gameObject.activeSelf)
                b.gameObject.SetActive(true);

            b.transform.SetParent(BallPoolMgr.instance._ballPool.transform);
            b.SetScaleOrigin();
            //给爆炸力散开
            int force = Random.Range(100, 220);
            b.rig.AddExplosionForce(force, Player.position, 4f);
            //延迟0.4s使其又可以被收集
            LeanTween.delayedCall(0.4f, () =>
            {
                m_AllNotAdsorbBalls.Add(b);
            });

            LevelStates[m_CurrentLevel].DicRandomPos[item] = null;
        }
        LevelStates[m_CurrentLevel].CurrentBallCount = 0;
        m_CurrentAdsorbBalls.Clear();
    }
    
    /// <summary>
    /// 随机得到一个空位置
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    Vector3 GetBallRandomPos(int layer)
    {
        m_tempList.Clear();
        foreach (var item in LevelStates[layer].DicRandomPos)
        {
            if (item.Value.IsNull())
            {
                m_tempList.Add(item.Key);
            }
        }
        if (m_tempList.Count == 0)
        {
            return Vector3.zero;
        }
        int index = Random.Range(0, m_tempList.Count);
        return m_tempList[index];
    }

    /// <summary>
    /// 升级的行为
    /// </summary>
    void CheckGetBigger()
    {
        if (NextAddAnimCompleteGetBigger)
        {
            //身上的球都消失
            int layer = m_CurrentLevel - 1;
            foreach (var item in LevelStates[layer].DicRandomPos)
            {
                //item.Value.DelayDeSpawn();
                item.Value.gameObject.SetActive(false);
            }
            //球变大 
            ChangeBody(true);
            NextAddAnimCompleteGetBigger = false;
        }
    }
    float rate = 0.17f;
    void ChangeBody(bool getBigger)
    {
        float height = 0.035f;

        //修改大球缩放之前将小球都移出父物体，否则小球会跟随大球一起变大
        foreach (var item in m_CurrentAdsorbBalls)
            item.transform.SetParent(null);

        if (getBigger)
            Player.localScale *= (1 + rate);
        else
            Player.localScale *= (1 - rate);
        //还原
        foreach (var item in m_CurrentAdsorbBalls)
            item.transform.SetParent(Player);

        if (getBigger)
            Player.transform.position += Vector3.up * height;
        else
            Player.transform.position -= Vector3.up * 0.035f;
    }
    
    /// <summary> 
    /// 球体表面平均分割点  
    /// </summary> 
    void CalualteRandomPoint(LevelState level)
    {
        float inc = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
        float off = 2.0f / level.BallCount;
        //注意保持数值精度  
        for (int i = 0; i < level.BallCount; i++)
        {
            float y = (float)i * off - 1.0f + (off / 2.0f);
            float r = Mathf.Sqrt(1.0f - y * y);
            float phi = i * inc;
            Vector3 pos = new Vector3(Mathf.Cos(phi) * r * level.Radius, y * level.Radius, Mathf.Sin(phi) * r * level.Radius);
            Vector3 posOffset = new Vector3(Random.Range(-0.007f, 0.007f), Random.Range(-0.007f, 0.007f), Random.Range(-0.007f, 0.007f));
            pos += posOffset;
            level.DicRandomPos.Add(pos, null);
            level.IsFull = false;
            level.CurrentBallCount = 0;
        }
    }

    /// <summary>
    /// 游戏失败
    /// </summary>
    public void GameOver()
    {
        Debug.LogError("游戏结束");
    }

    //private void OnDrawGizmos()
    //{
    //    if (m_DicFirstLayerRandomPos == null)
    //        return;
    //    Gizmos.color = Color.red;
    //    foreach (var item in m_DicFirstLayerRandomPos)
    //    {
    //        if (item.Value.IsNotNull())
    //        {
    //            Gizmos.DrawSphere(item.Key, 0.07f);
    //        }
    //    }
    //}



    //public Vector3 GetBallRandom(Vector2 random)
    //{
    //    float phi = 2 * Mathf.PI * random.x;
    //    float cosTheta = 1 - 2 * random.y;
    //    float sinTheta = Mathf.Sqrt(1 - cosTheta * cosTheta);

    //    return new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), cosTheta);
    //}

}

[System.Serializable]
public class LevelState
{
    /// <summary> 
    /// 大球半径  用来计算小球吸附位置
    /// </summary> 
    public float Radius = 0.128f;
    /// <summary>
    /// 吸附距离
    /// </summary>
    public float AdsorbDis = 0.3f;
    /// <summary> 
    /// 吸附数量
    /// </summary> 
    public int BallCount = 240;

    public Dictionary<Vector3, ball> DicRandomPos = new Dictionary<Vector3, ball>();

    [HideInInspector]
    public bool IsFull;

    [HideInInspector]
    public int CurrentBallCount;
}
