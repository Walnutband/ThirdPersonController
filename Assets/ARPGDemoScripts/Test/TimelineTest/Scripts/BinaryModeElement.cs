// using UnityEngine;
// using UnityEngine.UIElements;

// namespace ARPGDemo.Test.Timeline
// {
//     public class BinaryModeElement : VisualElement
//     {
//         // UXML 工厂 / Traits，使其在 UXML / UI Builder 可用
//         public new class UxmlFactory : UxmlFactory<BinaryModeElement, UxmlTraits> { }

//         public new class UxmlTraits : VisualElement.UxmlTraits
//         {
//             // 定义一个 asset 类型的 UXML 属性，名称为 "image"
//             readonly UxmlAssetAttributeDescription<Texture2D> m_ImageAttr =
//                 new UxmlAssetAttributeDescription<Texture2D> { name = "image" };

//             public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
//             {
//                 base.Init(ve, bag, cc);
//                 var node = (BinaryModeElement)ve;

//                 // 从 UXML bag 读取 Texture2D（若有），并赋值到控件属性
//                 var tex = m_ImageAttr.GetValueFromBag(bag, cc);
//                 if (tex != null)
//                 {
//                     node.Image = tex;
//                 }
//             }
//         }

//         // 运行时可通过 C# 访问的属性
//         public Texture2D Image
//         {
//             get => m_Image;
//             set
//             {
//                 if (m_Image == value) return;
//                 m_Image = value;
//                 UpdateBackgroundFromImage();
//             }
//         }
//         private Texture2D m_Image;

//         // 构造器（C# 创建时）
//         public BinaryModeElement()
//         {
//             // 初始样式或类名
//             AddToClassList("image-background-element");
//         }


//         void UpdateBackgroundFromImage()
//         {
//             if (m_Image != null)
//             {
//                 // 把 Texture2D 转为 StyleBackground（UI Toolkit 的样式类型）
//                 style.backgroundImage = new StyleBackground(m_Image);
//                 // 可选：设置 background-repeat / size 等
//                 style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Contain));
//                 // style.backgroundRepeat = new StyleEnum<BackgroundRepeat>(BackgroundRepeat.NoRepeat);
//             }
//             else
//             {
//                 // 清空背景
//                 style.backgroundImage = new StyleBackground();
//             }

//             // 触发重绘
//             MarkDirtyRepaint();
//         }
//     }
// }
