using System.IO;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    public class SaveManager : MonoBehaviour
    {
        private const int SaveSlotCount = 3;
        private const string SaveFileNameFormat = "save_slot_{0}.json";

        public SaveData[] LoadAllSlots()
        {
            var saves = new SaveData[SaveSlotCount];
            for (int i = 0; i < SaveSlotCount; i++)
            {
                saves[i] = LoadSlot(i + 1);
            }
            return saves;
        }

        public SaveData LoadSlot(int slotId)
        {
            var path = GetSavePath(slotId);
            if (!File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public void SaveSlot(int slotId, SaveData saveData)
        {
            saveData.slotId = slotId;
            var path = GetSavePath(slotId);
            var json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(path, json);
        }

        public void DeleteSlot(int slotId)
        {
            var path = GetSavePath(slotId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private string GetSavePath(int slotId)
        {
            return Path.Combine(Application.persistentDataPath, string.Format(SaveFileNameFormat, slotId));
        }
    }
}
