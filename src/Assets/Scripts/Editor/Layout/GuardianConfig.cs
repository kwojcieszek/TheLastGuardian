namespace PiterGame.EditorTools
{
    /// <summary>
    /// Boss or miniboss that guards the end of a level.
    /// </summary>
    public sealed class GuardianConfig
    {
        public string Actor { get; set; }

        public string DisplayName { get; set; }

        public int MaxHealth { get; set; }

        public float Scale { get; set; } = 1f;
    }
}
