namespace Ilumisoft.VisualStateMachine
{
    [System.Serializable]
    public class AnyState : Node { } //该状态不能被转入，只能转出，也就是用于在任何状态下都能转入指定的某个状态，可以无视所给定转换的起源状态
}