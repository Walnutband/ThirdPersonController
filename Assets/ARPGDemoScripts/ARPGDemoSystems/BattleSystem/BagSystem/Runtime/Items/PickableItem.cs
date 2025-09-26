using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    //作为组件，具有独立性，单独挂载在一个游戏对象上，并且可以被GetComponent获取。
    [AddComponentMenu("ARPGDemo/BattleSystem/PickableItem")]
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class PickableItem : MonoBehaviour, IInteractable<Item>
    {
        public int priority => 10;

        public InteractionType type => InteractionType.PickUp; //由拾取行为来与该对象进行交互

        /*TODO：感觉用Bag背包来封装物品会更好，在完全兼容的情况下能够实现更高得多的扩展性。*/
        // private Item m_Item;
        [SerializeField] private BagData.ItemInfo m_ItemInfo;
        [SerializeField] private AudioClip m_DisappearSound;

        public Item InteractableTarget()
        {
            ItemData itemData = InventoryManager.Instance.GetItemData(m_ItemInfo.id);
            return new Item(itemData, m_ItemInfo.amount);
        }

        void IInteractable.InteractionEnd() //显式实现
        {
            Disappear();
        }

        public void Disappear()
        {
            AudioManager.Instance.PlaySound(m_DisappearSound, transform.position);
            gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}