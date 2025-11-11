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
	public class DialogueEditorSearchWindow : ScriptableObject, ISearchWindowProvider
	{
		private DialogueEditorGraphView graphView;
		private DialogueEditorWindow editorWindow;

		public void Init(DialogueEditorWindow editorWindow, DialogueEditorGraphView graphView)
		{
			this.editorWindow = editorWindow;
			this.graphView = graphView;
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
			//作为第一个组，按理来说，不管有没有节点，都应该有这样的第一个组，因为在搜索窗口中始终会显示当前组的标题。
			searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 0));
			List<Type> types = new List<Type>();
			/*Ques：这里就是将名字包含了“Assembly”的程序集的元数据加载进来，其实就是那些预定义的程序集，只是我很疑惑，为何不直接给这个对话系统定义专门的程序集，那样不是方便得多了？
			而且性能也更好。。。*/
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name.Contains("Assembly")))
			{
				List<Type> result = assembly.GetTypes().Where(type =>
				{
					return type.IsClass && !type.IsAbstract && type.GetCustomAttribute<DialogueNodeAttribute>() != null;
				}).ToList();
				types.AddRange(result);
			}
			//通过节点属性设置的路径和名称来构造一个树形结构节点分类
			List<SearchWindowMenuItem> mainMenu = new List<SearchWindowMenuItem>();
			foreach (Type type in types)
			{
				//获取节点属性的NodePath
				//Tip: Attribute定义路径和名称，反射获取Attribute，确实是一种好方式。
				string nodePath = type.GetCustomAttribute<DialogueNodeAttribute>()?.Path;
				if (nodePath == null) continue;
				//将路径中每一项分割
				string[] menus = nodePath.Split('/');
				//遍历分割的每一项的名称
				/*Tip：由于引用的是循环之外的容器对象，所以每个循环之间其实是共用的该容器。*/
				List<SearchWindowMenuItem> currentFloor = mainMenu;
				for (int i = 0; i < menus.Length; i++)
				{
					string currentName = menus[i];
					bool exist = false;
					//还不是最后一项说明当前项还是菜单项
					bool lastFloor = (i == (menus.Length - 1));
					//如果当前项能够在当前层中找到说明当前项已经存在
					SearchWindowMenuItem temp = currentFloor.Find(item => item.Name == currentName);
					if (temp != null)
					{
						exist = true;
						//将当前项下的子项作为下一层。也就是找路径
						currentFloor = temp.ChildItems;
					}
					//当前项不存在 就构造当前项并加入到当前层级中
					if (!exist)
					{
						SearchWindowMenuItem item = new SearchWindowMenuItem() { Name = currentName, IsNode = lastFloor };
						currentFloor.Add(item);
						//如果当前项不是节点 且没有下一层
						if (!item.IsNode && item.ChildItems == null)
						{
							//构造新的子级层
							item.ChildItems = new List<SearchWindowMenuItem>();
						}
						//一个节点就对应一个节点类型。
						if (item.IsNode) item.type = type;
						currentFloor = item.ChildItems;
					}
				}
			}
			/*Tip：注意理解SearchTreeEntry的Level，从UI显示来看，位于一个SearchTreeGroupEntry中的SearchTreeEntry的level应该等于SearchTreeGroupEntry的level + 1，
			组的名字显示在上，组中的元素名字在显示在下面的滚动视图中。
			*/
			MakeSearchTree(mainMenu, 1, ref searchTreeEntries);
			return searchTreeEntries;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			//获取到当前鼠标的位置
			var worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(
				editorWindow.rootVisualElement.parent,
				context.screenMousePosition - editorWindow.position.position
			);
			var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousePosition);
			Type type = (Type)SearchTreeEntry.userData;
			DialogueGraphNode node = graphView.MakeNode(DialogueEditorUtility.CreateDialogueNodeData(type, editorWindow.CurrentOpenedGroupData), localMousePosition);
			//Undo.RegisterCreatedObjectUndo(node, "");
			return true;
		}

		//根据构造的节点目录结构构造最终的节点创建目录
		/*Tip：这里应该算是一个典型的树结构问题的递归解决算法。*/
		private void MakeSearchTree(List<SearchWindowMenuItem> floor, int floorIndex, ref List<SearchTreeEntry> treeEntries)
		{
			foreach (var item in floor)
			{
				//当前项不是节点
				if (!item.IsNode)
				{
					//构造一层
					SearchTreeEntry entry = new SearchTreeGroupEntry(new GUIContent(item.Name))
					{
						level = floorIndex,
					};
					treeEntries.Add(entry);
					//进入当前项的下一层继续构造，注意节点的level应当等于所属组的level + 1，所以这里传入的是floorIndex + 1
					MakeSearchTree(item.ChildItems, floorIndex + 1, ref treeEntries);
				}
				//当前项是节点
				else
				{
					//构造节点项 回到顶层 继续构造
					//注意这里的空格，因为选择项是紧贴搜索窗口左边界的，所以这里留出一点空间，但我感觉没啥区别。
					SearchTreeEntry entry = new SearchTreeEntry(new GUIContent("     " + item.Name))
					{
						userData = item.type,
						level = floorIndex
					};
					treeEntries.Add(entry);
				}
			}
		}

		//构造SearchWindow时 用来存储节点目录的结构
		public class SearchWindowMenuItem
		{
			//目录项的名称（可能是目录，可能是节点，总之就是出现在搜索窗口中的搜索项）
			public string Name { get; set; }

			//当前目录项是否是节点
			public bool IsNode { get; set; }

			public Type type;
			//因为目录结构就是树结构。
			public List<SearchWindowMenuItem> ChildItems { get; set; }
		}
	}
}
