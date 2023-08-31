using System;

namespace CodeBase.Data.Stats
{
    [Serializable]
    public class LevelStats
    {
        private const int TargetScore = 100;
        private const int AddingForRestarts = 1;

        public Scene Scene;
        public MoneyData MoneyData;
        public PlayTimeData PlayTimeData;
        public KillsData KillsData;
        public RestartsData RestartsData;
        public int StarsCount;
        public int Score;

        public LevelStats(Scene scene, int targetPlayTime, int totalEnemies)
        {
            Scene = scene;
            MoneyData = new MoneyData();
            PlayTimeData = new PlayTimeData(targetPlayTime);
            KillsData = new KillsData(totalEnemies);
            RestartsData = new RestartsData();
            StarsCount = (int)Constants.Zero;
            Score = (int)Constants.Zero;
        }

        public void CalculateScore()
        {
            CalculatePlayTime();
            CalculateKills();
            CalculateRestarts();
        }

        private void CalculatePlayTime()
        {
            if (PlayTimeData.IsPlayTimeLessTarget())
                StarsCount++;

            Score += (int)(TargetScore * PlayTimeData.Ratio);
        }

        private void CalculateKills()
        {
            if (KillsData.IsTotalKilled())
                StarsCount++;

            Score += (int)Math.Floor(TargetScore * KillsData.Ratio);
        }

        private void CalculateRestarts()
        {
            if (RestartsData.Count == Constants.Zero)
                StarsCount++;

            Score += TargetScore / (RestartsData.Count + AddingForRestarts);
        }
    }
}