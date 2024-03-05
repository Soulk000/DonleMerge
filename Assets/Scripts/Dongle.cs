using System.Collections;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    [SerializeField]
    private bool isDrag;
    public bool isMerge;
    public GameManager manager;
    public ParticleSystem effect;
    public int level;
    public Rigidbody2D rigId;
    PolygonCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;
    bool isAttach;

    private void Awake()
    {
        rigId = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        rigId.simulated = false;
        rigId.velocity = Vector2.zero;
        rigId.angularVelocity = 0;
        circle.enabled = true;
    }

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
            mousePos.y = 7;
            mousePos.z = 0;

            // 使用Vector3.Lerp平滑地移动物体到鼠标位置
            // transform.position 表示当前物体的位置
            // mousePos 表示目标位置
            // 0.1f 是插值的速度，较小的值会导致更平滑的移动，但可能需要更长的时间达到目标位置
            transform.position = Vector3.Lerp(transform.position, mousePos, 1f);
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

    // 当发生碰撞时调用
    private void OnCollisionEnter2D(Collision2D other)
    {
        StartCoroutine(AttachRoutine());
    }

    // 协程：处理附着逻辑
    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }
        isAttach = true;

        // 播放附着音效
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    // 在碰撞持续期间调用
    void OnCollisionStay2D(Collision2D collision)
    {
        // 检查碰撞的游戏对象是否具有"Dongle"标签
        if (collision.gameObject.tag == "Dongle")
        {
            // 获取碰撞对象的Dongle组件
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // 检查两个Dongle的级别（level）是否相同且未合并，并且级别小于8
            if (level == other.level && !isMerge && !other.isMerge && level < 8)
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

        if (targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }
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
            if (targetPos != Vector3.up * 100)
            {
                // 使用Vector3.Lerp平滑地移动Dongle到目标位置
                transform.position = Vector3.Lerp(transform.position, targetPos, 1f);
            }
            else if (targetPos == Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, Vector3.zero, 1f);
            }
            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

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
        LevelUpRoutine();
    }

    // 协程：处理Dongle升级动画
    void LevelUpRoutine()
    {
        // 设置动画中的级别
        anim.SetInteger("Level", level + 1);

        // 升级级别
        level++;

        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);
        if (level >= 7)
        {
            if (level == 7) {
                manager.UpdateGem(1);
            } else {
                manager.UpdateGem(2);
            }
        }
        // 更新GameManager中的最大级别
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        // 重置合并标志
        isMerge = false;
    }

    // 当触发器内有碰撞持续时调用
    void OnTriggerStay2D(Collider2D collision)
    {
        // 检查碰撞对象的标签是否为"Finish"
        if (collision.tag == "Finish")
        {
            // 增加死亡时间（deadTime），以秒为单位
            deadTime += Time.deltaTime;

            // 如果死亡时间大于2秒，改变物体的颜色为深红色
            if (deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }

            // 如果死亡时间大于5秒，调用GameManager中的GameOver方法
            if (deadTime > 5)
            {
                manager.GameOver();
            }
        }
    }

    // 当离开触发器时调用
    void OnTriggerExit2D(Collider2D collision)
    {
        // 检查碰撞对象的标签是否为"Finish"
        if (collision.tag == "Finish")
        {
            // 重置死亡时间为0
            deadTime = 0;

            // 恢复物体的颜色为白色
            spriteRenderer.color = Color.white;
        }
    }
    void EffectPlay()
    {
        // 将特效的位置设置为当前对象的位置
        effect.transform.position = transform.position;
        // 将特效的大小设置为当前对象的大小
        effect.transform.localScale = transform.localScale;
        // 播放特效
        effect.Play();
    }
}
