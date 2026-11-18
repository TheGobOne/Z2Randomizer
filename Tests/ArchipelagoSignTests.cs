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
    public void Placeholder_EndsWithTheTownType_SoEachTownIsUnique()
    {
        var signs = new[] { TownType.RAURU, TownType.RUTO, TownType.SARIA, TownType.MIDO, TownType.NABOORU,
            TownType.DARUNIA, TownType.NEW_KASUTO, TownType.OLD_KASUTO }
            .Select(CustomTexts.ArchipelagoSignPlaceholder).ToList();

        signs.Select(s => s.EncodedText[^1]).Should().Equal(1, 2, 3, 4, 5, 6, 7, 8);
        signs.Distinct().Should().HaveCount(8);
    }
}
