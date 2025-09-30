using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MyCustomEditor : EditorWindow
{
    [SerializeField] private int m_SelectedIndex = -1; //存储当前选中元素的索引
    private VisualElement m_RightPane; //位于窗口内部右边的显示区域

    [MenuItem("Window/UI Toolkit/示例/查找并显示Sprite资产")]
    public static void ShowMyEditor()
    {
        // This method is called when the user selects the menu item in the Editor.
        EditorWindow wnd = GetWindow<MyCustomEditor>();
        wnd.titleContent = new GUIContent("显示精灵资产");

        // Limit size of the window.
        wnd.minSize = new Vector2(450, 200);
        wnd.maxSize = new Vector2(1920, 720);
  }

  public void CreateGUI()
  {
      // Get a list of all sprites in the project.
      //查找所有Sprite资源文件的GUID，不包括第三方库的资源（放在Library文件夹中的缓存），主要是Assets中的内容，不过包括自定义库，比如在Packages文件夹中的内容
      var allObjectGuids = AssetDatabase.FindAssets("t:Sprite"); 
      var allObjects = new List<Sprite>();
      foreach (var guid in allObjectGuids) //无法直接加载资源文件本身，所以通过GUID---Path---Asset这样的方式来获取所有指定类型的资源文件
      {
        allObjects.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
      }

      // Create a two-pane view with the left pane being fixed.
      var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

      // Add the panel to the visual tree by adding it as a child to the root element.
      rootVisualElement.Add(splitView);

      // A TwoPaneSplitView always needs two child elements.
      var leftPane = new ListView();
      splitView.Add(leftPane);
      m_RightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
      splitView.Add(m_RightPane);

      // Initialize the list view with all sprites' names.
      leftPane.makeItem = () => new Label(); //构建列表中的单个元素（每个标签显示对应的Sprite文件的命名）
      leftPane.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; }; //两个参数，当前元素和它的索引
      leftPane.itemsSource = allObjects; //数据源，ListView 会根据 itemsSource 动态生成并管理 UI 元素。

      // React to the user's selection.响应用户的选择
      leftPane.selectionChanged += OnSpriteSelectionChange;

      // Restore the selection index from before the hot reload.在热重载之前存储选择的元素索引
      leftPane.selectedIndex = m_SelectedIndex;

      // Store the selection index when the selection changes.每次改变选择都要存储索引到成员m_SelectedIndex中
      leftPane.selectionChanged += (items) => { m_SelectedIndex = leftPane.selectedIndex; };
  }

    /*IEnumerable<object>表示一个支持迭代的对象集合，可以使用foreach进行枚举，即遍历
    IEnumerable<object> 的另一个优势是可以实现惰性枚举（即按需生成元素）。
    如果你传入一个使用 yield return 的方法生成的集合，元素只有在访问时才会被实际创建或计算，提高了效率。
    */
    private void OnSpriteSelectionChange(IEnumerable<object> selectedItems) 
    {
      // Clear all previous content from the pane.清空之前的显示内容
      m_RightPane.Clear();

      var enumerator = selectedItems.GetEnumerator(); //返回一个迭代器（枚举器）
      if (enumerator.MoveNext())
      {
          var selectedSprite = enumerator.Current as Sprite; //从基类object到具体类型
          if (selectedSprite != null)
          {
              // Add a new Image control and display the sprite.添加一个新的Image控件，分配引用，然后添加到右面板中即显示
              var spriteImage = new Image();
              spriteImage.scaleMode = ScaleMode.ScaleToFit;
              spriteImage.sprite = selectedSprite;

              // Add the Image control to the right-hand pane.
              m_RightPane.Add(spriteImage);
          }
      }
  }
}