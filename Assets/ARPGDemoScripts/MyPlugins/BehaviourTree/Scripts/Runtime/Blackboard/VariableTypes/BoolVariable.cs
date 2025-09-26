using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
	//Tip:泛型的常用技巧，派生一个具有确定含义类名的非泛型类，然后继承自确定泛型参数的泛型基类。
	public class BoolVariable : Variable<bool>
	{
	}

}