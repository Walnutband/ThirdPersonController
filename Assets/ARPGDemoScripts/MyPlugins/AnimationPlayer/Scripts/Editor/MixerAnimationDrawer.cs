
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyPlugins.AnimationPlayer.EditorSection
{
//     // [CustomPropertyDrawer(typeof(MixerAnimation.Motion))]
//     // public class MotionDrawer : PropertyDrawer
//     // {
//     //     public override VisualElement CreatePropertyGUI(SerializedProperty property)
//     //     {

//     //     }
//     // }

    [CustomPropertyDrawer(typeof(MixerAnimation))]
    public class MixerAnimationDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Debug.Log("CreatePropertyGUI");
            //按照默认方式绘制
            return new PropertyField(property);

            // VisualElement root = new VisualElement();
            // // Debug.Log("MixerAnimationDrawer");
            // // Debug.Log($"SerializedProperty: {property}");
            // // var motions = property.FindPropertyRelative("m_Motions.Array");
            // SerializedProperty prop = property.Copy();
            // var motions = prop.FindPropertyRelative("m_Motions");
            // int size = motions.arraySize;
            // // Debug.Log($"motions名字：{motions.name}");
            // // prop.NextVisible(true);
            // // Debug.Log($"下一个元素的名字：{prop.name}\n是否等于motions：{SerializedProperty.EqualContents(prop, motions)}");
            // //Tip：尬住了，这里返回的是null?结构体而非引用类型，所以!=null始终为true，而应该直接使用!来表示非空。但是这里其实应该判为true。
            // while (prop.NextVisible(true) && !SerializedProperty.EqualContents(prop, motions))
            // {//就是将m_Motions字段以前的所有可见的序列化字段全部默认绘制出来
            //     // Debug.Log("绘制其他属性");
            //     root.Add(new PropertyField(prop));
            // }

            // // prop.serializedObject
            // var foldout = new Foldout { text = motions.displayName, value = true };
            // root.Add(foldout);
            // //m_Motions的专门逻辑
            // for (int i = 0; i < size; i++)
            // {
            //     VisualElement v = new VisualElement()
            //     {
            //         style =
            //         {
            //             borderTopWidth = 1,
            //             borderBottomWidth = 1,
            //             borderLeftWidth = 1,
            //             borderRightWidth = 1,
            //             borderTopColor = new Color(1f, 1f, 1f),
            //             borderBottomColor = new Color(1f, 1f, 1f),
            //             borderLeftColor = new Color(1f, 1f, 1f),
            //             borderRightColor = new Color(1f, 1f, 1f),
            //             backgroundColor = new Color(100f / 255f, 100f / 255f, 100f / 255f),
            //             paddingRight = 2 //Tip：因为ObjectField会设置marginRight为-2导致超出右边界（我真没想通为何它要这样设置。）
            //         }
            //     };
            //     // Debug.Log("绘制元素");
            //     var motion = motions.GetArrayElementAtIndex(i);
            //     v.Add(new PropertyField(motion.FindPropertyRelative("m_Clip")));
            //     v.Add(new PropertyField(motion.FindPropertyRelative("m_Threshold")));
            //     foldout.Add(v);
            // }

            // return root;
        }
    }
//     [CustomPropertyDrawer(typeof(MixerAnimation), true)]
//     public class MixerAnimationDrawer : PropertyDrawer
//     {
//         private const string PROP_MOTIONS = "m_Motions";
//         private const string PROP_CLIP = "m_Clip";
//         private const string PROP_THRESHOLD = "m_Threshold";

//         public override VisualElement CreatePropertyGUI(SerializedProperty property)
//         {
//             // root container
//             var root = new VisualElement();
//             root.style.paddingLeft = 2;
//             root.style.paddingRight = 2;

//             // Foldout header for the whole MixerAnimation
//             var foldout = new Foldout { text = property.displayName, value = true };
//             root.Add(foldout);

//             // expose fadeDuration as a simple PropertyField
//             var fadeProp = property.FindPropertyRelative("m_FadeDuration");
//             var fadeField = new PropertyField(fadeProp);
//             fadeField.style.marginBottom = 4;
//             foldout.Add(fadeField);

//             // motions array container
//             var motionsProp = property.FindPropertyRelative(PROP_MOTIONS);
//             var listContainer = new VisualElement();
//             listContainer.style.flexDirection = FlexDirection.Column;
//             listContainer.style.flexGrow = 1;
//             foldout.Add(listContainer);

//             // toolbar: add / remove buttons and size display
//             var toolbar = new VisualElement();
//             toolbar.style.flexDirection = FlexDirection.Row;
//             toolbar.style.justifyContent = Justify.SpaceBetween;
//             toolbar.style.marginBottom = 4;
//             listContainer.Add(toolbar);

//             var leftGroup = new VisualElement();
//             leftGroup.style.flexDirection = FlexDirection.Row;
//             toolbar.Add(leftGroup);

//             var sizeLabel = new Label($"Count: {motionsProp.arraySize}");
//             sizeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
//             sizeLabel.style.marginRight = 6;
//             leftGroup.Add(sizeLabel);

//             var addButton = new Button(() =>
//             {
//                 motionsProp.arraySize++;
//                 property.serializedObject.ApplyModifiedProperties();
//                 RefreshListUI(property, listContainer, motionsProp, sizeLabel);
//             })
//             { text = "Add Motion" };
//             leftGroup.Add(addButton);

//             var removeButton = new Button(() =>
//             {
//                 if (motionsProp.arraySize > 0)
//                 {
//                     motionsProp.DeleteArrayElementAtIndex(motionsProp.arraySize - 1);
//                     property.serializedObject.ApplyModifiedProperties();
//                     RefreshListUI(property, listContainer, motionsProp, sizeLabel);
//                 }
//             })
//             { text = "Remove Last" };
//             leftGroup.Add(removeButton);

//             // initial build
//             RefreshListUI(property, listContainer, motionsProp, sizeLabel);

//             // callback: when inspector changes externally (other scripts, undo/redo), refresh UI
//             property.serializedObject.Update();
//             var updater = new IMGUIContainer(() => { });
//             root.schedule.Execute(() =>
//             {
//                 // periodically ensure label count is in sync (cheap)
//                 if (sizeLabel.text != $"Count: {motionsProp.arraySize}")
//                 {
//                     RefreshListUI(property, listContainer, motionsProp, sizeLabel);
//                 }
//             }).Every(200);

//             return root;
//         }

//         private void RefreshListUI(SerializedProperty rootProperty, VisualElement listContainer, SerializedProperty motionsProp, Label sizeLabel)
//         {
//             listContainer.Clear();

//             // re-add toolbar (we'll keep a small top header to show count and buttons)
//             var toolbar = new VisualElement();
//             toolbar.style.flexDirection = FlexDirection.Row;
//             toolbar.style.justifyContent = Justify.SpaceBetween;
//             toolbar.style.marginBottom = 4;

//             var leftGroup = new VisualElement();
//             leftGroup.style.flexDirection = FlexDirection.Row;
//             toolbar.Add(leftGroup);

//             sizeLabel.text = $"Count: {motionsProp.arraySize}";
//             sizeLabel.style.marginRight = 6;
//             leftGroup.Add(sizeLabel);

//             var addButton = new Button(() =>
//             {
//                 motionsProp.arraySize++;
//                 rootProperty.serializedObject.ApplyModifiedProperties();
//                 RefreshListUI(rootProperty, listContainer, motionsProp, sizeLabel);
//             })
//             { text = "Add Motion" };
//             leftGroup.Add(addButton);

//             var removeButton = new Button(() =>
//             {
//                 if (motionsProp.arraySize > 0)
//                 {
//                     motionsProp.DeleteArrayElementAtIndex(motionsProp.arraySize - 1);
//                     rootProperty.serializedObject.ApplyModifiedProperties();
//                     RefreshListUI(rootProperty, listContainer, motionsProp, sizeLabel);
//                 }
//             })
//             { text = "Remove Last" };
//             leftGroup.Add(removeButton);

//             listContainer.Add(toolbar);

//             // read thresholds to detect duplicates and for sorting logic
//             int count = motionsProp.arraySize;
//             var thresholds = new List<float>(count);
//             for (int i = 0; i < count; i++)
//             {
//                 var el = motionsProp.GetArrayElementAtIndex(i);
//                 thresholds.Add(el.FindPropertyRelative(PROP_THRESHOLD).floatValue);
//             }

//             // detect duplicates: map threshold -> list of indices
//             var dupMap = new Dictionary<float, List<int>>();
//             for (int i = 0; i < thresholds.Count; i++)
//             {
//                 float t = thresholds[i];
//                 if (!dupMap.ContainsKey(t)) dupMap[t] = new List<int>();
//                 dupMap[t].Add(i);
//             }
//             var duplicateIndices = new HashSet<int>();
//             foreach (var kv in dupMap)
//             {
//                 if (kv.Value.Count > 1)
//                 {
//                     foreach (var idx in kv.Value) duplicateIndices.Add(idx);
//                 }
//             }

//             // Build UI entries
//             for (int i = 0; i < count; i++)
//             {
//                 var el = motionsProp.GetArrayElementAtIndex(i);
//                 var entry = CreateMotionEntry(rootProperty, motionsProp, i, el, duplicateIndices.Contains(i));
//                 listContainer.Add(entry);
//             }

//             // After building, ensure sorting is correct. If not sorted, sort now.
//             if (!IsSortedAscending(thresholds))
//             {
//                 SortMotionsByThreshold(rootProperty, motionsProp);
//                 rootProperty.serializedObject.ApplyModifiedProperties();
//                 // Rebuild UI to reflect new order
//                 RefreshListUI(rootProperty, listContainer, motionsProp, sizeLabel);
//                 return;
//             }
//         }

//         private VisualElement CreateMotionEntry(SerializedProperty rootProperty, SerializedProperty motionsProp, int index, SerializedProperty elementProp, bool isDuplicate)
//         {
//             var container = new VisualElement();
//             container.style.flexDirection = FlexDirection.Column;
//             container.style.marginBottom = 4;
//             container.style.paddingLeft = 4;
//             container.style.paddingRight = 4;
//             container.style.paddingTop = 4;
//             container.style.paddingBottom = 4;
//             container.style.borderTopWidth = 1;
//             container.style.borderBottomWidth = 1;
//             container.style.borderLeftWidth = 1;
//             container.style.borderRightWidth = 1;
//             container.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
//             container.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);
//             container.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
//             container.style.borderRightColor = new Color(0.22f, 0.22f, 0.22f);
//             if (isDuplicate)
//             {
//                 container.style.backgroundColor = new StyleColor(new Color(1f, 0.7f, 0.7f)); // light red
//             }

//             var header = new Label($"Motion {index}");
//             header.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
//             header.style.marginBottom = 4;
//             container.Add(header);

//             // clip field
//             var clipProp = elementProp.FindPropertyRelative(PROP_CLIP);
//             var clipField = new PropertyField(clipProp);
//             clipField.Bind(new SerializedObject(rootProperty.serializedObject.targetObject));
//             clipField.SetValueWithoutNotify(null); // no-op ensure it's created
//             container.Add(clipField);

//             // threshold field (float)
//             var thresholdProp = elementProp.FindPropertyRelative(PROP_THRESHOLD);
//             var thresholdField = new FloatField("Threshold");
//             thresholdField.value = thresholdProp.floatValue;
//             thresholdField.RegisterValueChangedCallback(evt =>
//             {
//                 // write value to serialized prop
//                 thresholdProp.floatValue = evt.newValue;
//                 rootProperty.serializedObject.ApplyModifiedProperties();

//                 // after change, sort list and refresh UI
//                 SortMotionsByThreshold(rootProperty, motionsProp);
//                 rootProperty.serializedObject.ApplyModifiedProperties();
//                 // find the root's parent container to refresh UI - use schedule to avoid modifying during callback
//                 var parent = container.parent;
//                 parent?.schedule(() => RefreshListUI(rootProperty, parent, motionsProp, parent.Q<Label>()), 1);
//             });
//             container.Add(thresholdField);

//             // show warnings if missing clip or threshold uninitialized (optional)
//             if (clipProp.objectReferenceValue == null)
//             {
//                 clipField.style.borderLeftWidth = 2;
//                 clipField.style.borderLeftColor = new Color(1f, 0.6f, 0f); // orange hint
//             }

//             return container;
//         }

//         private static bool IsSortedAscending(List<float> list)
//         {
//             for (int i = 1; i < list.Count; i++)
//             {
//                 if (list[i - 1] > list[i]) return false;
//             }
//             return true;
//         }

//         private void SortMotionsByThreshold(SerializedProperty rootProperty, SerializedProperty motionsProp)
//         {
//             int count = motionsProp.arraySize;
//             if (count <= 1) return;

//             // extract data
//             var items = new List<MotionData>(count);
//             for (int i = 0; i < count; i++)
//             {
//                 var el = motionsProp.GetArrayElementAtIndex(i);
//                 var clip = el.FindPropertyRelative(PROP_CLIP).objectReferenceValue as AnimationClip;
//                 var thresh = el.FindPropertyRelative(PROP_THRESHOLD).floatValue;
//                 items.Add(new MotionData { clip = clip, threshold = thresh });
//             }

//             // sort by threshold ascending; stable sort using index as tiebreaker
//             items.Sort((a, b) =>
//             {
//                 int cmp = a.threshold.CompareTo(b.threshold);
//                 return cmp != 0 ? cmp : 0;
//             });

//             // write back sorted values
//             for (int i = 0; i < count; i++)
//             {
//                 var el = motionsProp.GetArrayElementAtIndex(i);
//                 el.FindPropertyRelative(PROP_CLIP).objectReferenceValue = items[i].clip;
//                 el.FindPropertyRelative(PROP_THRESHOLD).floatValue = items[i].threshold;
//             }

//             rootProperty.serializedObject.ApplyModifiedProperties();
//         }

//         private struct MotionData
//         {
//             public AnimationClip clip;
//             public float threshold;
//         }
//     }

}