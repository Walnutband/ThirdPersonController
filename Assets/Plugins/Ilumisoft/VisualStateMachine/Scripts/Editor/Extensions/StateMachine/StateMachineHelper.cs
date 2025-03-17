namespace Ilumisoft.VisualStateMachine.Editor.Extensions
{
    public static class StateMachineHelper
    {
        /// <summary>
        /// Gets the Graph behaviour from the state machines children and returns it
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public static Graph GetStateMachineGraph(this StateMachine stateMachine) => stateMachine.Graph;
        //{
        //    return stateMachine.Graph;
        //}

        /// <summary>
        /// Gets the Preferences behaviour from the state machines children and returns it
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public static Preferences GetPreferences(this StateMachine stateMachine)
        {
            return stateMachine.Graph.Preferences; //状态机本身的Graph以及Graph的首选项
        }
    }
}