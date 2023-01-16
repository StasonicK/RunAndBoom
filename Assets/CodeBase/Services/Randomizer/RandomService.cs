﻿using UnityEngine;

namespace CodeBase.Services.Randomizer
{
    public class RandomService : IRandomService
    {
        public int Next(int min, int max) =>
            Random.Range(min, max);
    }
}