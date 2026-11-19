using FluentAssertions;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

[TestClass]
public class ArchipelagoLogoTests
{
    private static byte[] RomWithDolls(int dolls)
    {
        var rom = new byte[16 + 8 * 0x4000 + 0x2000];
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
    public void Apply_RunTwiceChangesNothingTheSecondTime()
    {
        var rom = RomWithDolls(2);
        ArchipelagoLogo.Apply(rom);
        ArchipelagoLogo.Apply(rom).Should().Be(0);
    }
}
