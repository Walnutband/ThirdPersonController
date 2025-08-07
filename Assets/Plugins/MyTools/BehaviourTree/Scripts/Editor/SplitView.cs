using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MyTools.BehaviourTreeTool
{
    public class SplitView : TwoPaneSplitView
    { //只是为了将TwoPaneSplitView暴露给UI Builder
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
    }
}