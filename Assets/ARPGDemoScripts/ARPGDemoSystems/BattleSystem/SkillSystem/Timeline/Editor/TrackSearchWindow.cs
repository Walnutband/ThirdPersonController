using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    public class TrackSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        public Action<TimelineTrack, Type> entryCallback;
        private List<Type> types;

        public TrackSearchWindowProvider(Action<TimelineTrack, Type> entryCallback) => this.entryCallback = entryCallback;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (types == null)
            {
                types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(TimelineTrack)))
                .ToList();
            }

            List<SearchTreeEntry> tree = new List<SearchTreeEntry>() { new SearchTreeGroupEntry(new GUIContent("Tracks"), 0) };


            types.ForEach(type =>
            {
                // tree.Add(new SearchTreeEntry(new GUIContent(type.Name.Substring(0, type.Name.Length - "Track".Length))) { level = 1, userData = type });
                tree.Add(new SearchTreeEntry(new GUIContent(type.Name.Substring("TimelineTrack_".Length))) { level = 1, userData = type });
                Debug.Log(type.Name);
            });
            return tree;
            // foreach (Type type in types)
            // {
            //     tree.Add(new SearchTreeGroupEntry(new GUIContent(type.Name.)));
            // }
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Type type = SearchTreeEntry.userData as Type;
            if (type != null)
            {
                Debug.Log("typeName: "+ type.Name);
                var track = Activator.CreateInstance(type) as TimelineTrack;
                Debug.Log($"{(track == null ? "track为空": "track不为空")}");
                entryCallback(track, type);
                return true;
            }
            return false;
        }
    }
}