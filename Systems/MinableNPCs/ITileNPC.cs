using Microsoft.Xna.Framework;

namespace Verdant.Systems.MinableNPCs;

internal interface ITileNPC
{
    /// <summary>
    /// Whether the tile NPC can be mined or not.
    /// </summary>
    /// <param name="mousePosition">Position of the mouse.</param>
    /// <returns>Whether the NPC can be mined or not.</returns>
    public bool CanMineNPC(Point mousePosition);

    /// <summary>
    /// Runs when the tile NPC can be mined, as per <see cref="CanMineNPC(Point)"/>.
    /// </summary>
    /// <param name="mousePosition">Position of the mouse.</param>
    public void MineNPC(Point mousePosition);
}
