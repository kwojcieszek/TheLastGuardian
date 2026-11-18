namespace PiterGame
{
    /// <summary>
    /// Describes how the next level scene should start.
    /// </summary>
    public sealed class LevelLaunchRequest
    {
        public int LevelId { get; }

        public bool ResumeFromCheckpoint { get; }

        public LevelLaunchRequest(int levelId, bool resumeFromCheckpoint)
        {
            LevelId = levelId;
            ResumeFromCheckpoint = resumeFromCheckpoint;
        }
    }
}
