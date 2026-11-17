namespace ATM.ATM.Tests;

using Xunit;

public class CardTest
{
    [Fact]
    public void CardHasCardNumberTest()
    {
        string testCardNumber = "1234-5678";
        Account account = new Account(5000);
        Card card = new Card(testCardNumber, "1234", account);
        // Assert.True(card.CardNumber == "9999-9999", "Expected 9999-9999 but got " + card.CardNumber);
        Assert.Equal(testCardNumber, card.CardNumber);
    }

    [Fact]
    public void CardIsLinkedToAccountTest()
    {
        // setup
        Account account = new Account(5000);
        // given
        Card card = new Card("1234-5678", "1234", account);
        // then
        Assert.Same(account, card.Account);
    }

    [Theory]
    [InlineData("1234", "1234", true)]
    [InlineData("1234", "1235", false)]
    [InlineData("1234", "", false)]
    [InlineData("1234", "12345", false)]
    [InlineData("0000", "0000", true)]
    public void MatchesPinTest(string cardPin, string enteredPin, bool expectedMatch)
    {
        // Setup
        Account account = new Account(5000);
        Card card = new Card("1234-5678", cardPin, account);
        // When
        bool actualMatch = card.MatchesPin(enteredPin);
        // Then
        Assert.Equal(expectedMatch, actualMatch);
    }
}
