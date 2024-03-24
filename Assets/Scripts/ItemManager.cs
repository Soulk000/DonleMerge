using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public GameManager gameManager;
    public Dongle dongle;
    public GameObject hitText;
    public Text bombText;
    public GameObject errorHintText;
    private bool useBomb = false;

    void Update()
    {
        if (useBomb == true)
        {
            // 检查鼠标左键是否被单击
            if (Input.GetMouseButtonDown(0))
            {
                UseBomb();
            }
        }
    }

    void UseBomb()
    {
        // 将屏幕坐标转换为世界坐标
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        // 进行2D射线检测
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        // 如果射线击中了带有"Dongle"标签的对象
        if (hit.collider != null && hit.collider.CompareTag("Dongle"))
        {
            gameManager.bombCount -= 1;
            PlayerPrefs.SetInt("BombCount", gameManager.bombCount);
            gameManager.bombText.text = gameManager.bombCount.ToString();
            hit.collider.gameObject.SetActive(false);
            // Destroy(hit.collider.gameObject);
            StartCoroutine(MakeLive());
            useBomb = false;
        }
    }
    public void UseItem(string ItemName)
    {
        switch (ItemName)
        {
            case "Bomb":
                hitText.SetActive(false);
                useBomb = true;
                break;
            default:
                break;
        }
    }

    IEnumerator MakeLive()
    {
        yield return new WaitForSeconds(0.5f);
        gameManager.isLive = true;
    }

    public void BuyItem()
    {
        if (gameManager.gemCount < 5)
        {
            gameManager.buyItemText.SetActive(false);
            StartCoroutine(ShowErrorHint());
            return;
        }
        gameManager.bombCount += 1;
        PlayerPrefs.SetInt("bombText", gameManager.bombCount);
        gameManager.bombText.text = gameManager.bombCount.ToString();
        gameManager.gemCount -= 5;
        PlayerPrefs.SetInt("GemText", gameManager.gemCount);
        gameManager.gumText.text = gameManager.gemCount.ToString();
        gameManager.buyItemText.SetActive(false);
        StartCoroutine(MakeLive());
    }

    IEnumerator ShowErrorHint()
    {
        errorHintText.SetActive(true);
        gameManager.isLive = true;
        yield return new WaitForSeconds(1.5f);
        errorHintText.SetActive(false);
    }
}
