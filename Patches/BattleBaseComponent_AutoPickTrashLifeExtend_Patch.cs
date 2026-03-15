using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 保留原版 AutoPickTrash 逻辑，仅在“部分拾取（捡到一部分且仍有剩余）”时给该堆延寿。
    /// 延寿策略：每次 +30s，但总剩余寿命最多 60s，避免多次拾取导致寿命无限叠加。
    /// </summary>
    [HarmonyPatch(typeof(BattleBaseComponent), "AutoPickTrash")]
    public static class BattleBaseComponent_AutoPickTrashLifeExtend_Patch
    {
        private const int START_SEED_MULTIPLIER = 4;
        private const int MAX_ATTEMPTS_PER_CALL = 4;
        private const int LIFE_EXTEND_FRAMES = 30 * 60; // 每次延长 30s
        private const int LIFE_MAX_FRAMES = 60 * 60;    // 剩余寿命上限 60s

        public struct WatchState
        {
            public List<WatchEntry> Entries;
        }

        public struct WatchEntry
        {
            public int Index;
            public int ItemId;
            public int OldCount;
        }

        [HarmonyPrefix]
        public static void Prefix(BattleBaseComponent __instance, PlanetFactory factory, TrashSystem trashSystem, long time, ref WatchState __state)
        {
            __state = default;
            try
            {
                if (__instance == null || factory == null || trashSystem?.container == null)
                    return;

                ref EntityData ptr = ref factory.entityPool[__instance.entityId];
                if (ptr.id != __instance.entityId)
                    return;

                TrashContainer container = trashSystem.container;
                int trashCursor = container.trashCursor;
                if (trashCursor <= 0)
                    return;

                TrashObject[] trashObjPool = container.trashObjPool;
                TrashData[] trashDataPool = container.trashDataPool;
                int astroId = factory.planet.astroId;

                Vector3 pos = ptr.pos;
                float x = pos.x;
                float y = pos.y;
                float z = pos.z;
                float rangeSq = __instance.pickRange * __instance.pickRange;

                int start = (int)((time * START_SEED_MULTIPLIER + __instance.id) % 1000000000L);
                var entries = new List<WatchEntry>(MAX_ATTEMPTS_PER_CALL);

                for (int i = start; i < start + trashCursor; i++)
                {
                    int idx = i % trashCursor;
                    if (trashObjPool[idx].item <= 0 || trashObjPool[idx].expire >= 0 || trashDataPool[idx].nearPlanetId != astroId)
                        continue;

                    float dx = trashDataPool[idx].lPos.x - x;
                    float dxSq = dx * dx;
                    if (dxSq >= rangeSq) continue;
                    float dy = trashDataPool[idx].lPos.y - y;
                    float dySq = dy * dy;
                    if (dySq >= rangeSq) continue;
                    float dz = trashDataPool[idx].lPos.z - z;
                    float dzSq = dz * dz;
                    if (dzSq >= rangeSq || dxSq + dySq + dzSq >= rangeSq) continue;

                    entries.Add(new WatchEntry
                    {
                        Index = idx,
                        ItemId = trashObjPool[idx].item,
                        OldCount = trashObjPool[idx].count
                    });

                    if (entries.Count >= MAX_ATTEMPTS_PER_CALL)
                        break;
                }

                __state = new WatchState { Entries = entries };
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] AutoPickTrashLifeExtend Prefix 异常: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(TrashSystem trashSystem, WatchState __state)
        {
            try
            {
                if (trashSystem?.container == null || __state.Entries == null || __state.Entries.Count == 0)
                    return;

                TrashContainer container = trashSystem.container;
                TrashObject[] trashObjPool = container.trashObjPool;
                TrashData[] trashDataPool = container.trashDataPool;
                int trashCursor = container.trashCursor;

                foreach (var entry in __state.Entries)
                {
                    int idx = entry.Index;
                    if (idx < 0 || idx >= trashCursor)
                        continue;

                    int newItem = trashObjPool[idx].item;
                    int newCount = trashObjPool[idx].count;
                    if (newItem != entry.ItemId)
                        continue; // 槽位被复用或已被删除

                    // 部分拾取：本次数量减少，但该堆仍存在
                    bool partiallyPicked = newCount > 0 && newCount < entry.OldCount;
                    if (!partiallyPicked)
                        continue;

                    int oldLife = trashDataPool[idx].life;
                    if (oldLife > 0 && oldLife < LIFE_MAX_FRAMES)
                    {
                        int extended = oldLife + LIFE_EXTEND_FRAMES;
                        trashDataPool[idx].life = (extended > LIFE_MAX_FRAMES) ? LIFE_MAX_FRAMES : extended;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] AutoPickTrashLifeExtend Postfix 异常: {ex.Message}");
            }
        }
    }
}
