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
    /// <summary>The left half of the AP logo, as the same 32 bytes (top tile, then bottom tile).</summary>
    public static readonly byte[] Logo = Convert.FromHexString("0002007dfdffc6380307037dfcfec60054b4ab27570707032848535727070703");

    /// <summary>
    /// The right half of the AP logo (32 bytes, top then bottom). It goes at CHR 0x1A060, the tile $c7 slot that
    /// Hyrule writes from item_sprites.chr. That slot is in the fixed $1C00 window, so it is the same in every room.
    /// The game draws an item's second sprite flipped horizontally, so these tiles are stored mirrored.
    /// </summary>
    public static readonly byte[] LogoRight = Convert.FromHexString("00020201000100380307037d7dffc6387cdcfb77670707037cfcfb7777070703");

    /// <summary>
    /// CHR offset of the Link Doll's tiles in the first sprite bank (CHR.ONEUP is the same tile in PPU space).
    /// The doll repeats in each of the first DollBanks 8 KB banks, at the same offset (Hyrule writes Mario mode's
    /// 1-up the same way). The offsets are fixed: a custom character sprite changes the doll's pixels, not where it is.
    /// </summary>
    public const int DollChrOffset = 0x0A80;
    public const int DollBanks = 13;
    private const int ChrBankSize = 0x2000;

    /// <summary>CHR offset (from the start of the CHR data) of the tile $c7 slot.</summary>
    private const int RightHalfChrOffset = 0x1A060;

    /// <summary>
    /// Writes the logo over the Link Doll in every sprite bank the ROM has, and the right half into the $c7 slot.
    /// Returns the number of banks written. A ROM whose CHR is too small for a bank or for the $c7 slot gets what fits.
    /// </summary>
    public static int Apply(byte[] rom)
    {
        // the iNES header's byte 4 is the PRG bank count; the CHR starts after the PRG
        int start = rom.Length > 16 ? 16 + rom[4] * 0x4000 : 16;
        int count = 0;
        for (int bank = 0; bank < DollBanks; bank++)
        {
            int at = start + bank * ChrBankSize + DollChrOffset;
            if (at + Logo.Length > rom.Length)
            {
                break;
            }
            Logo.CopyTo(rom, at);
            count++;
        }
        // the right half goes in once, in the fixed item-sprite slot (bank 13), not in every bank
        int right = start + RightHalfChrOffset;
        if (count > 0 && right + LogoRight.Length <= rom.Length)
        {
            LogoRight.CopyTo(rom, right);
        }
        return count;
    }
}
