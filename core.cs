using ProjectM;
using ProjectM.Scripting;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace BestGearGuard
{
    public static class Core
    {
        public static World Server { get; } = GetWorld("Server") ?? throw new Exception("Server World not found. Did you install the mod on the server?");
        public static EntityManager EntityManager => Server.EntityManager;
        public static ServerGameManager ServerGameManager => Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
        public static PrefabCollectionSystem PrefabCollection { get; } = Server.GetExistingSystemManaged<PrefabCollectionSystem>();

        public static bool hasInitialized = false;

        private static World GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                    return world;
            }
            return null;
        }

        public static void LogException(Exception e, [CallerMemberName] string caller = null)
        {
            Debug.LogError($"[BestGearGuard] Exception in {caller}: {e.Message}\n{e.StackTrace}");
        }
    }
}
