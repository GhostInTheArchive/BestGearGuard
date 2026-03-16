using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;
using BestGearGuard.Utils;
using ProjectM.Network;

namespace BestGearGuard.Services
{
    public static class GearDebuffService
    {
        public static readonly PrefabGUID DebuffPrefab = new PrefabGUID(-1315531444); // SunDamageDebuff

        public static void ApplyDebuff(EntityManager em, Entity characterEntity)
        {
            try
            {
                if (characterEntity == Entity.Null || !em.Exists(characterEntity)) return;
                if (BuffUtility.TryGetBuff(em, characterEntity, DebuffPrefab, out _)) return;

                var userEntity = GetUserEntity(em, characterEntity);
                if (userEntity == Entity.Null) return;

                var debugEvents = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
                var applyEvent = new ApplyBuffDebugEvent { BuffPrefabGUID = DebuffPrefab };
                var fromChar = new FromCharacter { Character = characterEntity, User = userEntity };

                debugEvents.ApplyBuff(fromChar, applyEvent);

                if (BuffUtility.TryGetBuff(em, characterEntity, DebuffPrefab, out var buffEntity))
                {
                    // Reduce damage (default DamageFactorPerTick is 0.05, we lower it to 0.02)
                    if (em.HasComponent<SunDamageDebuff>(buffEntity))
                    {
                        var sunDebuff = em.GetComponentData<SunDamageDebuff>(buffEntity);
                        sunDebuff.DamageFactorPerTick = 0.01f;
                        em.SetComponentData(buffEntity, sunDebuff);
                    }

                    // Make permanent
                    if (em.HasComponent<LifeTime>(buffEntity))
                        em.RemoveComponent<LifeTime>(buffEntity);

                    if (em.HasComponent<RemoveBuffOnGameplayEvent>(buffEntity))
                        em.RemoveComponent<RemoveBuffOnGameplayEvent>(buffEntity);

                    if (em.HasComponent<RemoveBuffOnGameplayEventEntry>(buffEntity))
                        em.RemoveComponent<RemoveBuffOnGameplayEventEntry>(buffEntity);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError($"[GearDebuffService] ApplyDebuff error: {e.Message}");
            }
        }

        public static void RemoveDebuff(EntityManager em, Entity characterEntity)
        {
            try
            {
                if (characterEntity == Entity.Null || !em.Exists(characterEntity)) return;
                if (!BuffUtility.TryGetBuff(em, characterEntity, DebuffPrefab, out var buffEntity)) return;
                if (!em.Exists(buffEntity)) return;

                DestroyUtility.Destroy(em, buffEntity, DestroyDebugReason.None);
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError($"[GearDebuffService] RemoveDebuff error: {e.Message}");
            }
        }

        private static Entity GetUserEntity(EntityManager em, Entity characterEntity)
        {
            if (em.HasComponent<PlayerCharacter>(characterEntity))
                return em.GetComponentData<PlayerCharacter>(characterEntity).UserEntity;
            return Entity.Null;
        }
    }
}