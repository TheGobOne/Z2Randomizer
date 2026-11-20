using System;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Archipelago mode: Z2R draws every AP item with the Link Doll's tiles, so this replaces the Link Doll graphic in the
/// CHR with the left half of the Archipelago logo (the logo's pixels, shrunk to 16x16, as 8x16 sprite tiles), and
/// writes the right half once into the item's second sprite slot. Only the Link Doll's tiles and that blank slot
/// change, and every Link Doll in the game is an AP item in this mode, so nothing else looks different.
/// </summary>
public static class ArchipelagoLogo
{
    /// <summary>
    /// The Link Doll's top tile (16 bytes) as the CHR has it. The top tile is the same in every Link Doll found so far;
    /// the bottom tile varies between ROM builds, so the search uses the top tile and the logo replaces both.
    /// </summary>
    public static readonly byte[] LinkDoll = Convert.FromHexString("001c3e7f2255227f00001c225d2a1c22");

    /// <summary>The left half of the AP logo, as the same 32 bytes (top tile, then bottom tile).</summary>
    public static readonly byte[] Logo = Convert.FromHexString("0002007dfdffc6380307037dfcfec60054b4ab27570707032848535727070703");

    /// <summary>
    /// The right half of the AP logo (32 bytes, top then bottom). It goes at CHR 0x1A060, the tile $c7 slot that
    /// Hyrule writes from item_sprites.chr. That slot is in the fixed $1C00 window, so it is the same in every room.
    /// The game draws an item's second sprite flipped horizontally, so these tiles are stored mirrored.
    /// </summary>
    public static readonly byte[] LogoRight = Convert.FromHexString("00020201000100380307037d7dffc6387cdcfb77670707037cfcfb7777070703");

    /// <summary>CHR offset (from the start of the CHR data) of the tile $c7 slot.</summary>
    private const int RightHalfChrOffset = 0x1A060;

    /// <summary>
    /// Replaces every Link Doll in the ROM with the logo. Returns the number of places changed (0 if the ROM has no
    /// Link Doll, for example one that already has the logo).
    /// </summary>
    public static int Apply(byte[] rom)
    {
        int count = 0;
        // the iNES header's byte 4 is the PRG bank count; the CHR starts after the PRG
        int start = rom.Length > 16 ? 16 + rom[4] * 0x4000 : 16;
        for (int at = IndexOf(rom, LinkDoll, start); at >= 0; at = IndexOf(rom, LinkDoll, at + LinkDoll.Length))
        {
            Logo.CopyTo(rom, at);
            count++;
        }
        // the right half goes in once, in the fixed item-sprite slot (bank 13), not in every bank.
        // A ROM whose CHR is too small to have that slot keeps the left half only.
        int right = start + RightHalfChrOffset;
        if (count > 0 && right + LogoRight.Length <= rom.Length)
        {
            LogoRight.CopyTo(rom, right);
        }
        return count;
    }

    private static int IndexOf(byte[] data, byte[] pattern, int from)
    {
        int i = data.AsSpan(from).IndexOf(pattern);
        return i < 0 ? -1 : from + i;
    }
}
