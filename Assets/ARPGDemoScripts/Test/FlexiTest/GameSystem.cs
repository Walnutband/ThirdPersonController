using Physalia.Flexi;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ARPGDemo.Test
{
    // public class ActorAttribute
    public class GameplayAttribute //不只是角色的属性，还可能是装备、道具这些对象的属性。
    {
        public float baseValue;
        public float currentValue;
    }

    public class Data
    {
        public int value;
    }

    public class GameSystem : MonoBehaviour
    {
        private readonly Data _data = new();

        [SerializeField]
        private AbilityAsset _asset;

        private FlexiCore _core;
        private DefaultAbilityContainer _container;

        private void Awake()
        {
            var builder = new FlexiCoreBuilder();
            _core = builder.Build();

            // Pre-load abilities
            //_core.LoadAbilityAll(_asset.Data);

            //创建能力容器，传入AbilityData以及groupIndex
            _container = new DefaultAbilityContainer(this, _asset.Data, 0);
        }

        private void Update()
        {
            // if (Input.GetKeyUp(KeyCode.Space))
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _ = _core.TryEnqueueAbility(_container, EmptyEventContext.Instance);
                _core.Run();
            }
        }

        public void AddDataValue(int value)
        {
            _data.value += value;
            Debug.Log($"Data value: {_data.value}");  // Print log so we can trace.
        }
    }
}
