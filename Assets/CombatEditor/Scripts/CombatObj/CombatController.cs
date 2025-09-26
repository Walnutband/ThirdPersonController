using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace CombatEditor
{

    public class RecordedClip2Time
    {
        public AnimationClip _clip;
        public double time;
        public RecordedClip2Time(AnimationClip clip,double time)
        {
            _clip = clip;
            this.time = time;
        }
    }


    [System.Serializable]
	public class CombatGroup
	{
	    public bool IsFolded;
	    public string Label;
	    public List<AbilityScriptableObject> CombatObjs;
	    public List<AbilityObjWithEffect> eves = new List<AbilityObjWithEffect>();
	}
    
    //这是对于每一个Action的封装
	public class AbilityObjWithEffect
    {
        public AbilityScriptableObject Obj;
        public int Index;
        public List<AbilityEventEffect> EventEffects = new List<AbilityEventEffect>();
    }
	
	[System.Serializable]
	public class CharacterNode
	{
	    public enum NodeType { Animator, BottomCenter, BodyCenter, Head, Spine, Hand, RHand, LHand, Foot ,LFoot, RFoot, Weapon , WeaponBase, WeaponTip}
	    public NodeType type;
	    public Transform NodeTrans;
	}
	
	
	public class AbilityEventWithEffects
	{
	    public AbilityEvent eve;
	    public AbilityEventEffect effect;
	}



    public class CombatController : MonoBehaviour
    {
        public Animator _animator;
        public AbilityScriptableObject SelectedAbility;
        //Tip：
        public List<CombatGroup> CombatDatas = new List<CombatGroup>();
        public AnimationClip clip;

        CombatEventReceiver receiver;


        public AnimSpeedExecutor _animSpeedExecutor;
        public MoveExecutor _moveExecutor;


        public List<CharacterNode> Nodes = new List<CharacterNode>();

        public List<RecordedClip2Time> _recordedSelfTransClips = new List<RecordedClip2Time>();

        //一个Clip就代表一个Action，每个轨道就是一个Ability，这个字典就是每个CombatController组件自己的要附加到对应片段上的内容——动画片段和附加内容的映射关系
        public Dictionary<int, List<AbilityEventWithEffects>> ClipID_To_EventEffects;

        public List<AbilityEventEffect_States> RunningStates = new List<AbilityEventEffect_States>();

        private void Start()
        {

            ClipID_To_EventEffects = new Dictionary<int, List<AbilityEventWithEffects>>(); 
            ClearNullReference();
            InitClipsOnRunningLayers();
            InitAnimEffects();
            _animSpeedExecutor = new AnimSpeedExecutor(this);
            _moveExecutor = new MoveExecutor(this);
        }

        /*TODO：这里访问的是AnimatorController中的Layer层级，如果用另外的动画系统的话肯定就要在此修改了*/
        public void InitClipsOnRunningLayers()
        {
            LayerActiveClipIDs = new List<int[]>();
            for (int i = 0; i < _animator.layerCount; i++)
            {
                //每一层使用一个int数组来记录ClipID，这里主要是初始化分配内存，还没有实际赋值。
                LayerActiveClipIDs.Add(null); 
            }
        }
        public void ClearNullReference()
        {
            //遍历组Group
            for (int i = 0; i < CombatDatas.Count; i++)
            {
                //遍历动作Action
                for (int j = 0; j < CombatDatas[i].CombatObjs.Count; j++)
                {
                    if (CombatDatas[i].CombatObjs[j] == null)
                    {
                        CombatDatas[i].CombatObjs.RemoveAt(j);
                        //因为RemoveAt是直接将指定位置的元素从内存中删除了，同时将后面的元素向前移动，而在每次循环后会j++，所以在此j--以抵消
                        j--; 
                        continue;
                    }
                    else
                    {
                        //遍历轨道Track
                        for (int k = 0; k < CombatDatas[i].CombatObjs[j].events.Count; k++)
                        {
                            //这里的events[k]就代表的轨道，而该编辑器中一个轨道只能放一个Clip，也就相当于这里访问的Obj。
                            if (CombatDatas[i].CombatObjs[j].events[k].Obj == null)
                            {
                                CombatDatas[i].CombatObjs[j].events.RemoveAt(k);
                                k--;
                                continue;
                            }
                        }

                    }
                }
            }
        }


        private void Update()
        {
         
            RunEffects(0);
            _animSpeedExecutor.Execute(); //动画是典型的帧率越高越流畅的在渲染帧处理的对象。
        }
        private void FixedUpdate()
        {
            RunEffects(1);
        }

        public Vector3 GetCurrentRootMotion()
        {
            return _moveExecutor.GetCurrentRootMotion();
        }
        
        public List<int[]> LayerActiveClipIDs = new List<int[]>();
        
        //控制器的主要运行逻辑
        /// <summary>
        /// Fetch states and clips in animator.
        /// UpdateMode : 0:Update 1:FixedUpdate.
        /// </summary>
        public void RunEffects(int UpdateMode = 0)
        {
            for (int i = 0; i < _animator.layerCount; i++)
            {
                var LayerIndex = i;
                //如果没有在过渡的话
                if (!_animator.IsInTransition(LayerIndex))
                {/*TODO：Animator的API比起Animancer就差太多啦。。。*/
                    var CurrentAnimState = _animator.GetCurrentAnimatorStateInfo(LayerIndex);
                    var RunningClips = _animator.GetCurrentAnimatorClipInfo(LayerIndex);

                    int[] runningclipsID = new int[RunningClips.Length];
                    for (int j = 0; j < RunningClips.Length; j++)
                    {//获取当前正在运行的所有动画片段的InstanceID
                        runningclipsID[j] = RunningClips[j].clip.GetInstanceID();
                    }
                    UpdateLayerActiveClips(LayerIndex, runningclipsID);

                    for (int j = 0; j < RunningClips.Length; j++)
                    {
                        var CurrentClipID = RunningClips[j].clip.GetInstanceID();

                        if (!ClipID_To_EventEffects.ContainsKey(CurrentClipID)) { continue; }


                        RunningEventsOnClip(CurrentClipID, CurrentAnimState.normalizedTime, LayerIndex, UpdateMode);
                    }
                }
                //正在过渡的话，就是获取正在转入的目标状态的信息。（与上面就是if-else的互斥关系，而且逻辑完全一样，只是前者获取当前状态的信息，此处获取转入状态的信息）
                if (_animator.IsInTransition(LayerIndex))
                {
                    var NextAnimState = _animator.GetNextAnimatorStateInfo(LayerIndex);
                    var NextRunningClips = _animator.GetNextAnimatorClipInfo(LayerIndex);

                    int[] runningClipsID = new int[NextRunningClips.Length];
                    for (int j = 0; j < NextRunningClips.Length; j++)
                    {
                        runningClipsID[j] = NextRunningClips[j].clip.GetInstanceID();
                    }
                    UpdateLayerActiveClips(LayerIndex, runningClipsID);

                    for (int j = 0; j < NextRunningClips.Length; j++)
                    {
                        var CurrentClip = NextRunningClips[j].clip.GetInstanceID();
                        if (!ClipID_To_EventEffects.ContainsKey(CurrentClip)) { continue; }
                        RunningEventsOnClip(CurrentClip, NextAnimState.normalizedTime, LayerIndex, UpdateMode);
                    }
                }

            }
        }

        //运行设置在AnimationClip上的各个事件（就是各个轨道上的内容），就是将提前设定好的内容附加到对应片段的播放过程中。
        /*Tip：这里就是时间轴的基本逻辑，其实非常直观，就是一个框架而已，终究的难点还是在于各个轨道、各个轨道上的Clip的具体逻辑。*/
        /// <summary>
        //  Running the target effects on animation clip.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="NormalizedTime"></param>
        /// <param name="LayerIndex"></param>
        /// <param name="UpdateMode"> 0 : Update 1:FixedUpdate </param>
        public void RunningEventsOnClip(int clipID, float NormalizedTime, int LayerIndex, int UpdateMode = 0)
        {
            //一个AnimationClip就对应一个Action，这里就取出该Action的各个轨道的相关内容。
            List<AbilityEventWithEffects> abilityEventWithEffects = ClipID_To_EventEffects[clipID];

            for (int j = 0; j < abilityEventWithEffects.Count; j++)
            {
                var eve = abilityEventWithEffects[j];
                var StartTime = eve.eve.GetEventStartTime();
                var EndTime = eve.eve.GetEventEndTime();
                var EveTimeType = eve.eve.GetEventTimeType();
                //应用即时效果
                if (EveTimeType == AbilityEventObj.EventTimeType.EventTime && UpdateMode == 0)
                {
                    if (NormalizedTime >= StartTime)
                    {
                        //这样意思是可以跳转？？在跳转的那一帧会调用StartEffect，
                        if (eve.effect._EventObj.IsActive && !eve.effect.IsRunning)
                        {
                            eve.effect.StartEffect();
                        }
                        if (eve.effect._EventObj.IsActive && eve.effect.IsRunning)
                        {
                            eve.effect.EffectRunning();
                        }
                    }
                    //If self to self translation, events can't close itself cause clip is not change. So it need to close itself by percentage.
                    else
                    {
                        eve.effect.EndEffect();
                    }
                }
                //应用持续效果
                if (EveTimeType == AbilityEventObj.EventTimeType.EventRange || EveTimeType == AbilityEventObj.EventTimeType.EventMultiRange)
                {
                    //Start Even if the start frame is jumpped 
                    //  StartTime < CurrentTime < EndTime
                    if (NormalizedTime < EndTime && NormalizedTime >= StartTime)
                    {
                        if (!eve.effect.IsRunning && eve.effect._EventObj.IsActive && UpdateMode == 0)
                        {
                            eve.effect.StartEffect();
                        }

                        if (eve.effect.IsRunning)
                        {
                            if (UpdateMode == 0)
                            {
                                eve.effect.EffectRunning();
                                eve.effect.EffectRunning(NormalizedTime);
                            }
                            if (UpdateMode == 1)
                            {
                                eve.effect.EffectRunningFixedUpdate(NormalizedTime);
                            }
                        }

                    }
                    //If self to self translation, events can't close itself cause clip is not change. So it need to close itself by percentage.

                    if (NormalizedTime >= EndTime || NormalizedTime < StartTime && UpdateMode == 0)
                    {
                        if (eve.effect._EventObj.IsActive)
                        {
                            eve.effect.EndEffect();
                        }
                    }
                }
            }
        }

        //更新对应层级的活跃Clip
        /// <summary>
        /// Update Active Clips on target Layer, End last frame events if needed.
        /// </summary>
        /// <param name="LayerIndex"></param>
        /// <param name="clips"></param>
        public void UpdateLayerActiveClips(int LayerIndex, int[] clipsID)
        {
            bool RunningClipsChangedInLayer = false;
            if (LayerActiveClipIDs[LayerIndex] != null)
            {
                //充分不必要条件，从最容易判断的充分条件入手，在特殊情况下就可以减少计算量。
                if (LayerActiveClipIDs[LayerIndex].Length != clipsID.Length)
                {
                    RunningClipsChangedInLayer = true;
                }
                else
                {
                    for (int i = 0; i < LayerActiveClipIDs[LayerIndex].Length; i++)
                    {/*Tip：在AnimatorController中，如果没有正在过渡的话，应该只可能在混合树中才会存在同时有多个活跃的Clip的情况。*/
                        if (LayerActiveClipIDs[LayerIndex][i] != clipsID[i])
                        {
                            RunningClipsChangedInLayer = true;
                        }
                    }

                }
            }
            else RunningClipsChangedInLayer = true; //就是之前都没有记录过这个层级的活跃Clip，那么就自动视为有变化。

            if (RunningClipsChangedInLayer)
            {
                if (LayerActiveClipIDs[LayerIndex] != null)
                    for (int i = 0; i < LayerActiveClipIDs[LayerIndex].Length; i++)
                    {
                        var clip = LayerActiveClipIDs[LayerIndex][i];
                        if (ClipID_To_EventEffects.ContainsKey(clip))
                        {
                            for (int j = 0; j < ClipID_To_EventEffects[clip].Count; j++)
                            {
                                /*Ques：为什么会调用EndEffect结束？看上面的条件，意思是只要有变化就会调用EndEffect，有点不合理？*/
                                ClipID_To_EventEffects[clip][j].effect.EndEffect();
                            }
                        }
                    }
                LayerActiveClipIDs[LayerIndex] = clipsID; //直接更新整体。
            }
        }
        
	    public Transform GetNodeTranform(CharacterNode.NodeType type)
	    {
	        if(type == CharacterNode.NodeType.Animator)
	        {
	            if (_animator != null)
	            {
	                return _animator.transform;
	            }
	            return transform;
	        }
	
	        for(int i  =0;i<Nodes.Count;i++)
	        {
	            if(Nodes[i].type == type)
	            {
	                if(Nodes[i].NodeTrans == null)
	                {
	                    return _animator.transform;
	                }
	                return Nodes[i].NodeTrans;
	            }
	        }
	        return _animator.transform;
	    }
	
	    public void SimpleMoveRG(Vector3 deltaMove)
	    {
            _moveExecutor.Move(deltaMove);
        }
	
	    public void InitAnimReceiver()
	    {
	        receiver = _animator.gameObject.AddComponent<CombatEventReceiver>();
	        receiver.controller = this;
	        receiver.CombatDatasID = new List<string>();
	        for (int i =0;i<CombatDatas.Count;i++)
	        {
	            var Group = CombatDatas[i];
	            for (int j = 0; j < Group.CombatObjs.Count; j++)
	            {
	                receiver.CombatDatasID.Add(Group.CombatObjs[j].GetInstanceID().ToString());
	            }
	        }
	    }
	    public void StartEvent(int GroupIndex, int ObjIndex, int EventIndex)
	    {
	        if (CombatDatas[GroupIndex].eves[ObjIndex].Obj.events[EventIndex].Obj.IsActive)
	        {
	            CombatDatas[GroupIndex].eves[ObjIndex].EventEffects[EventIndex].eve = CombatDatas[GroupIndex].CombatObjs[ObjIndex].events[EventIndex];
	            CombatDatas[GroupIndex].eves[ObjIndex].EventEffects[EventIndex].StartEffect();
	        }
	    }
	    public void EndEvent(int GroupIndex, int ObjIndex, int EventIndex)
	    {
	        if (CombatDatas[GroupIndex].eves[ObjIndex].Obj.events[EventIndex].Obj.IsActive)
	        {
	            CombatDatas[GroupIndex].eves[ObjIndex].EventEffects[EventIndex].EndEffect();
	        }
	    }


        /// <summary>
        /// 初始化各个Action及其各个轨道（填充字典ClipID_To_EventEffects和）
        /// <para>就是设置变量引用</para>
        /// </summary>
        public void InitAnimEffects()
	    {
            //各个Group
	        for (int i = 0; i < CombatDatas.Count; i++)
            {
                var Group = CombatDatas[i];
                //各个Action
                for (int j = 0; j < Group.CombatObjs.Count; j++)
                {
                    //Caution: The Number of CombatObj and EventEffects must sync
                    var CombatObj = Group.CombatObjs[j];
                    AbilityObjWithEffect ae = new AbilityObjWithEffect(); //这是对于Action资产（AbilityScriptableObject）的封装，或者说运行时代表。
                    ae.Obj = CombatObj;
                    for (int k = 0; k < CombatObj.events.Count; k++)
                    {
                        /*Tip：注意理解InstanceID，这是对于运行时实例（UnityEngine.Object类型）的唯一标识，由引擎底层管理，在编辑器中可能会与其他ID搞混，
                        但要知道在代码中访问的内容必然是已经加载到内存中的内容，要么是运行时创建、要么是通过持久化资产反序列化得到。
                        当然这里能够通过GetInstanceID获取到AnimationClip的实例说明它已经加载到了内存中，这是因为引擎机制就是，加载场景时就会将其中引用的所有资产
                        同时加载到内存中，这并不需要专门的资产管理系统，因为资产管理就是因为内存有限而不得不在需要用到某些资源的时候才加载，不需要的时候就将其从内存中卸载，
                        而在其他时候就只是存储在硬盘中*/
                        var EventEffect = AddEventEffects(CombatObj.Clip.GetInstanceID(), CombatObj.events[k]);
                        EventEffect.AnimObj = CombatObj; //Ques：是否有必要保存对于所在Action的引用呢？
                        ae.EventEffects.Add(EventEffect);
                    }
                    Group.eves.Add(ae);
                }
            }
	    }
	
        /*Tip：将字段定义在使用它的地方确实更加直观，不过就不方便一眼看出来该类拥有哪些成员。*/
	    List<AbilityEventEffect> _abilityEventEffects = new List<AbilityEventEffect>();
	    public AbilityEventEffect AddEventEffects( int clipID, AbilityEvent eve)
	    {
	        AbilityEventObj EffectObj = eve.Obj;
	        AbilityEventEffect _abilityEventEffect = EffectObj.Initialize(); //这算是从资产转换到运行时对象。
	        _abilityEventEffect.eve = eve;
	        _abilityEventEffect._combatController = this;
	        _abilityEventEffects.Add(_abilityEventEffect); //落实到AbilityEventEffect

            //这个类应该只是单纯地将AbilityEvent与AbilityEventEffect关联起来（映射关系，不过并非字典那样的明确映射）
            AbilityEventWithEffects eveWithEffects = new AbilityEventWithEffects();
	        eveWithEffects.eve = eve;
	        eveWithEffects.effect = _abilityEventEffect;
	
	        //Save all animationEvents to dictionary
	        if(ClipID_To_EventEffects.ContainsKey(clipID))
	        {
                //列表中每个元素存储的就是在编辑器中看到的对应的一条轨道上的内容（Effect）
                ClipID_To_EventEffects[clipID].Add(eveWithEffects);
	        }
	        else
	        {
	            List<AbilityEventWithEffects> list = new List<AbilityEventWithEffects>();
	            list.Add(eveWithEffects);
                ClipID_To_EventEffects.Add(clipID, list);
	        }
	
	
	        return _abilityEventEffect;
	    }
	
	
        public bool IsInState(string Name)
        {
            for(int i =0;i<RunningStates.Count;i++)
            {
                if (RunningStates[i].CurrentStateName == Name)
                {
                    return true;
                }
            }
            return false;
        }
	
	
	}
}
