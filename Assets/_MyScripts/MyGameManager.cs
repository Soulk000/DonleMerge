using UnityEngine;
using System.Collections;

public class MyGameManager : MonoBehaviour
{
    // 当前生成的最新的Dongle对象
    public MyDongle lastDongle;

    // Dongle预制体，用于实例化新的Dongle对象
    public GameObject donglePrefab;

    // 用于组织Dongle对象的父物体的Transform
    public Transform dongleGroup;
    public int maxLevel;

    // 在游戏开始时设置帧率为120帧
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }

    // 游戏开始时调用，生成第一个Dongle对象
    void Start()
    {
        NextDongle();
    }

    // 生成一个新的Dongle对象
    MyDongle GetDongle()
    {
        // 使用Instantiate实例化一个新的Dongle对象，并设置其父物体为dongleGroup
        GameObject instant = Instantiate(donglePrefab, dongleGroup);

        // 从实例化的对象获取Dongle组件
        MyDongle instantDongle = instant.GetComponent<MyDongle>();

        // 返回实例化的Dongle对象
        return instantDongle;
    }

    // 生成下一个Dongle对象
    void NextDongle()
    {
        // 获取新的Dongle对象
        MyDongle newDongle = GetDongle();

        // 将新的Dongle对象设置为lastDongle，并为其设置一个随机的level
        lastDongle = newDongle;
				// 初始化manager;
        lastDongle.manager = this;
        lastDongle.level = Random.Range(0, maxLevel);

        // 激活新生成的Dongle对象
        lastDongle.gameObject.SetActive(true);

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
        yield return new WaitForSeconds(2.5f);
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
}