using System.Collections.Generic;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Complete geometry and population plan of one level.
    /// </summary>
    public sealed class LevelLayout
    {
        public int Id { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public string Biome { get; set; }

        public string Scene { get; set; }

        public float Length { get; set; }

        public List<LayoutSection> Sections { get; } = new List<LayoutSection>();

        public List<LayoutSurface> Surfaces { get; } = new List<LayoutSurface>();

        public List<LayoutHazard> Hazards { get; } = new List<LayoutHazard>();

        public List<LayoutEnemy> Enemies { get; } = new List<LayoutEnemy>();

        public List<LayoutPickup> Pickups { get; } = new List<LayoutPickup>();

        public List<LayoutNpc> Npcs { get; } = new List<LayoutNpc>();

        public List<LayoutDecoration> Decorations { get; } = new List<LayoutDecoration>();

        public float[] Checkpoints { get; set; }

        public LevelObjectiveConfig Objective { get; set; }

        public GuardianConfig Guardian { get; set; }

        public int CountPickups(PickupKind kind)
        {
            return Pickups.FindAll(pickup => pickup.Kind == kind).Count;
        }
    }
}
