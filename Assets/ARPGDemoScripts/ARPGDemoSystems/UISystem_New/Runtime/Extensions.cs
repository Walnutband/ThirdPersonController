
using ARPGDemo.UISystem_New;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem
{
    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottonCenter,
        BottomRight,
        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }

    public enum PivotPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public static class UIExtension
    {
        public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
        {
            source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

            switch (allign)
            {
                case (AnchorPresets.TopLeft):
                    {
                        source.anchorMin = new Vector2(0, 1);
                        source.anchorMax = new Vector2(0, 1);
                        break;
                    }
                case (AnchorPresets.TopCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 1);
                        source.anchorMax = new Vector2(0.5f, 1);
                        break;
                    }
                case (AnchorPresets.TopRight):
                    {
                        source.anchorMin = new Vector2(1, 1);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }

                case (AnchorPresets.MiddleLeft):
                    {
                        source.anchorMin = new Vector2(0, 0.5f);
                        source.anchorMax = new Vector2(0, 0.5f);
                        break;
                    }
                case (AnchorPresets.MiddleCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0.5f);
                        source.anchorMax = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (AnchorPresets.MiddleRight):
                    {
                        source.anchorMin = new Vector2(1, 0.5f);
                        source.anchorMax = new Vector2(1, 0.5f);
                        break;
                    }

                case (AnchorPresets.BottomLeft):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(0, 0);
                        break;
                    }
                case (AnchorPresets.BottonCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0);
                        source.anchorMax = new Vector2(0.5f, 0);
                        break;
                    }
                case (AnchorPresets.BottomRight):
                    {
                        source.anchorMin = new Vector2(1, 0);
                        source.anchorMax = new Vector2(1, 0);
                        break;
                    }

                case (AnchorPresets.HorStretchTop):
                    {
                        source.anchorMin = new Vector2(0, 1);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }
                case (AnchorPresets.HorStretchMiddle):
                    {
                        source.anchorMin = new Vector2(0, 0.5f);
                        source.anchorMax = new Vector2(1, 0.5f);
                        break;
                    }
                case (AnchorPresets.HorStretchBottom):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(1, 0);
                        break;
                    }

                case (AnchorPresets.VertStretchLeft):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(0, 1);
                        break;
                    }
                case (AnchorPresets.VertStretchCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0);
                        source.anchorMax = new Vector2(0.5f, 1);
                        break;
                    }
                case (AnchorPresets.VertStretchRight):
                    {
                        source.anchorMin = new Vector2(1, 0);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }

                case (AnchorPresets.StretchAll):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(1, 1);
                        source.SetPivot(PivotPresets.MiddleCenter);
                        source.anchoredPosition = Vector2.zero;
                        source.sizeDelta = Vector2.zero;
                        break;
                    }
            }
        }

        public static void SetPivot(this RectTransform source, PivotPresets preset)
        {

            switch (preset)
            {
                case (PivotPresets.TopLeft):
                    {
                        source.pivot = new Vector2(0, 1);
                        break;
                    }
                case (PivotPresets.TopCenter):
                    {
                        source.pivot = new Vector2(0.5f, 1);
                        break;
                    }
                case (PivotPresets.TopRight):
                    {
                        source.pivot = new Vector2(1, 1);
                        break;
                    }

                case (PivotPresets.MiddleLeft):
                    {
                        source.pivot = new Vector2(0, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleRight):
                    {
                        source.pivot = new Vector2(1, 0.5f);
                        break;
                    }

                case (PivotPresets.BottomLeft):
                    {
                        source.pivot = new Vector2(0, 0);
                        break;
                    }
                case (PivotPresets.BottomCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0);
                        break;
                    }
                case (PivotPresets.BottomRight):
                    {
                        source.pivot = new Vector2(1, 0);
                        break;
                    }
            }
        }

        /*Tip：这些创建方法让我感到迷惑的原因在于，每一个对象（GO和组件）及其属性都需要逐一写明，很容易遗漏。*/

        /// <summary>
        /// 为对应的UI层创建对应的画布。这里是从无到有创建Canvas对象的一个完整过程
        /// </summary>
        /// <returns>返回创建的UILayer游戏对象挂载的Canvas组件</returns>
        public static Canvas CreateLayerCanvas(UILayerType layer, bool is3D, Transform parent, Camera camera, float width, float height)
        {
            GameObject canvasGo = new GameObject(layer.ToString()); //直接以枚举常量名作为游戏对象名
            RectTransform rectTransform = canvasGo.AddComponent<RectTransform>();
            rectTransform.SetParentEx(parent); //通常以UIRoot对象为父对象。注意这里是一个扩展方法，除了SetParent以外，还会将三个局部值都进行初始化。
            //锚点扩展到四个对角。
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            //设置层级，用于相机剔除。UI相机只显示UI层级的对象，3DUI就是Default层级与场景对象一致。
            canvasGo.layer = is3D ? Layer.Default : Layer.UI; 
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = is3D ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;
            //设置渲染排序
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)layer;
            canvas.worldCamera = camera; //这应该就是在检视器中看到的渲染相机的对应属性，只不过定义在C++中。
            canvas.pixelPerfect = false;
            CanvasScaler canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            //缩放模式，随屏幕尺寸缩放，重点是设置好锚点
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //参考分辨率，要看做什么游戏，PC的话大概就是1920*1080
            canvasScaler.referenceResolution = new Vector2(width, height);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; //扩展方式进行缩放。
            //投射射线
            canvasGo.AddComponent<GraphicRaycaster>(); 

            return canvas;
        }

        //TODO：默认2D，而3DUI另外处理。
        public static Canvas CreatePanelCanvas(UIPanelType panel, Transform parent, Camera camera, float width, float height)
        {
            GameObject canvasGo = new GameObject(panel.ToString()); //直接以枚举常量名作为游戏对象名
            RectTransform rectTransform = canvasGo.AddComponent<RectTransform>();
            rectTransform.SetParentEx(parent); //通常以UIRoot对象为父对象。注意这里是一个扩展方法，除了SetParent以外，还会将三个局部值都进行初始化。
            //锚点扩展到四个对角。
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            //设置层级，用于相机剔除。UI相机只显示UI层级的对象，3DUI就是Default层级与场景对象一致。
            canvasGo.layer = Layer.UI;
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            //设置渲染排序
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)panel;
            canvas.worldCamera = camera; //这应该就是在检视器中看到的渲染相机的对应属性，只不过定义在C++中。
            canvas.pixelPerfect = false;
            CanvasScaler canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            //缩放模式，随屏幕尺寸缩放，重点是设置好锚点
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //参考分辨率，要看做什么游戏，PC的话大概就是1920*1080
            canvasScaler.referenceResolution = new Vector2(width, height);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; //扩展方式进行缩放。
            //投射射线
            canvasGo.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        public static CanvasGroup CreateBlackMask(Transform parent, float alpha = 0, string name = null)
        {
            GameObject maskGo = new GameObject("Black Mask");
            RectTransform rectTransform = maskGo.AddComponent<RectTransform>();
            rectTransform.SetParentEx(parent);
            //通常是完全填充Canvas
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            //Image显示内容，CanvasGroup调整alpha。
            Image image = maskGo.AddComponent<Image>();
            image.color = Color.black; //往往就是纯黑色
            image.raycastTarget = false;
            CanvasGroup canvasGroup = maskGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alpha; //黑色图片，完全透明，插值调整alpha就可以实现渐变效果了
            if (name != null)
                canvasGroup.name = name; //应该改变的游戏对象名
            return canvasGroup;
        }
    }
}