using NUnit.Framework.Interfaces;
using RPGCore.Dialogue.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGCore.Dialogue.Editor
{
	/*Tip：这是对话编辑器的核心控件，即节点视图，并非编辑窗口本身。*/
	public class DialogueEditorGraphView : GraphView
	{
		private DialogueEditorWindow editorWindow;
		private DialogueEditorSearchWindow searchWindow;

		/*Tip: 没有搞成自定义控件，大概是因为构造函数需要传入所在的EditorWindow，而Uxml工厂需要无参数的构造函数才能注册。*/
		public DialogueEditorGraphView(DialogueEditorWindow window)
		{
			this.editorWindow = window;
			//加载用于指定GridBackground样式的uss文件。由于uss文件是按照样式系统的规则来应用的，所以放在根元素的styleSheets容器中即可，按照规则作用于自身以及所有下层元素。
			styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphView"));
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			Insert(0, new GridBackground());
			MakeSearchTree();
		}

		private void MakeSearchTree()
		{
			searchWindow = ScriptableObject.CreateInstance<DialogueEditorSearchWindow>();
			searchWindow.Init(editorWindow, this);
			nodeCreationRequest = context =>
			{ 
				if (editorWindow.CanEditor)
				{
					
					// SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);

				}
			};
		}

		//连接两个节点时调用 获取到当前节点端口能够连接到的其余节点端口
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			List<Port> compatiblePorts = new List<Port>();
			ports.ForEach(
				(port) =>
				{
					if (startPort.node != port.node && //不能自己连接自己
						startPort.direction != port.direction)//不能input连input output连output
					{
						compatiblePorts.Add(port);
					}
				}
			);
			return compatiblePorts;
		}

		/// <summary>
		/// 在节点视图的指定位置生成节点
		/// </summary>
		/// <param name="dgNode">数据节点</param>
		/// <param name="position"></param>
		/// <returns></returns>
		/// <remarks>生成的是UI层的节点，应该叫做视图节点，就是这里的graphNode</remarks>
		public DialogueGraphNode MakeNode(DgNodeBase dgNode, Vector2 position)
		{
			DialogueGraphNode graphNode = GenerateGraphNode(dgNode, editorWindow);
			//设置位置和尺寸
			graphNode.SetPosition(new Rect(position, graphNode.GetPosition().size));
			AddElement(graphNode);
			return graphNode;
		}

		public Edge MakeEdge(Port oput, Port iput)
		{
			var edge = new Edge { output = oput, input = iput };
			edge?.input.Connect(edge);
			edge?.output.Connect(edge);
			AddElement(edge);
			return edge;
		}

		public Edge MakeEdge(DialogueGraphNode outputNode, DialogueGraphNode inputNode, int outputPortIndex = 0)
		{
			return MakeEdge(outputNode?.outputPorts[outputPortIndex], inputNode?.inputPort);
		}

		private DialogueGraphNode GenerateGraphNode(DgNodeBase nodeData, DialogueEditorWindow editorWindow)
		{
			List<Type> graphNodeTypes = new List<Type>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name.Contains("Assembly")))
			{
				List<Type> types = assembly.GetTypes().Where(type =>
				{//Tip：注意这里是DialogueGraphNodeAttribute，指的是视图节点，而还有个DialogueNodeAttribute其实指的是数据节点。
					return type.IsClass && !type.IsAbstract && type.GetCustomAttribute<DialogueGraphNodeAttribute>() != null;
				}).ToList();
				graphNodeTypes.AddRange(types);
			}
			//TODO：通过枚举类型DgNodeType来判断是哪个类型
			foreach (var graphNodeType in graphNodeTypes)
			{
				if (graphNodeType.GetCustomAttribute<DialogueGraphNodeAttribute>().Type == nodeData.Type)
				{
					return Activator.CreateInstance(graphNodeType, args: new object[] { nodeData, editorWindow }) as DialogueGraphNode;
				}
			}
			return null;
		}

		/// <summary>
		/// 生成节点视图
		/// </summary>
		/// <param name="itemData"></param>
		public void GenerateNodeGraphView(DialogueItemDataSO itemData)
		{
			if (itemData == null)
			{
				return;
			}
			//生成默认的节点
			if (itemData.dgNodes.Count == 0)
			{
				var start = MakeNode(DialogueEditorUtility.CreateDialogueNodeData<DgNodeStart>(editorWindow.CurrentOpenedGroupData), new Vector2(350, 300));
				var end = MakeNode(DialogueEditorUtility.CreateDialogueNodeData<DgNodeEnd>(editorWindow.CurrentOpenedGroupData), new Vector2(500, 300));
				MakeEdge(start, end);
				return;
			}
			//生成节点
			List<DgNodeBase> nullNodes = new List<DgNodeBase>();
			foreach (var node in itemData.dgNodes)
			{
				if(node==null)nullNodes.Add(node);
				else MakeNode(node, node.graphViewPosition);
			}
			//删除莫名其妙出现的空节点 问题可能出现在SaveNodeGraphView中
			foreach (var node in nullNodes) 
			{
				itemData.dgNodes.Remove(node);
			}
			//连接节点
			var graphNodeList = nodes.Select(node => node as DialogueGraphNode).ToList();
			foreach (var node in graphNodeList)
			{
				Port outputPort = null;
				Port inputPort = null;
				int outputPortIndex = 0;
				foreach (var guid in node.nodeData.nextNodesGuid)
				{
					if (!string.IsNullOrWhiteSpace(guid))
					{
						var nextgNode = graphNodeList.Find(node => node.nodeData.Guid == guid);
						outputPort = node.outputPorts[outputPortIndex];
						inputPort = nextgNode.inputPort;
						MakeEdge(outputPort, inputPort);
					}
					if (node.outputPorts.Count - 1 > outputPortIndex)
					{
						outputPortIndex++;
					}
				}
			}
			UpdateViewTransform(itemData.GraphViewPortPosition, itemData.GraphViewPortScale);
		}

		public void SaveNodeGraphView()
		{
			if (editorWindow.CurrentOpenedGroupData == null) return;
			//将移除的节点从资源中删除
			List<DgNodeBase> nodesToRemove = new List<DgNodeBase>();
			DialogueItemDataSO itemData = editorWindow.CurrentOpenedGroupData.GetOpenedEditorItem();
			foreach (var node in itemData.dgNodes)
			{
				if (!nodes.Select(node => (node as DialogueGraphNode).nodeData).Contains(node))
				{
					nodesToRemove.Add(node);
				}
			}
			foreach (var rnode in nodesToRemove)
			{
				DialogueEditorUtility.DeleteDialogueNodeData(rnode);
				itemData.dgNodes.Remove(rnode);
			}
			nodesToRemove.Clear();
			//删除所有的空引用
			List<int> indexToRemove = new();
			List<DgNodeBase> dgNodes = itemData.dgNodes;
			for (int i = 0; i < dgNodes.Count; i++)
			{
				if (dgNodes[i] == null)
				{
					indexToRemove.Add(i);
				}
			}
			for (int i = 0; i < indexToRemove.Count; i++)
			{
				dgNodes.RemoveAt(indexToRemove[i]);
			}
			//保存位置并清空链接关系
			foreach (var node in nodes.Select(node => node as DialogueGraphNode))
			{
				node.nodeData.graphViewPosition = node.GetPosition().position;
				node.nodeData.nextNodesGuid.Clear();
			}
			//保存链接信息
			foreach (var edge in edges.ToList())
			{
				if (edge.output == null || edge.input == null) break;
				var outputNode = edge.output.node as DialogueGraphNode;
				var inputNode = edge.input.node as DialogueGraphNode;
				int outputPortIndex = outputNode.outputPorts.FindIndex(port => port.portName == edge.output.portName);
				if (edge.output.capacity == Port.Capacity.Multi)
				{
					outputNode.nodeData.nextNodesGuid.Add(inputNode.nodeData.Guid);
				}
				else
				{
					if (outputPortIndex > outputNode.nodeData.nextNodesGuid.Count - 1)
					{
						int count = outputPortIndex - outputNode.nodeData.nextNodesGuid.Count + 1;
						outputNode.nodeData.nextNodesGuid.AddRange(new string[count]);
					}
					outputNode.nodeData.nextNodesGuid[outputPortIndex] = inputNode.nodeData.Guid;
				}
			}
			//保存当前graphview的位置缩放信息
			itemData.SaveGraphViewPortInfomation(viewTransform.position, viewTransform.scale);

			EditorUtility.SetDirty(editorWindow.CurrentOpenedGroupData);
			EditorUtility.SetDirty(itemData);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			DialogueEditorUtility.MakeSureDataLegalization(editorWindow.CurrentOpenedGroupData);
		}

		/*Tip：清空整个节点视图，其实就是删除所有的节点和边，但GraphView中可以存在的元素远不止于Node和Edge，所以应该考虑到更多的扩展。
		在切换Item或Group的时候，都是先清空再根据数据重新生成这些UI元素。
		*/
		public void ClearGraphView()
		{
			foreach (var node in nodes)
			{
				RemoveElement(node);
			}
			foreach (var edge in edges)
			{
				RemoveElement(edge);
			}
		}
	}
}
