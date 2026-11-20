using FluentAssertions;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

[TestClass]
public class ArchipelagoLogoTests
{
    private const int ChrStart = 16 + 8 * 0x4000;

    private static byte[] Rom(int chrSize = 0x20000, byte doll = 0x00)
    {
        var rom = new byte[ChrStart + chrSize];
        rom[4] = 8; // PRG banks
        for (int bank = 0; bank < ArchipelagoLogo.DollBanks; bank++)
        {
            int at = ChrStart + bank * 0x2000 + ArchipelagoLogo.DollChrOffset;
            if (at + 32 <= rom.Length)
            {
                rom.AsSpan(at, 32).Fill(doll);
            }
        }
        return rom;
    }

    private static bool LogoAt(byte[] rom, int bank) =>
        rom.AsSpan(ChrStart + bank * 0x2000 + ArchipelagoLogo.DollChrOffset, 32).SequenceEqual(ArchipelagoLogo.Logo);

    [TestMethod]
    public void Apply_WritesTheLogoInEverySpriteBank()
    {
        var rom = Rom();
        ArchipelagoLogo.Apply(rom).Should().Be(ArchipelagoLogo.DollBanks);
        for (int bank = 0; bank < ArchipelagoLogo.DollBanks; bank++)
        {
            LogoAt(rom, bank).Should().BeTrue();
        }
    }

    [TestMethod]
    public void Apply_ReplacesACustomCharactersDoll()
    {
        // a custom character sprite draws its own doll; the logo still goes in the same place
        var rom = Rom(doll: 0x5a);
        ArchipelagoLogo.Apply(rom).Should().Be(ArchipelagoLogo.DollBanks);
        LogoAt(rom, 0).Should().BeTrue();
        LogoAt(rom, ArchipelagoLogo.DollBanks - 1).Should().BeTrue();
    }

    [TestMethod]
    public void Apply_ChangesOnlyTheDollsAndTheRightHalfSlot()
    {
        var rom = Rom();
        var before = (byte[])rom.Clone();
        ArchipelagoLogo.Apply(rom);
        for (int i = 0; i < rom.Length; i++)
        {
            if (rom[i] == before[i]) { continue; }
            int chr = i - ChrStart;
            bool inDoll = chr % 0x2000 >= ArchipelagoLogo.DollChrOffset && chr % 0x2000 < ArchipelagoLogo.DollChrOffset + 32
                          && chr / 0x2000 < ArchipelagoLogo.DollBanks;
            bool inRight = chr >= 0x1A060 && chr < 0x1A080;
            (inDoll || inRight).Should().BeTrue($"byte {i:X} changed");
        }
    }

    [TestMethod]
    public void Apply_WritesTheRightHalfInTheItemSpriteSlot()
    {
        var rom = Rom();
        ArchipelagoLogo.Apply(rom);
        rom.AsSpan(ChrStart + 0x1A060, 32).SequenceEqual(ArchipelagoLogo.LogoRight).Should().BeTrue();
    }

    [TestMethod]
    public void Apply_SmallChrKeepsTheLeftHalfOnly()
    {
        var rom = Rom(chrSize: 0x2000);
        ArchipelagoLogo.Apply(rom).Should().Be(1);
        LogoAt(rom, 0).Should().BeTrue();
    }

    [TestMethod]
    public void Apply_RunTwiceGivesTheSameRom()
    {
        var rom = Rom();
        ArchipelagoLogo.Apply(rom);
        var once = (byte[])rom.Clone();
        ArchipelagoLogo.Apply(rom);
        rom.Should().Equal(once);
    }
}
