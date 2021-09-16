﻿// borrowed shamelessly and enhanced from Bag of Tricks https://www.nexusmods.com/pathfinderkingmaker/mods/26, which is under the MIT License

using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Assets.Controllers.GlobalMap;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
//using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.Rest.Cooking;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Dungeon;
using Kingmaker.Dungeon.Blueprints;
using Kingmaker.Dungeon.Units.Debug;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Formations;
using DG.Tweening;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Items;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.Kingdom.UI;
using Kingmaker.PubSubSystem;
using Kingmaker.RandomEncounters;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.TextTools;
using Kingmaker.UI;
//using Kingmaker.UI._ConsoleUI.Models;
using Kingmaker.UI.Common;
using Kingmaker.UI.FullScreenUITypes;
using Kingmaker.UI.Group;
using Kingmaker.UI.IngameMenu;
using Kingmaker.UI.Kingdom;
using Kingmaker.UI.Log;
using Kingmaker.UI.MainMenuUI;
using Kingmaker.UI.MVVM;
using Kingmaker.UI.MVVM._PCView.CharGen;
using Kingmaker.UI.MVVM._PCView.CharGen.Phases;
using Kingmaker.UI.MVVM._PCView.CharGen.Phases.Mythic;
//using Kingmaker.UI.RestCamp;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.ServiceWindow.LocalMap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.Spawners;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.LocalMap;
using Kingmaker.Visual.Sound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
//using Kingmaker.UI._ConsoleUI.GroupChanger;
using Kingmaker.UI.ActionBar;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;
using TMPro;
using TurnBased.Controllers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Kingmaker.UnitLogic.Class.LevelUp.LevelUpState;
using UnityModManager = UnityModManagerNet.UnityModManager;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.Skills;
using Kingmaker.UI.MVVM._VM.CharGen.Phases.FeatureSelector;
using Kingmaker.UI.MVVM._VM.CharGen;
using ModKit.Utility;
using ModKit;

namespace ToyBox.BagOfPatches {
    static class LevelUp {
        public static Settings settings = Main.settings;
        public static UnityModManager.ModEntry.ModLogger modLogger = ModKit.Logger.modLogger;
        public static Player player = Game.Instance.Player;

        [HarmonyPatch(typeof(LevelUpController), "CanLevelUp")]
        static class LevelUpController_CanLevelUp_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleNoLevelUpRestrictions) {
                    __result = true;
                }
            }
        }

        // ignoreAttributesPointsRemainng
        [HarmonyPatch(typeof(StatsDistribution), "IsComplete")]
        static class StatsDistribution_IsComplete_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleIgnoreAttributePointsRemaining) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(SpendAttributePoint), "Check")]
        static class SpendAttributePoint_Check_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleIgnoreAttributePointsRemaining) {
                    __result = true;
                }
            }
        }
        // ignoreAttributeCap
        [HarmonyPatch(typeof(StatsDistribution), "CanAdd", new Type[] { typeof(StatType) })]
        static class StatsDistribution_CanAdd_Patch {
            /*
            public static bool Prefix() {
                return !settings.toggleIgnoreAttributeCap;
            }
            
            private static void Postfix(ref bool __result, StatsDistribution __instance, StatType attribute) {
               __result = __instance.Available 
                    && (settings.toggleIgnoreAttributeCap || __instance.StatValues[attribute] < 18)
                    && (__instance.GetAddCost(attribute) <= __instance.Points);
            }
            */
            private static void Postfix(ref bool __result, StatsDistribution __instance) {
                if (settings.toggleIgnoreAttributeCap && __instance.Available) {
                    __result = true;
                }
            }
        }
        // ignoreSkillPointsRemaining
        [HarmonyPatch(typeof(CharGenSkillsPhaseVM), "SelectionStateIsCompleted")]
        static class CharGenSkillsPhaseVM_SelectionStateIsCompleted_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleIgnoreSkillPointsRemaining) {
                    __result = true;
                }
            }
        }
        // ignoreSkillPointsRemaing, ignoreSkillCap
        [HarmonyPatch(typeof(SpendSkillPoint), "Check", new Type[] { typeof(LevelUpState), typeof(UnitDescriptor) })]
        static class SpendSkillPoint_Check_Patch {
            public static bool Prefix(SpendSkillPoint __instance) {
                return !(settings.toggleIgnoreSkillCap || settings.toggleIgnoreSkillPointsRemaining);
            }
            private static void Postfix(ref bool __result, SpendSkillPoint __instance, LevelUpState state, UnitDescriptor unit) {
                __result = (StatTypeHelper.Skills).Contains<StatType>(__instance.Skill)
                    && (settings.toggleIgnoreSkillCap || unit.Stats.GetStat(__instance.Skill).BaseValue < state.NextCharacterLevel)
                    && (settings.toggleIgnoreSkillPointsRemaining || state.SkillPointsRemaining > 0);
            }
        }
        // ignoreSkillCap
        [HarmonyPatch(typeof(CharGenSkillAllocatorVM), "UpdateSkillAllocator")]
        static class CharGenSkillAllocatorVM_UpdateSkillAllocator_Patch {
            public static bool Prefix(CharGenSkillAllocatorVM __instance) {
                if (settings.toggleIgnoreSkillCap) {
                    __instance.IsClassSkill.Value = (bool)__instance.Skill?.ClassSkill;
                    ModifiableValue stat1 = __instance.m_LevelUpController.Unit.Stats.GetStat(__instance.StatType);
                    ModifiableValue stat2 = __instance.m_LevelUpController.Preview.Stats.GetStat(__instance.StatType);
                    __instance.CanAdd.Value = !__instance.m_LevelUpController.State.IsSkillPointsComplete() && __instance.m_LevelUpController.State.SkillPointsRemaining > 0;
                    __instance.CanRemove.Value = stat2.BaseValue > stat1.BaseValue;
                    return false;
                }
                return true;
            }
        }

        // full HD
        [HarmonyPatch(typeof(ApplyClassMechanics), "ApplyHitPoints", new Type[] { typeof(LevelUpState), typeof(ClassData), typeof(UnitDescriptor) })]
        static class ApplyClassMechanics_ApplyHitPoints_Patch {
            private static void Postfix(LevelUpState state, ClassData classData, ref UnitDescriptor unit) {
                if (settings.toggleFullHitdiceEachLevel && unit.IsPartyMemberOrPet() && state.NextClassLevel > 1) {

                    int newHitDie = ((int)classData.CharacterClass.HitDie / 2) - 1;
                    unit.Stats.HitPoints.BaseValue += newHitDie;
                }
#if false
                else if (StringUtils.ToToggleBool(settings.toggleRollHitDiceEachLevel) && unit.IsPartyMemberOrPet() && state.NextLevel > 1) {
                    int oldHitDie = ((int)classData.CharacterClass.HitDie / 2) + 1;
                    DiceFormula diceFormula = new DiceFormula(1, classData.CharacterClass.HitDie);
                    int roll = RuleRollDice.Dice.D(diceFormula);

                    unit.Stats.HitPoints.BaseValue -= oldHitDie;
                    unit.Stats.HitPoints.BaseValue += roll;
                }
#endif
            }
        }
        [HarmonyPatch(typeof(PrerequisiteFeature), "CheckInternal")]
        static class PrerequisiteFeature_CanLevelUp_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleIgnoreFeaturePrerequisites) {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(PrerequisiteFeaturesFromList), "CheckInternal")]
        static class PrerequisiteFeaturesFromList_CanLevelUp_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleIgnoreFeatureListPrerequisites) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(FeatureSelectionState), "IgnorePrerequisites", MethodType.Getter)]
        static class FeatureSelectionState_IgnorePrerequisites_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleFeaturesIgnorePrerequisites) {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(IgnorePrerequisites), "Ignore", MethodType.Getter)]
        static class IgnorePrerequisites_Ignore_Patch {
            private static void Postfix(ref bool __result) {
                if (settings.toggleIgnoreClassAndFeatRestrictions) {
                    __result = true;
                }
            }
        }
#if false
        [HarmonyPatch(typeof(CharGenMythicPhaseVM), "IsClassVisible")]
        static class CharGenMythicPhaseVM_IsClassVisible_Patch {
            private static void Postfix(ref bool __result, BlueprintCharacterClass charClass) {
                Logger.Log("IsClassVisible");
                if (settings.toggleIgnoreClassAndFeatRestrictions) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(CharGenMythicPhaseVM), "IsClassAvailableToSelect")]
        static class CharGenMythicPhaseVM_IsClassAvailableToSelect_Patch {
            private static void Postfix(ref bool __result, BlueprintCharacterClass charClass) {
                Logger.Log("CharGenMythicPhaseVM.IsClassAvailableToSelect");
                if (settings.toggleIgnoreClassAndFeatRestrictions) {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(CharGenMythicPhaseVM), "IsPossibleMythicSelection", MethodType.Getter)]
        static class CharGenMythicPhaseVM_IsPossibleMythicSelection_Patch {
            private static void Postfix(ref bool __result) {
                Logger.Log("CharGenMythicPhaseVM.IsPossibleMythicSelection");
                if (settings.toggleIgnoreClassAndFeatRestrictions) {
                    __result = true;
                }
            }
        }
#endif

        [HarmonyPatch(typeof(LevelUpController), "IsPossibleMythicSelection", MethodType.Getter)]
        static class LevelUpControllerIsPossibleMythicSelection_Patch {
            private static void Postfix(ref bool __result) {
                //Logger.Log($"LevelUpController.IsPossibleMythicSelection {settings.toggleIgnoreClassAndFeatRestrictions}");
                if (settings.toggleIgnoreClassAndFeatRestrictions) {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(PrerequisiteCasterTypeSpellLevel), "CheckInternal")]
        public static class PrerequisiteCasterTypeSpellLevel_Check_Patch {
            public static void Postfix(
                    [CanBeNull] FeatureSelectionState selectionState,
                    [NotNull] UnitDescriptor unit,
                    [CanBeNull] LevelUpState state,
                    ref bool __result) {
                if (!unit.IsPartyMemberOrPet()) return; // don't give extra feats to NPCs

                if (settings.toggleIgnoreCasterTypeSpellLevel) {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(PrerequisiteNoArchetype), "CheckInternal")]
        public static class PrerequisiteNoArchetype_Check_Patch {
            public static void Postfix(
                    [CanBeNull] FeatureSelectionState selectionState,
                    [NotNull] UnitDescriptor unit,
                    [CanBeNull] LevelUpState state,
                    ref bool __result) {
                if (!unit.IsPartyMemberOrPet()) return; // don't give extra feats to NPCs

                if (settings.toggleIgnoreForbiddenArchetype) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(PrerequisiteStatValue), "CheckInternal")]
        public static class PrerequisiteStatValue_Check_Patch {
            public static void Postfix(
                    [CanBeNull] FeatureSelectionState selectionState,
                    [NotNull] UnitDescriptor unit,
                    [CanBeNull] LevelUpState state,
                    ref bool __result) {
                if (!unit.IsPartyMemberOrPet()) return; // don't give extra feats to NPCs
                if (settings.toggleIgnorePrerequisiteStatValue) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(PrerequisiteAlignment), "CheckInternal")]
        public static class PrerequisiteAlignment_Check_Patch {
            public static void Postfix(
                    [CanBeNull] FeatureSelectionState selectionState,
                    [NotNull] UnitDescriptor unit,
                    [CanBeNull] LevelUpState state,
                    ref bool __result) {
                if (!unit.IsPartyMemberOrPet()) return; // don't give extra feats to NPCs

                if (settings.toggleIgnoreAlignmentWhenChoosingClass) {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(PrerequisiteNoFeature), "CheckInternal")]
        public static class PrerequisiteNoFeature_Check_Patch {
            public static void Postfix(
                    [CanBeNull] FeatureSelectionState selectionState,
                    [NotNull] UnitDescriptor unit,
                    [CanBeNull] LevelUpState state,
                    ref bool __result) {
                if (!unit.IsPartyMemberOrPet()) return; // don't give extra feats to NPCs
                if (settings.toggleIgnoreForbiddenFeatures) {
                    __result = true;
                }
            }
        }
#if false
        [HarmonyPatch(typeof(Spellbook), "AddCasterLevel")]
        public static class Spellbook_AddCasterLevel_Patch {
            public static bool Prefix() {
                return false;
            }

            public static void Postfix(ref Spellbook __instance, ref int ___m_CasterLevelInternal, List<BlueprintSpellList> ___m_SpecialLists) {
                int maxSpellLevel = __instance.MaxSpellLevel;
                ___m_CasterLevelInternal += settings.addCasterLevel;
                int maxSpellLevel2 = __instance.MaxSpellLevel;
                if (__instance.Blueprint.AllSpellsKnown) {
                    Traverse addSpecialMethod = Traverse.Create(__instance).Method("AddSpecial", new Type[] { typeof(int), typeof(BlueprintAbility) });
                    for (int i = maxSpellLevel + 1; i <= maxSpellLevel2; i++) {
                        foreach (BlueprintAbility spell in __instance.Blueprint.SpellList.GetSpells(i)) {
                            __instance.AddKnown(i, spell);
                        }
                        foreach (BlueprintSpellList specialList in ___m_SpecialLists) {
                            foreach (BlueprintAbility spell2 in specialList.GetSpells(i)) {
                                addSpecialMethod.GetValue(i, spell2);
                            }
                        }
                    }
                }
            }
        }
#endif
        [HarmonyPatch(typeof(SpellSelectionData), "CanSelectAnything", new Type[] { typeof(UnitDescriptor) })]
        public static class SpellSelectionData_CanSelectAnything_Patch {
            public static void Postfix(UnitDescriptor unit, bool __result) {
                if (!unit.IsPartyMemberOrPet()) return; // don't give extra feats to NPCs
                if (settings.toggleSkipSpellSelection) {
                    __result = false;
                }
            }
        }

        // Let user advance if no options left for feat selection
        [HarmonyPatch(typeof(CharGenFeatureSelectorPhaseVM), "CheckIsCompleted")]
        static class CharGenFeatureSelectorPhaseVM_CheckIsCompleted_Patch {
            private static void Postfix(CharGenFeatureSelectorPhaseVM __instance, ref bool __result) {
                if (settings.toggleOptionalFeatSelection) {
                    __result = true;
                }
                else if (settings.toggleNextWhenNoAvailableFeatSelections || settings.featsMultiplier != 1) {
                    var featureSelectorStateVM = __instance.FeatureSelectorStateVM;
                    var selectionState = featureSelectorStateVM.SelectionState;
                    var selectionVM = __instance.FeatureSelectorStateVM;
                    var state = Game.Instance.LevelUpController.State;
                    IFeatureSelection selection = (selection = (selectionVM.Feature as IFeatureSelection));
                    var availableItems = selection?.Items
                        .Where((IFeatureSelectionItem item) => selection.CanSelect(state.Unit, state, selectionState, item));
                    //modLogger.Log($"CharGenFeatureSelectorPhaseVM_CheckIsCompleted_Patch - availableCount: {availableItems.Count()}");
                    if (availableItems.Count() == 0)
                        __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(LevelUpHelper), "AddFeaturesFromProgression")]
        public static class MultiplyFeatPoints_LevelUpHelper_AddFeatures_Patch {
            public static bool Prefix(
                [NotNull] LevelUpState state,
                [NotNull] UnitDescriptor unit,
                [NotNull] IList<BlueprintFeatureBase> features,
                FeatureSource source,
                int level) {
                if (settings.featsMultiplier < 2) return true;
                //modLogger.Log($"name: {unit.CharacterName} isMemberOrPet:{unit.IsPartyMemberOrPet()}".cyan().bold());
                if (!unit.IsPartyMemberOrPet()) return true;
                modLogger.Log($"Log adding {settings.featsMultiplier}x features for {unit.CharacterName}");
                foreach (BlueprintFeature blueprintFeature in features.OfType<BlueprintFeature>()) {
                    for (int i = 0; i < settings.featsMultiplier; ++i) {
                        if (blueprintFeature.MeetsPrerequisites((FeatureSelectionState)null, unit, state, true)) {
                            if (blueprintFeature is IFeatureSelection selection && (!selection.IsSelectionProhibited(unit) || selection.IsObligatory()))
                                state.AddSelection((FeatureSelectionState)null, source, selection, level);
                            Kingmaker.UnitLogic.Feature feature = (Kingmaker.UnitLogic.Feature)unit.AddFact((BlueprintUnitFact)blueprintFeature);
                            if (blueprintFeature is BlueprintProgression progression)
                                LevelUpHelper.UpdateProgression(state, unit, progression);
                            FeatureSource source1 = source;
                            int level1 = level;
                            feature.SetSource(source1, level1);
                        }
                    }
                }
                return false;
            }
        }
    }
}
