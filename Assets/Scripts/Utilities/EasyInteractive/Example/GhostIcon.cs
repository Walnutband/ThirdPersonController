using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 鼠标拖拽图标
/// </summary>
public class GhostIcon : MonoBehaviour
{
    public static GhostIcon Instance; //单例，就是接收当前拖拽对象的图标并显示于鼠标位置
    private Image _icon; //引用当前显示的图片
    private bool _isShow = false; //是否跟随鼠标移动，并非是否显示，是否显示由

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        _icon = GetComponent<Image>();
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (_isShow)
        {
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        }
    }
    public void ShowGhostIcon(Sprite sprite)  //传入图片，在鼠标处显示
    {
        if (sprite == null) return;
        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        _icon.sprite = sprite;
        gameObject.SetActive(true);
        _isShow = true;
    }

    public void HideGhostIcon()
    {//还可以再加一个清空Image组件所引用的图片
        _isShow = false;
        gameObject.SetActive(false);
    }
}
