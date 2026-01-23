using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace ARPGDemo.Test
{
    public class UxmlTest : MonoBehaviour 
    {
        public PlayableTest test;

        private UIDocument ui;

        private void Awake()
        {
            ui = GetComponent<UIDocument>();
            
        }

        private void Start()
        {
            // ui.rootVisualElement.Add(new Button(test.CreateBehaviour));
            // ui.rootVisualElement.Q<Button>().clicked += test.CreateBehaviour;
            ui.rootVisualElement.Q<Button>("CreateGraph").RegisterCallback<ClickEvent>(test.CreateGraph);
            ui.rootVisualElement.Q<Button>("PlayGraph").RegisterCallback<ClickEvent>(test.PlayGraph);
            ui.rootVisualElement.Q<Button>("StopGraph").RegisterCallback<ClickEvent>(test.StopGraph);
            ui.rootVisualElement.Q<Button>("DebugInfo").RegisterCallback<ClickEvent>(DebugGraphInfo);
            ui.rootVisualElement.Q<Button>("PlayClip").RegisterCallback<ClickEvent>(test.PlayClip);
            ui.rootVisualElement.Q<Button>("PauseClip").RegisterCallback<ClickEvent>(test.PauseClip);
        }

        private void DebugGraphInfo(ClickEvent _evt)
        {
            if (test.playableGraph.IsValid())
            {
                Debug.Log($"test.playableGraph.IsValid() == {test.playableGraph.IsValid()}");
                Debug.Log($"test.playableGraph.IsDone() == {test.playableGraph.IsDone()}");
                Debug.Log($"test.playableGraph.IsPlaying() == {test.playableGraph.IsPlaying()}");
                Debug.Log($"test.playableGraph.GetRootPlayableCount() == {test.playableGraph.GetRootPlayableCount()}");
                Debug.Log($"test.playableGraph.GetPlayableCount() == {test.playableGraph.GetPlayableCount()}");
                Debug.Log($"test.playableGraph.GetTimeUpdateMode() == {test.playableGraph.GetTimeUpdateMode()}");
            }
        }
    }
    
}