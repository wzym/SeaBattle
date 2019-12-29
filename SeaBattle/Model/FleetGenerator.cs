using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class FleetGenerator
    {
        private static readonly List<string> ShipNames = new List<string>
        {
            "Стремительный", "Коварный", "Летящий", "Упорный", "Леденящий", "Двенадцатый", "Контролирующий",
            "Подводный", "Невероятный", "Чёрный", "Гладкий", "Светящийся", "Разящий", "Бездомный", "Нескончаемый",
            "Бесшовный", "Крылатый", "Пронзительный", "Центральный", "Говорящий", "Стрела", "Обходящий",
            "Прохладный", "Северный", "Вздорный ахтунг", "Южный подводник", "Уборщик", "Чистильщик",
            "Стреляющий без поражения в каждой третьей войне"
        };
        
        private readonly Field field;

        public FleetGenerator(Field field)
        {
            this.field = field;
        }

        internal Ship GetShip(int decksAmount)
        {
            var variants = FormVariants(decksAmount);
            var variant = StaticMethods.GetRandomElement(variants);
            var ship = new Ship(variant.HeadPosition
                , variant.Size
                , variant.IsHorizontal
                , ShipNames.PullElement());
            return ship;
        }

        private List<Ship> FormVariants(int decksAmount)
        {
            var result = new List<Ship>();
            for(var y = 0; y < field.Height; y++)
                for(var x = 0; x <= field.Width - decksAmount; x++)
                {
                    var variant = new Ship(new Point(x, y), decksAmount);
                    if (IsGood(variant)) result.Add(variant);                    
                }
            for(var y = 0; y < field.Height - decksAmount; y++)
                for(var x = 0; x < field.Width; x++)
                {
                    var variant = new Ship(new Point(x, y), decksAmount, false);
                    if (IsGood(variant)) result.Add(variant);
                }
            return result;
        }

        private bool IsGood(Ship variant)
            => variant
                .PreliminaryBody()
                .All(cell => field[cell].Type == CellType.Sea);
    }
}