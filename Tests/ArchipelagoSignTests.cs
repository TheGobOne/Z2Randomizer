using System.Linq;
using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.Tests;

[TestClass]
public class ArchipelagoSignTests
{
    [TestMethod]
    public void Placeholder_ReadsApItemTown()
    {
        Text sign = CustomTexts.ArchipelagoSignPlaceholder(TownType.MIDO);

        byte[] text = sign.EncodedText.TakeWhile(b => b != 0xFF).ToArray();
        Util.FromGameText(text).Should().Be(Util.FromGameText(Util.ToGameText("AP$ITEM$TOWN")));
    }

    [TestMethod]
    public void Placeholder_HasRoomForTheLongestSign()
    {
        byte[] bytes = CustomTexts.ArchipelagoSignPlaceholder(TownType.RAURU).EncodedText;

        bytes.Should().HaveCount(CustomTexts.ARCHIPELAGO_SIGN_SIZE);
        // the longest sign plus its end byte must fit before the town byte
        Util.ToGameText("MAGIC$CONTAINER$TOWN", true).Length.Should().BeLessThanOrEqualTo(bytes.Length - 1);
    }

    [TestMethod]
    public void HintPlaceholder_HasRoomForTheLongestHint_AndEndsWithItsNumber()
    {
        byte[] bytes = CustomTexts.ArchipelagoHintPlaceholder(3).EncodedText;

        bytes.Should().HaveCount(CustomTexts.ARCHIPELAGO_HINT_SIZE);
        bytes[^1].Should().Be(0x83);
        // the longest place plus an item name of two full lines, and its end byte, must fit before the number byte
        Util.ToGameText("carock$disappears$with the$MAGICMAGIC$CONTAINERS", true).Length
            .Should().BeLessThanOrEqualTo(bytes.Length - 1);
    }

    [TestMethod]
    public void SpellHintPlaceholder_HasRoomForFourFullLines_AndEndsWithTheItemsNumber()
    {
        byte[] bytes = CustomTexts.ArchipelagoSpellHintPlaceholder(Collectable.MIRROR).EncodedText;

        bytes.Should().HaveCount(CustomTexts.ARCHIPELAGO_SPELL_HINT_SIZE);
        bytes[^1].Should().Be(0xA3);
        Util.ToGameText("DOWNSTAB AT$ABCDEFGHIJK$ABCDEFGHIJK$ABCDEFGHIJK", true).Length
            .Should().BeLessThanOrEqualTo(bytes.Length - 1);
    }

    [TestMethod]
    public void ItemTextPlaceholder_HasRoomForFourFullLines_AndEndsWithItsNumber()
    {
        byte[] bytes = CustomTexts.ArchipelagoItemTextPlaceholder(5).EncodedText;

        bytes.Should().HaveCount(CustomTexts.ARCHIPELAGO_ITEM_TEXT_SIZE);
        bytes[^1].Should().Be(0xC5);
        Util.ToGameText("ABCDEFGHIJK$ABCDEFGHIJK$ABCDEFGHIJK$ABCDEFGHIJK", true).Length
            .Should().BeLessThanOrEqualTo(bytes.Length - 1);
    }

    [TestMethod]
    public void SpellHintPlaceholders_AreUniqueAndDifferFromHelpfulHints()
    {
        var all = CustomTexts.ARCHIPELAGO_SPELL_HINT_ITEMS.Select(i => CustomTexts.ArchipelagoSpellHintPlaceholder(i).EncodedText[^1])
            .Concat(Enumerable.Range(0, 8).Select(n => CustomTexts.ArchipelagoHintPlaceholder(n).EncodedText[^1])).ToList();

        all.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void Placeholder_EndsWithTheTownType_SoEachTownIsUnique()
    {
        var signs = new[] { TownType.RAURU, TownType.RUTO, TownType.SARIA, TownType.MIDO, TownType.NABOORU,
            TownType.DARUNIA, TownType.NEW_KASUTO, TownType.OLD_KASUTO }
            .Select(CustomTexts.ArchipelagoSignPlaceholder).ToList();

        signs.Select(s => s.EncodedText[^1]).Should().Equal(1, 2, 3, 4, 5, 6, 7, 8);
        signs.Distinct().Should().HaveCount(8);
    }
}
