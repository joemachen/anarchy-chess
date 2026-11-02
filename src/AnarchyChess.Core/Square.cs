namespace AnarchyChess.Core;

/// <summary>A board square. File 0 is "a", rank 0 is "1" (the player's back rank).</summary>
public readonly record struct Square(int File, int Rank)
{
    public string Name => $"{(char)('a' + File)}{Rank + 1}";

    public static Square Parse(string name) => new(name[0] - 'a', int.Parse(name[1..]) - 1);

    public Square Offset(int files, int ranks) => new(File + files, Rank + ranks);

    public override string ToString() => Name;
}
