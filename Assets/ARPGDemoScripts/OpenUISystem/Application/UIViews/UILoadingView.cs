using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace SkierFramework
{
    public class UILoadingView : UIView
    {
        #region 控件绑定变量声明，自动生成请勿手改
#pragma warning disable 0649
        [ControlBinding]
        private Slider Slider;
        [ControlBinding]
        private TextMeshProUGUI TextDes;
        [ControlBinding]
        private TextMeshProUGUI TextValue;

#pragma warning restore 0649
        #endregion

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Reset();
            Slider.onValueChanged.AddListener((value) =>
            {
                //这里格式字符串，0表示第一个参数，冒号后面F代表固定小数点格式、0表示以 0 位小数显示，即不显示小数部分（会四舍五入到最接近的整数）。
                TextValue.text = string.Format("{0:F0}%", value * 100);
            });
        }

        public void SetLoading(float value, string desc)
        {
            Slider.DOValue(value, 0.3f);
            TextDes.text = desc;
        }

        public override void OnClose()
        {
            base.OnClose();
            Reset();
        }

        public void Reset()
        {
            Slider.value = 0;
            TextDes.text = "";
            TextValue.text = "0%";
        }
    }
}
