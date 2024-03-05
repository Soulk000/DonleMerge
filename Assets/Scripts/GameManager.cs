using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("------------[ Core ]------------")]
    public int maxLevel;
    public int score;
    public bool isOver;
    [Header("------------[ Oject Pooling ]------------")]
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    [Header("------------[ Audio ]------------")]
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx
    {
        LevelUp,
        Next,
        Attach,
        Button,
        Over
    };
    [Header("------------[ UI ]------------")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;
    public Text gumText;

    [Header("------------[ ETC ]------------")]
    public GameObject line;
    public GameObject bottom;
    public Image gemImage;
    int sfxCursor;

    // 在游戏开始时设置帧率为120帧
    private void Awake()
    {
        Application.targetFrameRate = 120;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }

        if(!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
        gumText.text = PlayerPrefs.GetInt("GemText").ToString();
    }

    // 游戏开始时调用，播放背景音乐并生成第一个Dongle对象
    public void GameStart()
    {
        line.SetActive(true);
        // bottom.SetActive(true);
        gemImage.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        gumText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextDongle", 1f);
    }

    Dongle MakeDongle()
    {
        // 使用Instantiate实例化一个新的effect对象，并设置其父物体为effectGroup
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        // 从实例化的对象获取ParticleSystem组件
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 使用Instantiate实例化一个新的Dongle对象，并设置其父物体为dongleGroup
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        // 从实例化的对象获取Dongle组件
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        // 初始化effect
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        // 返回实例化的Dongle对象
        return instantDongle;
    }
    // 生成一个新的Dongle对象
    Dongle GetDongle()
    {
        for(int index = 0; index < donglePool.Count; index++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }

        return MakeDongle();
    }

    // 生成下一个Dongle对象
    void NextDongle()
    {
        if (isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel - 2);

        // 激活新生成的Dongle对象
        lastDongle.gameObject.SetActive(true);

        // 播放下一个Dongle的音效
        SfxPlay(Sfx.Next);
        // 启动协程等待下一个Dongle生成
        StartCoroutine(WaitNext());
    }

    // 协程：等待上一个Dongle被销毁后生成下一个Dongle
    IEnumerator WaitNext()
    {
        // 循环等待lastDongle被销毁
        while (lastDongle != null)
        {
            yield return null;
        }

        // 等待2.5秒后生成下一个Dongle
        yield return new WaitForSeconds(1f);
        NextDongle();
    }

    // 当触摸按下时调用，触发当前Dongle的Drag方法
    public void TouchDown()
    {
        if (lastDongle == null)
        {
            return;
        }
        lastDongle.Drag();
    }

    // 当触摸抬起时调用，触发当前Dongle的Drop方法，并将lastDongle置为null
    public void TouchUp()
    {
        if (lastDongle == null)
        {
            return;
        }
        lastDongle.Drop();
        lastDongle = null;
    }

    // 游戏结束的方法
    public void GameOver()
    {
        if (isOver)
        {
            return;
        }

        isOver = true;

        // 启动协程处理游戏结束逻辑
        StartCoroutine(GameOverRoutine());
    }

    // 协程：处理游戏结束逻辑
    IEnumerator GameOverRoutine()
    {
        // 获取场景中所有Dongle对象
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // 停止所有Dongle对象的刚体模拟
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigId.simulated = false;
        }

        // 遍历所有Dongle对象
        for (int index = 0; index < dongles.Length; index++)
        {
            // 将Dongle对象隐藏，并移动到一个很高的位置（上方100个单位）
            dongles[index].Hide(Vector3.up * 100);

            // 等待0.1秒
            yield return new WaitForSeconds(0.1f);
        }

        // 等待0.1秒后播放游戏结束的音效
        yield return new WaitForSeconds(0.1f);

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        subScoreText.text = "Score: " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    // 播放音效的方法
    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        // 播放音效
        sfxPlayer[sfxCursor].Play();
        // 更新音效游标
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    private void Update() {
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }

    public void UpdateGem(int gemCount)
    {
        // 获取之前保存的 GemText 值，并加上 gemCount
        int newGemValue = PlayerPrefs.GetInt("GemText") + gemCount;

        // 将新的值保存到 PlayerPrefs
        PlayerPrefs.SetInt("GemText", newGemValue);

        // 将新的值赋给 gumText.text
        gumText.text = newGemValue.ToString();
    }
}