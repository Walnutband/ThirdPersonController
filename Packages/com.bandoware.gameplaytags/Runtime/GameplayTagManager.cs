using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   public class GameplayTagManager
   {
      public static bool HasBeenReloaded => s_HasBeenReloaded;

      private static Dictionary<string, GameplayTagDefinition> s_TagDefinitionsByName = new();
      private static GameplayTagDefinition[] s_TagsDefinitions;
      private static GameplayTag[] s_Tags;
      private static bool s_IsInitialized;
      private static bool s_HasBeenReloaded;

      public static ReadOnlySpan<GameplayTag> GetAllTags()
      {
         InitializeIfNeeded();
         return new ReadOnlySpan<GameplayTag>(s_Tags);
      }

      internal static GameplayTagDefinition GetDefinitionFromRuntimeIndex(int runtimeIndex)
      {
         InitializeIfNeeded();
         return s_TagsDefinitions[runtimeIndex];
      }

      public static GameplayTag RequestTag(string name, bool logWarningIfNotFound = true)
      {
         if (string.IsNullOrEmpty(name))
            return GameplayTag.None;

         if (!TryGetDefinition(name, out GameplayTagDefinition definition))
         {
            if (logWarningIfNotFound)
               Debug.LogWarning($"No tag registered with name \"{name}\".");

            return GameplayTagDefinition.CreateInvalidDefinition(name).Tag;
         }

         return definition.Tag;
      }

      public static bool RequestTag(string name, out GameplayTag tag)
      {
         GameplayTag result = RequestTag(name, logWarningIfNotFound: false);
         tag = result;
         return tag.IsValid && !tag.IsNone;
      }

      private static bool TryGetDefinition(string name, out GameplayTagDefinition definition)
      {
         InitializeIfNeeded();
         return s_TagDefinitionsByName.TryGetValue(name, out definition);
      }

      public static void InitializeIfNeeded()
      {
         if (s_IsInitialized)
            return;

         GameplayTagRegistrationContext context = new();

#if UNITY_EDITOR

         // Register tags from all assemblies with the GameplayTagAttribute attribute.
         foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
         {
            AssemblyGameplayTagSource source = new(assembly);
            source.RegisterTags(context);
         }

         // Register tags from all JSON files in the ProjectSettings/GameplayTags directory.
         foreach (IGameplayTagSource source in FileGameplayTagSource.GetAllFileSources())
            source.RegisterTags(context);

#else

         // Register tags from the GameplayTags file in StreamingAssets.   
         BuildGameplayTagSource buildSource = new();
         buildSource.RegisterTags(context);

#endif

         foreach (GameplayTagRegistrationError error in context.GetRegistrationErrors())
            Debug.LogError($"Failed to register gameplay tag \"{error.TagName}\": {error.Message} (Source: {error.Source?.Name ?? "Unknown"})");

         s_TagsDefinitions = context.GenerateDefinitions();

         // Skip the first tag definition which is the "None" tag.
         IEnumerable<GameplayTag> tags = s_TagsDefinitions
            .Select(definition => definition.Tag)
            .Skip(1);

         s_Tags = Enumerable.ToArray(tags);
         foreach (GameplayTagDefinition definition in s_TagsDefinitions)
            s_TagDefinitionsByName[definition.TagName] = definition;

         s_IsInitialized = true;
      }

      public static void ReloadTags()
      {
         s_IsInitialized = false;
         s_TagDefinitionsByName.Clear();

         InitializeIfNeeded();

         s_HasBeenReloaded = true;

         if (Application.isPlaying)
            Debug.LogWarning("Gameplay tags have been reloaded at runtime." +
               " Existing data structures using gameplay tags may not work as expected." +
               " A domain reload is required.");
      }
   }
}