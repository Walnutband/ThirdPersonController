using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
	public class Vector3Variable : Variable<Vector3> //注意Vector3是一个值类型，如果用return的话，返回的是其副本。
	{
	}
}