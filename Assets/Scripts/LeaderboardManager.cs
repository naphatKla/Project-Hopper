using System;
using Cysharp.Threading.Tasks;
using Dan.Main;
using Dan.Models;
using UnityEngine;

public static class LeaderboardManager
{
    public static string OwnerLocalName { get; private set; }
    public static int OwnerLocalHighestScore { get; private set; }
    
    public static async UniTask<Entry[]> LoadAllDataFromServer()
    {
        bool isDataLoaded = false;
        Entry[] entries = new Entry[] { };
        
        Leaderboards.ProjectHopper.GetEntries(loadedEntries =>
        {
            entries = loadedEntries;
            isDataLoaded = true;
        });
        await UniTask.WaitUntil(() => isDataLoaded).TimeoutWithoutException(TimeSpan.FromSeconds(5f));
        return entries;
    }

    public static async UniTask<Entry> LoadOwnerDataFromServer()
    {
        bool isDataLoaded = false;
        Entry entry = new Entry();
        
        Leaderboards.ProjectHopper.GetPersonalEntry(loadedEntry =>
        {
            entry = loadedEntry;
            isDataLoaded = true;
        });
        await UniTask.WaitUntil(() => isDataLoaded).TimeoutWithoutException(TimeSpan.FromSeconds(5f));
        return entry;
    }

    public static void UploadDataToServer(string name, int highestScore)
    {
        Leaderboards.ProjectHopper.UploadNewEntry(name, highestScore);
    }
    
    public static void UploadDataToServer(int highestScore)
    {
        Leaderboards.ProjectHopper.UploadNewEntry(OwnerLocalName, highestScore);
    }

    public static void SetLocalData(string name, int highestScore)
    {
        OwnerLocalName = name;
        OwnerLocalHighestScore = highestScore;
    }
    
    public static void SetLocalData(int highestScore)
    {
        OwnerLocalHighestScore = highestScore;
    }
}
