﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle
{
    public static class CommonParameters
    {
        internal static readonly string[] ShipNames = 
        {
            "Стремительный", "Коварный", "Летящий", "Упорный", "Леденящий", "Двенадцатый", "Контролирующий",
            "Подводный", "Невероятный", "Чёрный", "Гладкий", "Светящийся", "Разящий", "Бездомный", "Нескончаемый",
            "Бесшовный", "Крылатый", "Пронзительный", "Центральный", "Говорящий", "Стрела", "Обходящий",
            "Прохладный", "Северный", "Южный подводник", "Уборщик", "Чистильщик",
            "Стреляющий без поражения в каждой третьей войне", "Рождённый в январе"
        };

        internal static readonly string[] Sss = new HashSet<string>
        {
            "Стремительный", "Коварный", "Летящий", "Упорный", "Леденящий", "Двенадцатый", "Контролирующий",
            "Подводный", "Невероятный", "Чёрный", "Гладкий", "Светящийся", "Разящий", "Бездомный", "Нескончаемый",
            "Бесшовный", "Крылатый", "Пронзительный", "Центральный", "Говорящий", "Стрела", "Обходящий",
            "Прохладный", "Северный", "Южный подводник", "Уборщик", "Чистильщик",
            "Стреляющий без поражения в каждой третьей войне", "Аделактикат", "Аргумент Х", "Логическое убеждение",
            "Агидайн", "Рождённый в январе"
        }.ToArray();

        internal static readonly Dictionary<int, int> NewFleetParams = new Dictionary<int, int>
        {
            {4, 1}, {3, 2}, {2, 3}, {1, 4}
        };

        public const int TimeLapse = 750;
    }
}