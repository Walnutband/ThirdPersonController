using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ARPGDemo.BuffSystemSample
{
    ///<summary>
    ///角色的“状态”，用来管理当前应该怎么移动、应该怎么旋转、应该怎么播放动画的。
    ///是一个角色的总的“调控中心”。
    ///</summary>
    public class ChaState : MonoBehaviour
    {
        ///<summary>
        //角色最终的可操作性状态
        ///</summary>
        private ChaControlState _controlState = new ChaControlState(true, true, true);

        ///<summary>
        ///GameTimeline专享的ChaControlState
        ///</summary>
        public ChaControlState timelineControlState = new ChaControlState(true, true, true);

        public ChaControlState controlState
        {
            get
            {
                return this._controlState + this.timelineControlState;
            }
        }

        /*TODO：这里专门设置一个无敌状态变量感觉不太合适，因为这应该属于一个Buff，而不是角色本身的一个属性。*/
        ///<summary>
        ///角色的无敌状态持续时间，如果在无敌状态中，子弹不会碰撞，DamageInfo处理无效化
        ///单位：秒
        ///</summary>
        public float immuneTime
        {
            get
            {
                return _immuneTime;
            }
            set
            {
                _immuneTime = Mathf.Max(_immuneTime, value);
            }
        }
        private float _immuneTime = 0.00f;

        ///<summary>
        ///角色是否处于一种蓄力的状态
        ///</summary>
        public bool charging = false;

        ///<summary>
        ///角色主动期望的移动方向
        ///</summary>
        public float moveDegree
        {
            get
            {
                return _wishToMoveDegree;
            }
        }
        private float _wishToMoveDegree = 0.00f;

        ///<summary>
        ///角色主动期望的面向
        ///</summary>
        public float faceDegree
        {
            get
            {
                return _wishToFaceDegree;
            }
        }
        private float _wishToFaceDegree = 0.00f;

        ///<summary>
        ///角色是否已经死了，这不由我这个系统判断，其他系统应该告诉我
        ///</summary>
        public bool dead = false;

        //来自操作或者ai的移动请求信息
        private Vector3 moveOrder = new Vector3();

        //来自强制发生的位移信息，通常是技能效果等导致的，比如翻滚、被推开等
        private List<MovePreorder> forceMove = new List<MovePreorder>();

        //收到的来自各方的播放动画的请求
        private List<string> animOrder = new List<string>();

        //来自操作或者ai的旋转角度请求
        private float rotateToOrder;

        //来自强制执行的旋转角度
        private List<float> forceRotate = new List<float>();

        ///<summary>
        ///角色现有的资源，比如hp之类的
        ///</summary>
        public ChaResource resource = new ChaResource(1);

        [Tooltip("角色所处阵营，阵营不同就会对打")]
        ///<summary>
        ///角色所处阵营，阵营不同就会对打
        ///</summary>
        public int side = 0; //TODO：或许应该使用枚举类型定义（取名比如Team），当然一般来说最简单的就是友方、敌人、中立者。

        ///<summary>
        ///根据tags可以判断出这是什么样的人
        ///</summary>
        public string[] tags = new string[0];

        /*TODO：作为总属性的property由baseProp、buffProp和equipmentProp组成，从逻辑上来看，应该只有总属性property会对外公开，而另外三种属性都属于个体私有的，
        只是来源有所不同，baseProp来源于自身（可以是角色等级、角色能力值），buffProp来源于所拥有的Buff，equipmentProp来源于所穿戴的装备，根据自身所拥有的Buff和装备计算即可，
        然后参与到总属性property的计算中，然后在外部参与要用到角色属性的相关逻辑*/

        ///<summary>
        ///角色当前的属性(总属性)
        ///</summary>
        public ChaProperty property
        {
            get
            {
                return _prop;
            }
        }
        private ChaProperty _prop = ChaProperty.zero;

        ///<summary>
        ///角色移动力，单位：米/秒
        ///</summary>
        public float moveSpeed
        {
            get
            {
                //这个公式也可以通过给策划脚本接口获得，这里就写代码里了，不走策划脚本了
                //设定，值=0.2+5.6*x/(x+100)，初始速度是100，移动力3米/秒，最小值0.2米/秒。
                return this._prop.moveSpeed * 5.600f / (this._prop.moveSpeed + 100.000f) + 0.200f;
            }
        }

        ///<summary>
        ///角色行动速度，是一个timescale，最小0.1，初始行动速度值也是100。
        ///</summary>
        public float actionSpeed
        {
            get
            {
                return this._prop.actionSpeed * 4.90f / (_prop.actionSpeed + 390.00f) + 0.100f;
            }
        }

        ///<summary>
        ///角色的基础属性，也就是每个角色“裸体”且不带任何buff的“纯粹的属性”
        ///先写死，正式的应该读表
        ///</summary>
        public ChaProperty baseProp = new ChaProperty(
            100, 100, 0, 20, 100
        );

        ///<summary>
        ///角色来自buff的属性
        ///这个数组并不是说每个buff可以占用一条数据，而是分类总和
        ///在这个游戏里buff带来的属性总共有2类，plus和times，用策划设计的公式就是plus的属性加完之后乘以times的属性
        ///所以数组长度其实只有2：[0]buffPlus, [1]buffTimes
        ///</summary>
        public ChaProperty[] buffProp = new ChaProperty[2] { ChaProperty.zero, ChaProperty.zero };

        ///<summary>
        ///来自装备的属性
        ///</summary>
        public ChaProperty equipmentProp = ChaProperty.zero;

        ///<summary>
        ///角色的技能
        ///</summary>
        public List<SkillObj> skills = new List<SkillObj>();

        ///<summary>
        ///角色身上的buff
        ///</summary>
        public List<BuffObj> buffs = new List<BuffObj>();


        private UnitMove unitMove;
        private UnitAnim unitAnim;
        private UnitRotate unitRotate;
        private Animator animator;
        private UnitBindManager bindPoints;
        private GameObject viewContainer;

        void Start()
        {
            rotateToOrder = transform.rotation.eulerAngles.y;

            synchronizedUnits();

            AttrRecheck();
        }

        void FixedUpdate()
        {
            float timePassed = Time.fixedDeltaTime;
            if (dead == false)
            {
                //如果角色没死，做这些事情：

                //无敌时间减少
                if (_immuneTime > 0) _immuneTime -= timePassed;

                //技能冷却时间
                for (int i = 0; i < this.skills.Count; i++)
                {
                    if (this.skills[i].cooldown > 0)
                    {
                        this.skills[i].cooldown -= timePassed;
                    }
                }

                //对身上的buff进行管理
                List<BuffObj> toRemove = new List<BuffObj>();
                for (int i = 0; i < this.buffs.Count; i++)
                {
                    if (buffs[i].permanent == false) buffs[i].duration -= timePassed;
                    buffs[i].timeElapsed += timePassed;

                    if (buffs[i].model.tickTime > 0 && buffs[i].model.onTick != null)
                    {
                        //float取模不精准，所以用x1000后的整数来
                        if (Mathf.RoundToInt(buffs[i].timeElapsed * 1000) % Mathf.RoundToInt(buffs[i].model.tickTime * 1000) == 0)
                        {
                            buffs[i].model.onTick(buffs[i]); //Buff的监测性内容
                            buffs[i].ticked += 1;
                        }
                    }

                    //只要duration <= 0，不管是否是permanent都移除掉
                    if (buffs[i].duration <= 0 || buffs[i].stack <= 0)
                    {
                        if (buffs[i].model.onRemoved != null)
                        {
                            buffs[i].model.onRemoved(buffs[i]);
                        }
                        toRemove.Add(buffs[i]);
                    }
                }
                if (toRemove.Count > 0)
                {
                    for (int i = 0; i < toRemove.Count; i++)
                    {//Ques：这里使用Remove传入的是元素而非索引，按理来说是遍历，那么这里是否是一个O(n^2)的操作。
                        this.buffs.Remove(toRemove[i]);
                    }
                    AttrRecheck();
                }

                toRemove = null;

                //给各个系统发消息
                bool wishToMove = moveOrder != Vector3.zero;
                if (wishToMove == true)
                    _wishToMoveDegree = Mathf.Atan2(moveOrder.x, moveOrder.z) * 180 / Mathf.PI;

                ChaControlState curCS = this.controlState;// _controlState + timelineControlState;

                //首先是合并移动信息，发送给UnitMove
                bool tryRun = curCS.canMove == true && moveOrder != Vector3.zero;
                float tryMoveDegree = Mathf.Atan2(moveOrder.x, moveOrder.z) * 180 / Mathf.PI;
                if (tryMoveDegree > 180) tryMoveDegree -= 360;
                if (unitMove)
                {
                    if (curCS.canMove == false) moveOrder = Vector3.zero;
                    int fmIndex = 0;
                    while (fmIndex < forceMove.Count)
                    {
                        moveOrder += forceMove[fmIndex].VeloInTime(timePassed);
                        if (forceMove[fmIndex].duration <= 0)
                        {
                            forceMove.RemoveAt(fmIndex);
                        }
                        else
                        {
                            fmIndex++;
                        }
                    }
                    unitMove.MoveBy(moveOrder);
                    moveOrder = Vector3.zero;
                    //forceMove.Clear();
                }

                _wishToFaceDegree = rotateToOrder;
                if (wishToMove == false) _wishToMoveDegree = _wishToFaceDegree;
                //然后是旋转信息
                if (unitRotate)
                {
                    if (curCS.canRotate == false) rotateToOrder = transform.rotation.eulerAngles.y;
                    for (int i = 0; i < forceRotate.Count; i++)
                    {
                        //这里全是增量，而不是设定为，所以可以直接加
                        rotateToOrder += forceRotate[i];
                    }
                    unitRotate.RotateTo(rotateToOrder);
                    forceRotate.Clear();
                }
                //再是动画处理
                if (unitAnim)
                {
                    unitAnim.timeScale = this.actionSpeed;
                    //先计算默认（规则下）的动画，并且添加到动画组
                    if (tryRun == false)
                    {
                        animOrder.Add("Stand");    //如果没有要求移动，就用站立
                    }
                    else
                    {
                        string tt = Utils.GetTailStringByDegree(transform.rotation.eulerAngles.y, tryMoveDegree);
                        animOrder.Add("Move" + tt);
                    }
                    //送给动画系统处理
                    for (int i = 0; i < animOrder.Count; i++)
                    {
                        unitAnim.Play(animOrder[i]);
                    }
                    animOrder.Clear();
                }
                if (animator)
                {
                    animator.speed = this.actionSpeed;
                }
            }
            else
            {
                _wishToFaceDegree = transform.rotation.eulerAngles.y * 180.00f / Mathf.PI;
                _wishToMoveDegree = _wishToFaceDegree;
            }
        }

        private void synchronizedUnits()
        {
            if (!unitMove) unitMove = this.gameObject.GetComponent<UnitMove>();
            if (!unitAnim) unitAnim = this.gameObject.GetComponent<UnitAnim>();
            if (!unitRotate) unitRotate = this.gameObject.GetComponent<UnitRotate>();
            if (!animator) animator = this.gameObject.GetComponent<Animator>();
            if (!bindPoints) bindPoints = this.gameObject.GetComponent<UnitBindManager>();
            if (!viewContainer) viewContainer = this.gameObject.GetComponentInChildren<ViewContainer>().gameObject;
        }

        ///<summary>
        ///命令移动
        ///<param name="move">移动力</param>
        ///</summary>
        public void OrderMove(Vector3 move)
        {
            this.moveOrder.x = move.x;
            this.moveOrder.z = move.z;
        }

        ///<summary>
        ///强制移动
        ///<param name="moveInfo">移动信息</param>
        ///</summary>
        public void AddForceMove(MovePreorder move)
        {
            this.forceMove.Add(move);
        }

        ///<summary>
        ///命令旋转到多少度
        ///<param name="degree">旋转目标</param>
        ///</summary>
        public void OrderRotateTo(float degree)
        {
            this.rotateToOrder = degree;
        }

        ///<summary>
        ///强制旋转的力量
        ///<param name="degree">偏移角度</param>
        ///</summary>
        public void AddForceRotate(float degree)
        {
            this.forceRotate.Add(degree);
        }

        ///<summary>
        ///添加角色要做的动作请求
        ///<param name="animName">要做的动作</param>
        ///</summary>
        public void Play(string animName)
        {
            animOrder.Add(animName);
        }

        ///<summary>
        ///杀死这个角色
        ///</summary>
        public void Kill()
        {
            this.dead = true;
            if (unitAnim)
            {
                unitAnim.Play("Dead");
            }
            //如果不是主角，尸体就会消失
            if (this.gameObject != SceneVariants.MainActor())
                this.gameObject.AddComponent<UnitRemover>().duration = 5.0f;
        }

        /*Tip：在处理Buff时是按照BuffObj和BuffModel整体来处理的，但是实际运算时是按照Buff所应用到的那些基础属性来计算的，所以在此要根据当前记录的Buff更新当前的基础属性值，
        须注意，基础属性指的是属性的基础性，而当前的基础属性值如下所示，其实是基础值 + 装备值 + Buff值。*/
        ///<summary>
        ///重新计算所有属性，并且获得一个最终属性
        ///其实这个应该走脚本函数返回，抛给脚本函数多个ChaProperty，由脚本函数运作他们的运算关系，并返回结果
        ///</summary>
        private void AttrRecheck()
        {
            _controlState.Origin();
            this._prop.Zero();

            for (var i = 0; i < buffProp.Length; i++) buffProp[i].Zero();
            for (int i = 0; i < this.buffs.Count; i++)
            {
                for (int j = 0; j < Mathf.Min(buffProp.Length, buffs[i].model.propMod.Length); j++)
                {
                    buffProp[j] += buffs[i].model.propMod[j] * buffs[i].stack;
                }
                _controlState += buffs[i].model.stateMod;
            }

            this._prop = (this.baseProp + this.equipmentProp + this.buffProp[0]) * this.buffProp[1];

            if (unitMove)
            {
                unitMove.bodyRadius = this._prop.bodyRadius;
            }
        }

        ///<summary>
        ///增加角色的血量等资源，直接改变数字的，属于最后一步操作了
        ///<param name="value">要改变的量，负数为减少</param>
        ///</summary>
        public void ModResource(ChaResource value)
        {
            this.resource += value;
            this.resource.hp = Mathf.Clamp(this.resource.hp, 0, this._prop.hp);
            this.resource.ammo = Mathf.Clamp(this.resource.ammo, 0, this._prop.ammo);
            this.resource.stamina = Mathf.Clamp(this.resource.stamina, 0, 100);
            if (this.resource.hp <= 0)
            {
                this.Kill();
            }
        }


        ///<summary>
        ///在角色身上放一个特效，其实是挂在一个gameObject而已
        ///<param name="bindPointKey">绑点名称，角色有Muzzle/Head/Body这3个，需要再加</param>
        ///<param name="effect">要播放的特效文件名，统一走Prefabs/下拿</param>
        ///<param name="effectKey">这个特效的key，要删除的时候就有用了</param>
        ///<param name="effect">要播放的特效</param>
        ///</summary>
        public void PlaySightEffect(string bindPointKey, string effect, string effectKey = "", bool loop = false)
        {
            bindPoints.AddBindGameObject(bindPointKey, "Prefabs/" + effect, effectKey, loop);
        }

        ///<summary>
        ///删除角色身上的一个特效
        ///<param name="bindPointKey">绑点名称，角色有Muzzle/Head/Body这3个，需要再加</param>
        ///<param name="effectKey">这个特效的key，要删除的时候就有用了</param>
        ///</summary>
        public void StopSightEffect(string bindPointKey, string effectKey)
        {
            bindPoints.RemoveBindGameObject(bindPointKey, effectKey);
        }

        ///<summary>
        ///判断这个角色是否会被这个damageInfo所杀
        ///<param name="dInfo">要判断的damageInfo</param>
        ///<return>如果是true代表角色可能会被这次伤害所杀</return>
        ///</summary>
        public bool CanBeKilledByDamageInfo(DamageInfo damageInfo)
        {
            if (this.immuneTime > 0 || damageInfo.isHeal() == true) return false;
            //这里Demo只是单纯为了处理暴击逻辑，本质作用是处理在Buff之外的可以对DamageInfo（伤害值）产生影响的逻辑。
            int dValue = damageInfo.DamageValue(false); 
            return dValue >= this.resource.hp;
        }

        ///<summary>
        ///为角色添加buff，当然，删除也是走这个的（其实这样来看AddBuffInfo就应该设置成BuffInfo，就是在添加和移除Buff的过程中作为一个临时对象来传递Buff信息，而且AddBuff也应该改成AddOrRemoveBuff更加准确）
        ///</summary>
        public void AddBuff(AddBuffInfo buff) //利用AddBuffInfo添加Buff
        {
            List<GameObject> bCaster = new List<GameObject>(); //虽然只有一个元素但还是搞成容器，是为了与含有多个元素时的情况共用GetBuffById方法，当然其实也可以用重载方法。
            if (buff.caster) bCaster.Add(buff.caster);
            List<BuffObj> hasOnes = GetBuffById(buff.buffModel.id, bCaster); 
            int modStack = Mathf.Min(buff.addStack, buff.buffModel.maxStack); //最大maxStack因为在层数减到非正数时就会直接移除Buff，所以添加层数是不可能大于maxStack的。
            bool toRemove = false;
            BuffObj toAddBuff = null;
            //如果自身已经有了对应Buff的话，就只是影响对应BuffObj的一些属性值。
            if (hasOnes.Count > 0)
            { //hasOnes虽然是容器，但其实只会用到第一个元素。
              //已经存在
                hasOnes[0].buffParam = new Dictionary<string, object>();
                //TODO：完全更新参数，已有键就会覆盖其值，没有键就会新增。但是感觉这里很没有规范，键是string，值是object，充满了任意性，没有标准。
                if (buff.buffParam != null)
                {
                    foreach (KeyValuePair<string, object> kv in buff.buffParam) { hasOnes[0].buffParam[kv.Key] = kv.Value; }
                    ;
                }
                //持续时间，覆盖或者相加。
                hasOnes[0].duration = (buff.durationSetTo == true) ? buff.duration : (buff.duration + hasOnes[0].duration);
                //Ques：因为需要考虑边界，所以先对相加之后的结果进行一些判断来“修正”modStack值以便随后实际的结果符合预设标准。但是我很疑惑的是，为什么不直接相加之后，再进行判断，然后修正hasOnes[0].stack的值呢？
                int afterAdd = hasOnes[0].stack + modStack;
                modStack = afterAdd >= hasOnes[0].model.maxStack ?
                    (hasOnes[0].model.maxStack - hasOnes[0].stack) :
                    (afterAdd <= 0 ? (0 - hasOnes[0].stack) : modStack);

                hasOnes[0].stack += modStack; //modStack为负数的话，就是移除Buff。
                hasOnes[0].permanent = buff.permanent;
                toAddBuff = hasOnes[0];
                toRemove = hasOnes[0].stack <= 0; //一般可以叠层的Buff，在图标显示上，往往是第一层不显示数字，到了第二层时才会开始显示层数。
            }
            else
            {
                //新建
                toAddBuff = new BuffObj(
                    buff.buffModel,
                    buff.caster,
                    this.gameObject,
                    buff.duration,
                    buff.addStack,
                    buff.permanent,
                    buff.buffParam
                );
                buffs.Add(toAddBuff);
                buffs.Sort((a, b) => //添加了新Buff，就根据优先级重新排序整理一下。
                {
                    return a.model.priority.CompareTo(b.model.priority);
                });
            }
            //触发Buff的第一个回调onOccur
            if (toRemove == false && buff.buffModel.onOccur != null)
            {
                buff.buffModel.onOccur(toAddBuff, modStack); //注意这里的modStack是经过修正的，大概是指的“之前层数与当前层级的差值（变化值）”。
            }
            AttrRecheck(); //更新属性值
        }

        ///<summary>
        ///获取（自己）角色身上对应的buffObj
        ///<param name="id">buff的model的id</param>
        ///<param name="caster">如果caster不是空，那么就代表只有buffObj.caster在caster里面的才符合条件</param>
        ///<return>符合条件的buffObj数组</return>
        ///</summary>
        public List<BuffObj> GetBuffById(string id, List<GameObject> caster = null)
        {
            List<BuffObj> res = new List<BuffObj>();
            for (int i = 0; i < this.buffs.Count; i++)
            {
                //首先是id相同，也就是源数据BuffModel是同一个。然后是要么没有caster，要么就是与之前向本角色添加该buff的caster是同一个
                /*TODO：这里的条件设置我估计不一定，这里还要判断是否是同一个caster，或许也可以不管是否是同一个caster，或者不同caster添加同一个Buff可以叠加、但同一个caster就不能叠加，之类的设计，
                总之应该不一定就是这里的条件。*/
                if (buffs[i].model.id == id && (caster == null || caster.Count <= 0 || caster.Contains(buffs[i].caster) == true))
                {
                    res.Add(buffs[i]);
                }
            }
            return res;
        }

        ///<summary>
        ///根据id获得角色学会的技能（skillObj），如果没有则返回null
        ///<param name="id">技能的id</param>
        ///<return>skillObj or null</return>
        ///</summary>
        public SkillObj GetSkillById(string id)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i].model.id == id)
                {
                    return skills[i];
                }
            }
            return null;
        }

        ///<summary>
        ///释放一个技能，释放技能并不总是成功的，如果你一直发释放技能的命令，那失败率应该是骤增的
        ///<param name="id">要释放的技能的id</param>
        ///<return>是否释放成功</return>
        ///</summary>
        public bool CastSkill(string id)
        {
            if (this.controlState.canUseSkill == false) return false; //不能用技能就不放了
            SkillObj skillObj = GetSkillById(id);
            /*TODO：这里是检查技能能否释放的最前提条件，冷却时间当然是其中一个条件，但其实也要看具体什么游戏，总之这里的最前提条件应该放在一个方法里，比如SkillObj的一个公开方法中，
            让它在内部判断，然后返回判断结果即可*/
            if (skillObj == null || skillObj.cooldown > 0) return false;
            bool castSuccess = false;
            //个体资源足够释放技能
            if (this.resource.Enough(skillObj.model.condition) == true)
            {
                //Timeline结构天然与技能相对应。
                TimelineObj timeline = new TimelineObj(
                    skillObj.model.effect, this.gameObject, skillObj
                );
                //首先技能要经过自身所拥有的Buff作用。
                for (int i = 0; i < buffs.Count; i++)
                {
                    /*Tip：看起来是每次都要遍历，似乎很消耗性能，但实际上很多Buff的多数回调点可能都是空的，因为一个Buff的作用本来就很有限，而设置这么多回调点只是为了所有Buff
                    都能够使用这同一个Buff类型，所以在这样一个遍历中大概率实际执行的Buff是所拥有的Buff中的极少数。*/
                    if (buffs[i].model.onCast != null)
                    {
                        timeline = buffs[i].model.onCast(buffs[i], skillObj, timeline);
                    }
                }
                if (timeline != null)
                {
                    this.ModResource(-1 * skillObj.model.cost); //消耗资源。
                    SceneVariants.CreateTimeline(timeline); //前面是准备好了Timeline实例以及其各个成员，现在将其注册到TimelineManager中参与统一处理。
                    castSuccess = true;
                }

            }
            /*TODO：当然，对于无法释放技能，技能释放失败，等等情况，都还需要扩展逻辑，所以大概还需要考虑留出一些对应的回调点*/
            skillObj.cooldown = 0.1f;   //无论成功与否，都会进入gcd
            return castSuccess;
        }

        ///<summary>
        ///初始化角色的属性
        ///</summary>
        public void InitBaseProp(ChaProperty cProp)
        {
            this.baseProp = cProp;
            this.AttrRecheck();
            this.resource.hp = this._prop.hp;
            this.resource.ammo = this._prop.ammo;
            this.resource.stamina = 100;
        }

        ///<summary>
        ///学习某个技能
        ///<param name="skillModel">技能的模板</param>
        ///<param name="level">技能等级</param>
        ///</summary>
        public void LearnSkill(SkillModel skillModel, int level = 1)
        {
            this.skills.Add(new SkillObj(skillModel, level));
            //从技能获取Buff，应该类似于从穿上装备获取的Buff。
            /*Tip：这里的Buff本质上应该是技能的被动效果，也就是常驻Buff*/
            if (skillModel.buff != null)
            {
                for (int i = 0; i < skillModel.buff.Length; i++)
                {
                    AddBuffInfo abi = skillModel.buff[i];
                    abi.permanent = true;
                    abi.duration = 10;
                    abi.durationSetTo = true;
                    this.AddBuff(abi);
                }
            }
        }

        ///<summary>
        ///设置视觉元素
        ///</summary>
        public void SetView(GameObject view, Dictionary<string, AnimInfo> animInfo)
        {
            if (view == null) return;
            synchronizedUnits();
            view.transform.SetParent(viewContainer.transform);
            view.transform.position = new Vector3(0, this.gameObject.transform.position.y, 0);
            this.gameObject.transform.position = new Vector3(
                this.gameObject.transform.position.x,
                0,
                this.gameObject.transform.position.z
            );
            this.gameObject.GetComponent<UnitAnim>().animInfo = animInfo;
        }

        ///<summary>
        ///设置无敌时间
        ///<param name="time">无敌的时间，单位：秒</param>
        ///</summary>
        public void SetImmuneTime(float time)
        {
            this._immuneTime = Mathf.Max(this._immuneTime, time);
        }

        ///<summary>
        ///是否拥有某个tag
        ///</summary>
        public bool HasTag(string tag)
        {
            if (this.tags == null || this.tags.Length <= 0) return false;
            for (int i = 0; i < this.tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    return true;
                }
            }
            return false;
        }
    }
}