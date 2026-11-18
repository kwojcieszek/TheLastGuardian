namespace PiterGame.EditorTools
{
    /// <summary>
    /// A 40-unit slice of a level with one obstacle pattern.
    /// </summary>
    public sealed class LayoutSection
    {
        public float X { get; set; }

        public float Width { get; set; }

        public string Kind { get; set; }

        public int Pattern { get; set; } = -1;

        public bool Safe { get; set; }

        public float End => X + Width;
    }
}
