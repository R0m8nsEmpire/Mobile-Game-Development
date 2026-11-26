using System.IO;
using UnityEngine;
using CMP5402.Week9;

// Lab 9: holds the player's progress for the tracker and saves it next to the queue.
// Medium and Hard: replace this with a ProgressRecord field inside your Week 7 SaveData.
public class ProgressStore : MonoBehaviour
{
    public static ProgressStore Instance { get; private set; }
    public ProgressRecord Progress { get; private set; } = new ProgressRecord();
    public AchievementTracker Tracker { get; private set; }
    string path;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        path = Path.Combine(Application.persistentDataPath, "progress.json");
        if (File.Exists(path))
        {
            try { Progress = JsonUtility.FromJson<ProgressRecord>(File.ReadAllText(path)) ?? new ProgressRecord(); }
            catch { Progress = new ProgressRecord(); }
        }
    }

    void Start()
    {
        Tracker = new AchievementTracker(PlatformHub.Instance, Progress, Save);
        Tracker.Unlocked += id => Debug.Log("Achievement unlocked locally: " + id);
    }

    public void Save()
    {
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonUtility.ToJson(Progress, true));
        if (File.Exists(path)) File.Replace(tmp, path, path + ".bak");
        else File.Move(tmp, path);
    }
}