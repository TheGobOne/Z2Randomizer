using System;
using js65;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Archipelago mode with helpful hints: a "?" over each town NPC whose text is an unread helpful hint (Asm/HintMarks.s).
/// The "?" is one 8x16 sprite in tiles $e4/$e5 of the fixed $1C00 sprite page (CHR bank $68, mapped in every room).
/// item_sprites.chr puts art there, but no entry of ItemTileTable or CommonEnemyTileTable (FullItemShuffle.s) uses
/// tile $e5, so nothing else draws it. The only blank pair, $f4/$f5, is the blank sprite weapons and NPCs use.
/// </summary>
public static class ArchipelagoHintMark
{
    /// <summary>Tile byte of the sprite. Odd, so it picks the $1000 pattern table; the top tile is $e4 and the bottom is $e5.</summary>
    public const byte Tile = 0xe5;

    /// <summary>
    /// A blank top tile ($e4), then the "?" in color 3 (both planes) as the bottom tile ($e5). With sprite palette 1 that
    /// is white in towns. No town sprite palette has yellow; color 1 of palette 1 is red.
    /// </summary>
    public static readonly byte[] Glyph = Convert.FromHexString("00000000000000000000000000000000" + "3C66060C180018003C66060C18001800");

    /// <summary>CHR offset of tile $e4 in the fixed item page, bank $68 (0x1A000 + 0x240).</summary>
    public const int GlyphChrOffset = 0x1A240;

    /// <summary>
    /// Writes the "?" into the fixed item page. Call after everything else that writes bank $68.
    /// </summary>
    public static void Apply(byte[] rom)
    {
        int start = rom.Length > 16 ? 16 + rom[4] * 0x4000 : 16;
        int at = start + GlyphChrOffset;
        if (at + Glyph.Length > rom.Length)
        {
            throw new InvalidOperationException("ROM CHR is too small for the Archipelago hint mark");
        }
        Glyph.CopyTo(rom, at);
    }

    /// <summary>Adds the draw and read hooks.</summary>
    public static void AddHooks(Assembler engine)
    {
        engine.Module().Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.HintMarks.s"), "hint_marks.s");
    }

    public static bool Enabled(RandomizerProperties props) =>
        props.ArchipelagoMode && props.HintMarkers && props.HelpfulHints != HelpfulHintOption.NONE;
}
