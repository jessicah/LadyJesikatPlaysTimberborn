using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.Common;
using Timberborn.GameFactionSystem;
using Timberborn.GameGoods;
using Timberborn.GoodCollectionSystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.ModManagerScene;
using Timberborn.NeedCollectionSystem;
using Timberborn.NeedSpecs;
using Timberborn.RecoverableGoodSystemUI;
using Timberborn.Workshops;
using UnityEngine;

namespace ExtraDebugLogging;

public class ExtraDebugLoggingModStarter : IModStarter
{
    public void StartMod(IModEnvironment environment) => new Harmony(nameof(Logging)).PatchAll();
}

public static class Logging
{
    [HarmonyPatch(typeof(RecipeGoodsProcessor), nameof(RecipeGoodsProcessor.ProcessRecipes)), HarmonyPrefix]
    public static bool ProcessRecipesPrefix(ref Dictionary<StorableGood, int> __result, RecipeGoodsProcessor __instance, IEnumerable<string> recipeIds, int capacityMultiplier = 1)
    {
        Dictionary<StorableGood, int> dictionary = new();

        foreach (string text in recipeIds)
        {
            try
            {
                RecipeSpec recipe = __instance._recipeSpecService.GetRecipe(text);

                if ((recipe.Blueprint.IsAllowedByFeatureToggles()))
                {
                    __instance.ProcessRecipe(recipe, dictionary, capacityMultiplier);
                }
            }
            catch (Exception)
            {
                Debug.LogWarning($"Unable to find matching recipe for '{text}'");
                Debug.LogWarning("List of available recipes:");
                foreach (string recipeText in __instance._recipeSpecService._recipeSpecs.Keys)
                {
                    Debug.LogWarning($" - {recipeText}");
                }

                throw;
            }
        }

        __result = dictionary;

        return false;
    }

    [HarmonyPatch(typeof(GameGoodFilter), nameof(GameGoodFilter.Load)), HarmonyPrefix]
    public static bool GameGoodFilterLoadPrefix(GameGoodFilter __instance)
    {
        Debug.LogWarning("Loading Game Goods...");
        HashSet<string> hashSet = __instance._goodCollectionIdsProviders.SelectMany<IGoodCollectionIdsProvider, string>((IGoodCollectionIdsProvider provider) => provider.GetGoodCollectionIds()).ToHashSet<string>();
        foreach (GoodCollectionSpec goodCollectionSpec in __instance._specService.GetSpecs<GoodCollectionSpec>())
        {
            if (hashSet.Contains(goodCollectionSpec.CollectionId))
            {
                __instance._allowedGoods.AddRange<string>(goodCollectionSpec.Goods);

                Debug.LogWarning($"Adding {goodCollectionSpec.Goods.Length} goods for {goodCollectionSpec.CollectionId}");
                foreach (string key in goodCollectionSpec.Goods)
                {
                    Debug.LogWarning($" - {key}");
                }
            }
        }
        List<GoodSpec> list = __instance._specService.GetSpecs<GoodSpec>().ToList<GoodSpec>();
        using (HashSet<string>.Enumerator enumerator2 = __instance._allowedGoods.GetEnumerator())
        {
            while (enumerator2.MoveNext())
            {
                string goodId = enumerator2.Current;
                if (list.All<GoodSpec>((GoodSpec good) => good.Id != goodId))
                {
                    throw new ArgumentException("There is no GoodSpec with id: " + goodId);
                }
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(FactionNeedService), nameof(FactionNeedService.Load)), HarmonyPrefix]
    public static bool FactionNeedServiceLoadPrefix(FactionNeedService __instance)
    {
        Debug.LogWarning("Loading Faction Needs...");
        HashSet<string> hashSet = __instance._needCollectionIdsProviders.SelectMany<INeedCollectionIdsProvider, string>((INeedCollectionIdsProvider provider) => provider.GetNeedCollectionIds()).ToHashSet<string>();
        HashSet<string> hashSet2 = new HashSet<string>();
        foreach (NeedCollectionSpec needCollectionSpec in __instance._specService.GetSpecs<NeedCollectionSpec>())
        {
            if (hashSet.Contains(needCollectionSpec.CollectionId))
            {
                hashSet2.AddRange<string>(needCollectionSpec.Needs);

                Debug.LogWarning($"Adding {needCollectionSpec.Needs.Length} needs for {needCollectionSpec.CollectionId}");
                foreach (string key in needCollectionSpec.Needs)
                {
                    Debug.LogWarning($" - {key}");
                }
            }
        }

        foreach (NeedSpec needSpec2 in from needSpec in __instance._specService.GetSpecs<NeedSpec>()
                                       orderby needSpec.Order
                                       select needSpec)
        {
            if (hashSet2.Contains(needSpec2.Id) && needSpec2.Blueprint.IsAllowedByFeatureToggles())
            {
                __instance._needs.Add(__instance._needModificationService.ModifyIfEligible(needSpec2));
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(FactionNeedService), nameof(FactionNeedService.GetBeaverOrBotNeedById)), HarmonyPrefix]
    public static bool GetBeaverOfBotNeedByIdPrefix(FactionNeedService __instance, ref NeedSpec __result, string id)
    {
        var beaversNeeds = __instance.GetBeaverNeeds().Where<NeedSpec>((NeedSpec need) => need.Id == id);
        if (beaversNeeds.Count() == 1)
        {
            __result = beaversNeeds.First();
            return false;
        }

        var botNeeds = __instance.GetBotNeeds().Where<NeedSpec>((NeedSpec need) => need.Id == id);
        if (botNeeds.Count() == 1)
        {
            __result = botNeeds.First();
            return false;
        }

        if (beaversNeeds.Count() == 0 && botNeeds.Count() == 0)
        {
            throw new InvalidOperationException($"Need with id {id} not found");
        }
        else if (beaversNeeds.Count() > 1)
        {
            throw new InvalidOperationException($"Need with id {id} has multiple beaver needs");
        }
        else if (botNeeds.Count() > 1)
        {
            throw new InvalidOperationException($"Need with id {id} has multiple bot needs");
        }

        throw new InvalidOperationException("Need with id " + id + " not found or multiple needs found");
    }

    [HarmonyPatch(typeof(RecoverableGoodItemFactory), nameof(RecoverableGoodItemFactory.Create)), HarmonyPrefix]
    public static bool RecoverableGoodItemFactoryCreatePrefix(RecoverableGoodItemFactory __instance, ref RecoverableGoodItem __result, string goodId)
    {
        Debug.LogWarning($"RecoverableGoodItemFactory::Create({goodId})");

        return true;
    }

    [HarmonyPatch(typeof(ManufactoryInventoryInitializer), nameof(ManufactoryInventoryInitializer.Initialize)), HarmonyPrefix]
    public static bool ManufactoryInventoryInitializerInitialize(ManufactoryInventoryInitializer __instance, Manufactory subject, Inventory decorator)
    {
        Debug.LogWarning($"ManufactoryInventoryInitializer::Initialize({subject?.Name ?? "<unknown subject>"}, {decorator?.Name ?? "<unknown decorator>"})");

        if (subject is null)
            throw new InvalidOperationException($"subject is null");

        Debug.LogWarning("=> subject.GetComponent<ManufactorySpec>");
        ManufactorySpec component = subject.GetComponent<ManufactorySpec>();

        if (component is null)
            throw new InvalidOperationException("manufactory spec not found");

        Debug.LogWarning($"=> _recipeGoodsProcessor.ProcessRecipes({String.Join(", ", component.ProductionRecipeIds)}, 1)");

        Dictionary<StorableGood, int> dictionary = __instance._recipeGoodsProcessor.ProcessRecipes(component.ProductionRecipeIds, 1);

        if (dictionary is null)
            throw new InvalidOperationException("failed to process recipes");

        Debug.LogWarning($"=> _inventoryInitializerFactory.Create({decorator.Name}, {dictionary.Values.Sum()}, {ManufactoryInventoryInitializer.InventoryComponentName})");
        InventoryInitializer inventoryInitializer = __instance._inventoryInitializerFactory.Create(decorator, dictionary.Values.Sum(), ManufactoryInventoryInitializer.InventoryComponentName);

        if (inventoryInitializer is null)
            throw new InvalidOperationException("failed to create inventory initializer");

        Debug.LogWarning($"=> inventoryInitializer.AddAllowedGoods({String.Join(",", dictionary.Keys)})");
        inventoryInitializer.AddAllowedGoods(dictionary.Select<KeyValuePair<StorableGood, int>, StorableGoodAmount>((KeyValuePair<StorableGood, int> pair) => new StorableGoodAmount(pair.Key, pair.Value)));

        RecipeGoodDisallower component2 = subject.GetComponent<RecipeGoodDisallower>();
        if (component2 is null)
            throw new InvalidOperationException("failed to get recipe good disallower");
        Debug.LogWarning("=> inventoryInitializer.AddGoodDisallower(disallower)");
        inventoryInitializer.AddGoodDisallower(component2);
        Debug.LogWarning("=> inventoryInitializer.HasPublicOutput()");
        inventoryInitializer.HasPublicOutput();
        Debug.LogWarning("=> inventoryInitializer.Initialize()");
        inventoryInitializer.Initialize();
        Debug.LogWarning("=> subject.InitializeInventory(decorator)");
        subject.InitializeInventory(decorator);
        Debug.LogWarning("=> this._inventoryNeedBehaviourInitializer.AddNeedBehavior(decorator)");
        __instance._inventoryNeedBehaviorInitializer.AddNeedBehavior(decorator);

        return false;
    }
#if false
[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.BlockConflictsWithBlockAbove)), HarmonyPostfix]
public static void BlockConflictsWithBlockAbovePostfix(BlockValidator __instance, ref bool __result, Block block)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::BlockConflictsWithBlockAbove(): {block.Coordinates}");
    }
}

[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.BlockConflictsWithBlocksBelow)), HarmonyPostfix]
public static void BlockConflictsWithBlocksBelowPostfix(BlockValidator __instance, ref bool __result, Block block)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::BlockConflictsWithBlocksBelow(): {block.Coordinates}");
    }
}

[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.BlockConflictsWithExistingObject)), HarmonyPostfix]
public static void BlockConflictsWithExistingObjectPostfix(BlockValidator __instance, ref bool __result, Block block)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::BlockConflictsWithExistingObject(): {block.Coordinates}");
    }
}

[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.BlockConflictsWithMatterBelow)), HarmonyPostfix]
public static void BlockConflictsWithMatterBelowPostfix(BlockValidator __instance, ref bool __result, Block block, bool ignoreUnfinishedStackable)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::BlockConflictsWithMatterBelow(): {block.Coordinates}: matter below = {block.MatterBelow}");
    }
}

[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.BlockConflictsWithTerrain)), HarmonyPostfix]
public static void BlockConflictsWithTerrainPostfix(BlockValidator __instance, ref bool __result, Block block)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::BlockConflictsWithTerrain(): {block.Coordinates}");
    }
}

[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.ConflictsWithUndergroundBlockObject)), HarmonyPostfix]
public static void ConflictsWithUndergroundBlockObjectPostfix(BlockValidator __instance, ref bool __result, Block block)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::ConflictsWithUndergroundBlockObject(): {block.Coordinates}");
    }
}

[HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.UndergroundBlockIsNotUnderground)), HarmonyPostfix]
public static void UndergroundBlockIsNotUndergroundPostfix(BlockValidator __instance, ref bool __result, Block block)
{
    if (__result is true)
    {
        Debug.LogWarning($"BlockValidator::UndergroundBlockIsNotUnderground(): {block.Coordinates}");
    }
}

[HarmonyPatch(typeof(StackableBlockService), nameof(StackableBlockService.IsStackableBlockAt)), HarmonyPostfix]
public static void IsStackableBlockAtPostfix(StackableBlockService __instance, ref bool __result, Vector3Int coords, bool finishedOnly)
{
    if (__result is false)
    {
        ReadOnlyList<BlockObject> objectsAt = __instance._blockService.GetObjectsAt(coords);
        Debug.LogWarning($"StackableBlockService::IsStackableBlockAt(): {coords}: finishedOnly = {finishedOnly}, objectsAt.Count = {objectsAt.Count}");
    }
}

[HarmonyPatch(typeof(StackableBlockService), nameof(StackableBlockService.IsFinishedStackableBlockAt)), HarmonyPostfix]
public static void IsFinishedStackableBlockAtPostfix(StackableBlockService __instance, ref bool __result, Vector3Int coords)
{
    if (__result is false)
    {
        ReadOnlyList<BlockObject> objectsAt = __instance._blockService.GetObjectsAt(coords);
        Debug.LogWarning($"StackableBlockService::IsFinishedStackableBlockAt(): {coords}: objectsAt.Count = {objectsAt.Count}");
    }
}

[HarmonyPatch(typeof(MatterBelowValidator), nameof(MatterBelowValidator.Validate), new Type[] { typeof(Block), typeof(bool) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Normal }), HarmonyPrefix]
public static bool MatterBelowValidatorValidatePrefix(MatterBelowValidator __instance, ref bool __result, in Block block, bool ignoreUnfinishedStackable)
{
    if (block.Coordinates.m_X == 75 && block.Coordinates.m_Y == 249 && block.Coordinates.m_Z == 27)
    {
        MatterBelow matterBelow = block.MatterBelow;
        Vector3Int coordinates = block.Coordinates + new Vector3Int(0, 0, 1);
        Debug.LogWarning($"MatterBelowValidator::Validate(): {coordinates}: matter below = {matterBelow}, ignoreUnfinishedStackable = {ignoreUnfinishedStackable}");
        bool flag;
        switch (matterBelow)
        {
            case MatterBelow.Ground:
                flag = __instance.AtGroundLevel(coordinates) || (!ignoreUnfinishedStackable && __instance.UnfinishedGroundBelow(coordinates)) || (block.Underground && __instance._terrainService.Underground(coordinates.Below()));
                Debug.LogWarning($"MatterBelowValidator::Validate(): {coordinates}: AtGroundLevel = {__instance.AtGroundLevel(coordinates)}, UnfinishedGroundBelow = {!ignoreUnfinishedStackable && __instance.UnfinishedGroundBelow(coordinates)}, Underground = {(block.Underground && __instance._terrainService.Underground(coordinates.Below()))}");
                break;
            case MatterBelow.GroundOrStackable:
                flag = __instance.AtGroundLevel(coordinates) || (block.Underground && __instance._terrainService.Underground(coordinates.Below())) || __instance.StackableBelow(coordinates, ignoreUnfinishedStackable);
                Debug.LogWarning($"MatterBelowValidator::Validate(): {coordinates}: AtGroundLevel = {__instance.AtGroundLevel(coordinates)}, Underground = {(block.Underground && __instance._terrainService.Underground(coordinates.Below()))}, StackableBelow = {__instance.StackableBelow(coordinates, ignoreUnfinishedStackable)}");
                break;
            case MatterBelow.Air:
                flag = __instance.AboveGround(coordinates) && !__instance.TopBlockBelow(coordinates);
                Debug.LogWarning($"MatterBelowValidator::Validate(): {coordinates}: AboveGround = {__instance.AboveGround(coordinates)}, TopBlockBelow = {__instance.TopBlockBelow(coordinates)}");
                break;
            case MatterBelow.Any:
                flag = true;
                Debug.LogWarning($"MatterBelowValidator::Validate(): {coordinates}: Any matter below, always true");
                break;
            case MatterBelow.Stackable:
                flag = __instance.StackableBelow(coordinates, ignoreUnfinishedStackable);
                Debug.LogWarning($"MatterBelowValidator::Validate(): {coordinates}: StackableBelow = {__instance.StackableBelow(coordinates, ignoreUnfinishedStackable)}");
                break;
            default:
                throw new NotSupportedException(matterBelow.ToString());
        }
    }

    return true;
}
#endif
}