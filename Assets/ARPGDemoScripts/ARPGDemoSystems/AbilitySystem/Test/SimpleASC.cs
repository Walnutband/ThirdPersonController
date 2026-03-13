
using ARPGDemo.ControlSystem.InputActionBindings;
using UnityEngine;

namespace ARPGDemo.AbilitySystem.Test
{
    public class SimpleASC : MonoBehaviour
    {
        public AbilityTaskExecutor_New executor;
        public AbilityTask_Test task;
        public AbilityTask_Test load;
        public AbilityTask_Test hold;
        public AbilityTask_Test release;


        public InputActionBinder<InvokeSpecifiedCallback> binder;
        public InputActionBinder<InvokeSpecifiedCallback> binderLoad;
        // public InputActionBinder<InvokeSpecifiedCallback> binderHold;
        public InputActionBinder<InvokeSpecifiedCallback> binderRelease;

        public AbilityBase m_ChargedThrowAbility;

        public void OnEnable()
        {
            binder.Enable();
            binder.bindedEvent.AddCallback(Execute);
            Debug.Log("绑定输入");
            binderLoad.Enable();
            binderLoad.bindedEvent.AddCallback(Load);
            binderRelease.Enable();
            binderRelease.bindedEvent.AddCallback(Release);
        }

        private void Update()
        {
            executor.OnTick(Time.deltaTime);
        }

        public void Execute()
        {
            executor.Execute(task);
            Debug.Log("执行任务");
        }

        private void Load()
        {
            executor.Execute(load, Hold);
        }

        private void Hold()
        {
            executor.Execute(hold, Release);
        }

        private void Release()
        {
            executor.Stop();
            // executor.Execute(release, () => executor.AMC.moveState.Play(0));
            executor.Execute(release, () => executor.animState.Stop(release.stopDuration));
        }
    }
}