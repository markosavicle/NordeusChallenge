using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    [System.Serializable]
    public class SaveData
    {
        public int slotId;
        public string heroName;
        public PlayerData player;
        public List<MoveData> learnedMoves = new List<MoveData>();

        public static SaveData Create(int slotId, string heroName, PlayerData player)
        {
            return new SaveData
            {
                slotId = slotId,
                heroName = heroName,
                player = player,
                learnedMoves = new List<MoveData>(player.learnedMoves)
            };
        }
    }
}
