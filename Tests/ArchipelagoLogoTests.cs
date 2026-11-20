using FluentAssertions;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

[TestClass]
public class ArchipelagoLogoTests
{
    private const int ChrStart = 16 + 8 * 0x4000;

    private static byte[] RomWithDolls(int dolls, int chrSize = 0x20000)
    {
        var rom = new byte[ChrStart + chrSize];
        rom[4] = 8; // PRG banks
        for (int i = 0; i < dolls; i++)
        {
            ArchipelagoLogo.LinkDoll.CopyTo(rom, 16 + 8 * 0x4000 + i * 0x100);
            rom[16 + 8 * 0x4000 + i * 0x100 + 16] = 0x6b; // a bottom tile that differs between builds
        }
        return rom;
    }

    [TestMethod]
    public void Apply_ReplacesEveryLinkDoll()
    {
        var rom = RomWithDolls(3);
        ArchipelagoLogo.Apply(rom).Should().Be(3);
        for (int i = 0; i < 3; i++)
        {
            rom.AsSpan(16 + 8 * 0x4000 + i * 0x100, 32).SequenceEqual(ArchipelagoLogo.Logo).Should().BeTrue();
        }
    }

    [TestMethod]
    public void Apply_LeavesARomWithoutLinkDollsAlone()
    {
        var rom = RomWithDolls(0);
        var before = (byte[])rom.Clone();
        ArchipelagoLogo.Apply(rom).Should().Be(0);
        rom.Should().Equal(before);
    }

    [TestMethod]
    public void Apply_WritesTheRightHalfInTheItemSpriteSlot()
    {
        var rom = RomWithDolls(1);
        ArchipelagoLogo.Apply(rom);
        rom.AsSpan(ChrStart + 0x1A060, 32).SequenceEqual(ArchipelagoLogo.LogoRight).Should().BeTrue();
    }

    [TestMethod]
    public void Apply_SmallChrKeepsTheLeftHalfOnly()
    {
        var rom = RomWithDolls(1, chrSize: 0x2000);
        ArchipelagoLogo.Apply(rom).Should().Be(1);
        rom.AsSpan(ChrStart, 32).SequenceEqual(ArchipelagoLogo.Logo).Should().BeTrue();
    }

    [TestMethod]
    public void Apply_RunTwiceChangesNothingTheSecondTime()
    {
        var rom = RomWithDolls(2);
        ArchipelagoLogo.Apply(rom);
        ArchipelagoLogo.Apply(rom).Should().Be(0);
    }
}
