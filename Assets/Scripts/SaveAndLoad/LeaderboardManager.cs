using Dan.Main;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SaveAndLoad
{
    public class LeaderboardManager : MonoBehaviour
    {
        [Button]
        public void LoadEntries()
        {
            Leaderboards.ProjectHopper.GetEntries(entries =>
            {
                Debug.Log(entries.Length);
                foreach (var entry in entries)
                {
                    Debug.Log(entry.Username + entry.Score);
                }
            });
            
            Leaderboards.ProjectHopper.UploadNewEntry("asdas", 1000);
        }
    }
}
