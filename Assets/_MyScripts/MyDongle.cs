using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class MyDongle : MonoBehaviour
{
    [SerializeField]
    private bool isDrag;
    Rigidbody2D rigId;
    CircleCollider2D circle;
    public int level;
    Animator anim;
    public bool isMerge;
    public MyGameManager manager;

    private void Awake()
    {
        rigId = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
    }

    // private void OnEnable() {
        // anim.SetInteger("Level", level);
    // }
    // Update is called once per frame
    void Update()
    {
        // 检查是否处于拖拽状态
        if (isDrag)
        {
            // 将屏幕坐标（鼠标位置）转换为世界坐标
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 计算物体在x轴上的左右边界，考虑物体的大小
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            // 限制物体在x轴上的移动范围
            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }

            // 将z坐标设为0，确保物体在2D平面上移动
            mousePos.y = 8;
            mousePos.z = 0;

            // 使用Vector3.Lerp平滑地移动物体到鼠标位置
            // transform.position 表示当前物体的位置
            // mousePos 表示目标位置
            // 0.1f 是插值的速度，较小的值会导致更平滑的移动，但可能需要更长的时间达到目标位置
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.1f);
        }
    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigId.simulated = true;
    }

    // 在碰撞持续期间调用
    void OnCollisionStay2D(Collision2D collision)
    {
        // 检查碰撞的游戏对象是否具有"Dongle"标签
        if (collision.gameObject.tag == "MyDongle")
        {
            // 获取碰撞对象的Dongle组件
            MyDongle other = collision.gameObject.GetComponent<MyDongle>();

            // 检查两个Dongle的级别（level）是否相同且未合并，并且级别小于7
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // 比较位置，如果当前Dongle在下方或者位置相同但x坐标更大
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    // 隐藏其他Dongle，并升级当前Dongle
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }
    }

    // 将Dongle隐藏的方法
    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        // 停止刚体模拟
        rigId.simulated = false;
        // 禁用CircleCollider2D
        circle.enabled = false;

        // 启动协程以平滑隐藏Dongle
        StartCoroutine(HideRoutine(targetPos));
    }

    // 协程：平滑隐藏Dongle
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        while (frameCount < 20)
        {
            frameCount++;
            // 使用Vector3.Lerp平滑地移动Dongle到目标位置
            transform.position = Vector3.Lerp(transform.position, targetPos, 1f);
            yield return null;
        }

        // 重置标志并禁用GameObject
        isMerge = false;
        gameObject.SetActive(false);
    }

    // 升级Dongle的方法
    void LevelUp()
    {
        isMerge = true;

        // 将刚体速度和角速度重置为零
        rigId.velocity = Vector2.zero;
        rigId.angularVelocity = 0;

        // 启动协程以处理升级动画
        StartCoroutine(LevelUpRoutine());
    }

    // 协程：处理Dongle升级动画
    IEnumerator LevelUpRoutine()
    {
        // 等待0.2秒
        yield return new WaitForSeconds(0.2f);

        // 设置动画中的级别
        // anim.SetInteger("Level", level + 1);

        // 等待0.3秒
        yield return new WaitForSeconds(0.3f);

        // 升级级别
        level++;

        // 更新GameManager中的最大级别
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        // 重置合并标志
        isMerge = false;
    }
}