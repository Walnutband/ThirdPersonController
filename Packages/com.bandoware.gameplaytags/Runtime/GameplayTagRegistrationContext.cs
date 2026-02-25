using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace BandoWare.GameplayTags
{
   internal class GameplayTagRegistrationError
   {
      public string Message { get; }
      public IGameplayTagSource Source { get; }
      public string TagName { get; }

      public GameplayTagRegistrationError(string message, IGameplayTagSource source, string tagName)
      {
         Message = message;
         Source = source;
         TagName = tagName;
      }
   }

   internal class GameplayTagRegistrationContext
   {
      private List<GameplayTagDefinition> m_Definition = new();
      private Dictionary<string, GameplayTagDefinition> m_TagsByName = new();
      private string m_LastRegistrarionErrorMessage;
      private List<GameplayTagRegistrationError> m_RegistrationErrors = new();

      public bool RegisterTag(string name, string description, GameplayTagFlags flags, IGameplayTagSource source)
      {
         Assert.IsNotNull(source, "Source cannot be null when registering a tag.");
         return RegisterTagInternal(name, description, flags, source);
      }

      private bool RegisterTagInternal(string name, string description, GameplayTagFlags flags, IGameplayTagSource source)
      {
         if (!GameplayTagUtility.IsNameValid(name, out string errorMessage))
            m_RegistrationErrors.Add(new GameplayTagRegistrationError(errorMessage, source, name));

         if (m_TagsByName.TryGetValue(name, out GameplayTagDefinition existingDefinition))
         {
            existingDefinition.Description ??= description;

            if (source != null)
               existingDefinition.AddSource(source);

            return true;
         }

         GameplayTagDefinition definition = new(name, description, flags);

         if (source != null)
            definition.AddSource(source);

         m_TagsByName.Add(name, definition);
         m_Definition.Add(definition);

         return true;
      }

      public GameplayTagDefinition[] GenerateDefinitions()
      {
         RegisterMissingParents();
         SortDefinitionsAlphabetically();
         RegisterNoneTag();
         SetTagRuntimeIndices();
         FillParentsAndChildren();
         SetHierarchyTags();

         return m_Definition.ToArray();
      }

      private void RegisterNoneTag()
      {
         m_Definition.Insert(0, GameplayTagDefinition.NoneTagDefinition);
      }

      private void RegisterMissingParents()
      {
         List<GameplayTagDefinition> definitions = new(m_Definition);
         foreach (GameplayTagDefinition definition in definitions)
         {
            string[] parentTagNames = GameplayTagUtility.GetHeirarchyNames(definition.TagName);

            GameplayTagFlags flags = definition.Flags;
            foreach (string parentTagName in Enumerable.Reverse(parentTagNames))
            {
               if (m_TagsByName.TryGetValue(parentTagName, out GameplayTagDefinition parentTag))
               {
                  flags |= parentTag.Flags;
                  continue;
               }

               RegisterTagInternal(parentTagName, string.Empty, flags, null);
            }
         }
      }

      private void SortDefinitionsAlphabetically()
      {
         m_Definition.Sort((a, b) => string.Compare(a.TagName, b.TagName, StringComparison.OrdinalIgnoreCase));
      }

      private void FillParentsAndChildren()
      {
         Dictionary<GameplayTagDefinition, List<GameplayTagDefinition>> childrenLists = new();

         // Skip the first tag definition which is the "None" tag
         for (int i = 1; i < m_Definition.Count; i++)
         {
            GameplayTagDefinition definition = m_Definition[i];
            string[] parentTagNames = GameplayTagUtility.GetHeirarchyNames(definition.TagName);
            for (int j = 0; j < parentTagNames.Length - 1; j++)
            {
               string parentTagName = parentTagNames[j];
               GameplayTagDefinition parentDefinition = m_TagsByName[parentTagName];
               if (!childrenLists.TryGetValue(parentDefinition, out List<GameplayTagDefinition> children))
               {
                  children = new();
                  childrenLists.Add(parentDefinition, children);
               }

               children.Add(definition);
            }
         }

         foreach ((GameplayTagDefinition definition, List<GameplayTagDefinition> children) in childrenLists)
         {
            definition.SetChildren(children);
            foreach (GameplayTagDefinition child in children)
               child.SetParent(definition);
         }
      }

      private void SetHierarchyTags()
      {
         for (int i = 1; i < m_Definition.Count; i++)
         {
            GameplayTagDefinition definition = m_Definition[i];

            List<GameplayTag> hierarcyTags = new();

            if (definition.ParentTagDefinition != null)
               hierarcyTags.AddRange(definition.ParentTagDefinition.HierarchyTags.ToArray());

            hierarcyTags.Add(definition.Tag);
            definition.SetHierarchyTags(hierarcyTags.ToArray());
         }
      }

      private void SetTagRuntimeIndices()
      {
         for (int i = 0; i < m_Definition.Count; i++)
            m_Definition[i].SetRuntimeIndex(i);
      }

      public IEnumerable<GameplayTagRegistrationError> GetRegistrationErrors()
      {
         return m_RegistrationErrors;
      }
   }
}