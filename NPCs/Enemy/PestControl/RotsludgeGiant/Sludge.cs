using Microsoft.Xna.Framework;
using Terraria;

namespace Verdant.NPCs.Enemy.PestControl.RotsludgeGiant;

public class Sludge(Point frm)
{
    public const int Size = Rotsludge.Size;

    public bool Alive => life > 0;

    public int life = 1;
    public Point originalFrame = frm;
    public Point frame = frm;

    internal void Hit(int i, int j, Sludge[,] sludges)
    {
        const int Chance = 3;

        life--;

        Frame(i, j, sludges);

        if (i >= 1 && sludges[i - 1, j].Alive)
        {
            Frame(i - 1, j, sludges);

            if (Main.rand.NextBool(Chance))
                sludges[i - 1, j].Hit(i - 1, j, sludges);
        }

        if (i < Size - 1 && sludges[i + 1, j].Alive)
        {
            Frame(i + 1, j, sludges);

            if (Main.rand.NextBool(Chance))
                sludges[i + 1, j].Hit(i + 1, j, sludges);
        }

        if (j >= 1 && sludges[i, j - 1].Alive)
        {
            Frame(i, j - 1, sludges);

            if (Main.rand.NextBool(Chance))
                sludges[i, j - 1].Hit(i, j - 1, sludges);
        }

        if (j < Size - 1 && sludges[i, j + 1].Alive)
        {
            Frame(i, j + 1, sludges);

            if (Main.rand.NextBool(Chance))
                sludges[i, j + 1].Hit(i, j + 1, sludges);
        }
    }

    public void Frame(int i, int j, Sludge[,] sludges)
    {
        byte flags = GetFrameFlags(i, j, sludges);

        if (flags == 0b_1111) // <
            frame = new Point(18, 18);
        else if (flags == 0b_0011) // <>
            frame = new Point(54, 36);
        else if (flags == 0b_0111) // <^>
            frame = new Point(18, 36);
        else if (flags == 0b_0001) // <
            frame = new Point(72, 0);
        else if (flags == 0b_0101) // <^
            frame = new Point(36, 36);
        else if (flags == 0b_0010) // >
            frame = new Point(54, 18);
        else if (flags == 0b_0110) // ^>
            frame = new Point(0, 36);
        else if (flags == 0b_1110) // <^v
            frame = new Point(0, 18);
        else if (flags == 0b_0100) // ^
            frame = new Point(72, 18);
        else if (flags == 0b_1100) // ^v
            frame = new Point(72, 36);
        else if (flags == 0b_1011) // <v>
            frame = new Point(18, 0);
        else if (flags == 0b_1001) // <v
            frame = new Point(36, 0);
        else if (flags == 0b_1010) // v>
            frame = default;
        else if (flags == 0b_1101) // ^v>
            frame = new Point(36, 18);
        else if (flags == 0b_1000) // v
            frame = new Point(54, 0);
        else if (flags == 0b_0000) //
            frame = new Point(90, 0);
    }

    private static byte GetFrameFlags(int i, int j, Sludge[,] sludges)
    {
        byte flags = 0b_0000;

        if (i >= 1 && sludges[i - 1, j].Alive)
            flags |= 0b_0001; // Left

        if (i < Size - 1 && sludges[i + 1, j].Alive)
            flags |= 0b_0010; // Right

        if (j >= 1 && sludges[i, j - 1].Alive)
            flags |= 0b_0100; // Above

        if (j < Size - 1 && sludges[i, j + 1].Alive)
            flags |= 0b_1000; // Below

        return flags;
    }

    public override string ToString() => $"{life} : {frame}";
}
