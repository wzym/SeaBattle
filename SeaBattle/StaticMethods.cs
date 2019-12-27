using System;
using System.Collections.Generic;

namespace SeaBattle
{
    public static class StaticMethods
    {
        private static readonly Random Rnd = new Random();
        
        internal static T GetRandomElement<T>(IReadOnlyList<T> sequence)
            => sequence[Rnd.Next(sequence.Count)];
    }
}