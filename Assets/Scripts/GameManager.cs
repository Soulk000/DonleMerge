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
    public GameObject hitText;
    public GameObject buyItemText;
    public Text bombText;

    [Header("------------[ ETC ]------------")]
    public GameObject line;
    public GameObject bottom;
    public Image gemImage;
    public bool isLive;
    public int bombCount;
    public int gemCount;
    int sfxCursor;

    private bool isPaused = false;
    private int maxScore;

    private void Awake()
    {
        Application.targetFrameRate = 120;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }

        LoadSavedData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;

        if (pauseStatus)
        {
            // 游戏暂停时保存数据
            SaveGameData();
        }
    }

    void LoadSavedData()
    {
        if(!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScore = PlayerPrefs.GetInt("MaxScore");
        maxScoreText.text = maxScore.ToString();

        if(!PlayerPrefs.HasKey("GemText"))
        {
            PlayerPrefs.SetInt("GemText", 0);
        }
        gemCount = PlayerPrefs.GetInt("GemText");
        gumText.text = PlayerPrefs.GetInt("GemText").ToString();

        if(!PlayerPrefs.HasKey("BombCount"))
        {
            PlayerPrefs.SetInt("BombCount", bombCount);
        }
        bombCount = PlayerPrefs.GetInt("BombCount");
        bombText.text = bombCount.ToString();
    }

    public void GameStart()
    {
        line.SetActive(true);
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
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

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

    void NextDongle()
    {
        if (isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel - 2);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null || isLive == false || isPaused)
        {
            return;
        }
        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null || isLive == false || isPaused)
        {
            return;
        }
        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isOver)
        {
            return;
        }

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigId.simulated = false;
        }

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 100);

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f);

        maxScore = Mathf.Max(score, maxScore);
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

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) // 按下"Esc"键
        {
            SaveGameData(); // 保存游戏数据
            Application.Quit(); // 退出游戏
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }

    public void UpdateGem(int innerGemCount)
    {
        int newGemValue = PlayerPrefs.GetInt("GemText") + innerGemCount;
        PlayerPrefs.SetInt("GemText", newGemValue);
        gemCount = newGemValue;
        gumText.text = newGemValue.ToString();
    }

    public void CheckUseItem(bool checkUseItem)
    {
        if (bombCount <= 0)
        {
            CheckBuyItem(true);
            return;
        }
        isLive = !checkUseItem;
        if (checkUseItem == true) {
            hitText.SetActive(true);
        } else {
            isLive = true;
            hitText.SetActive(false);
        }
    }

    public void CheckBuyItem(bool checkBuyItem)
    {
        isLive = !checkBuyItem;
        if (checkBuyItem == true) {
            buyItemText.SetActive(true);
        } else {
            isLive = true;
            buyItemText.SetActive(false);
        }
    }

    private void SaveGameData()
    {
        PlayerPrefs.SetInt("MaxScore", maxScore);
        PlayerPrefs.SetInt("GemText", int.Parse(gumText.text));
        PlayerPrefs.SetInt("BombCount", bombCount);
    }
}
