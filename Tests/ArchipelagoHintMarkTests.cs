using FluentAssertions;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

[TestClass]
public class ArchipelagoHintMarkTests
{
    // the ROM layout the randomizer builds: 16 PRG banks of 16 KB, then 16 CHR banks of 8 KB
    private const int PrgBanks = 16;
    private const int ChrStart = 16 + PrgBanks * 0x4000;

    private static byte[] Rom()
    {
        var rom = new byte[ChrStart + 0x20000];
        rom[4] = PrgBanks;
        return rom;
    }

    [TestMethod]
    public void Glyph_IsBlankTopTile_ThenExclamationMarkBottomTile()
    {
        ArchipelagoHintMark.Glyph.Length.Should().Be(32);
        ArchipelagoHintMark.Glyph.AsSpan(0, 16).ToArray().Should().AllBeEquivalentTo((byte)0);
        ArchipelagoHintMark.Glyph.AsSpan(16, 16).ToArray().Should().Equal(
            0x18, 0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x00,
            0x18, 0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x00);
    }

    [TestMethod]
    public void Tile_IsTheBottomTileOfTheSpritePairStartingAtE4()
    {
        ArchipelagoHintMark.Tile.Should().Be(0xe5);
        ArchipelagoHintMark.GlyphChrOffset.Should().Be(0x1A000 + (0xE4 - 0xC0) * 16);
    }

    [TestMethod]
    public void HintMarksAsm_HintSize_MatchesTheHintPlaceholderSize()
    {
        string asm = Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.HintMarks.s");
        var match = System.Text.RegularExpressions.Regex.Match(asm, @"^HINT_SIZE\s*=\s*(\d+)", System.Text.RegularExpressions.RegexOptions.Multiline);
        match.Success.Should().BeTrue();
        int.Parse(match.Groups[1].Value).Should().Be(CustomTexts.ARCHIPELAGO_HINT_SIZE);
    }

    [TestMethod]
    public void Enabled_OnlyWhenHintMarkersIsOn_AndHelpfulHintsAreOn()
    {
        new RandomizerConfiguration().HintMarkers.Should().BeTrue("Hint Markers is on by default");

        var props = new RandomizerProperties { ArchipelagoMode = true, HelpfulHints = HelpfulHintOption.TOWNS_SEPARATE };
        ArchipelagoHintMark.Enabled(props).Should().BeFalse();

        props.HintMarkers = true;
        ArchipelagoHintMark.Enabled(props).Should().BeTrue();

        props.HelpfulHints = HelpfulHintOption.NONE;
        ArchipelagoHintMark.Enabled(props).Should().BeFalse();
    }

    [TestMethod]
    public void Apply_WritesTheGlyphOnlyInTilesE4_E5_OfTheFixedSpritePage()
    {
        var rom = Rom();
        var before = (byte[])rom.Clone();
        ArchipelagoHintMark.Apply(rom);
        rom.AsSpan(ChrStart + 0x1A240, 32).SequenceEqual(ArchipelagoHintMark.Glyph).Should().BeTrue();
        for (int i = 0; i < rom.Length; i++)
        {
            if (rom[i] == before[i]) { continue; }
            int chr = i - ChrStart;
            (chr >= 0x1A240 && chr < 0x1A260).Should().BeTrue($"byte {i:X} changed");
        }
    }
}
