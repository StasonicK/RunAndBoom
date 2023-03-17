using System;

namespace CodeBase.Data
{
    [Serializable]
    public class LevelStats
    {
        public string Name { get; private set; }

        public MoneyData MoneyData { get; private set; }

        public LevelStats(string name)
        {
            Name = name;
            MoneyData = new MoneyData();
        }
    }
}