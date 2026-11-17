namespace ATM.ATM.Tests;

using Xunit;

// Enhetstester för Card-klassen: kortnummer, PIN-kod, koppling till konto
// samt metoden MatchesPin.
public class CardTest
{
    // Kortet ska lagra exakt det kortnummer som gavs i konstruktorn.
    [Fact]
    public void CardHasCardNumberTest()
    {
        string testCardNumber = "1234-5678";
        Account account = new Account(5000);
        Card card = new Card(testCardNumber, "1234", account);
        // Assert.True(card.CardNumber == "9999-9999", "Expected 9999-9999 but got " + card.CardNumber);
        Assert.Equal(testCardNumber, card.CardNumber);
    }

    // Kortet ska lagra exakt den PIN-kod som gavs i konstruktorn.
    [Fact]
    public void CardHasPinCodeTest()
    {
        string testPin = "1234";
        Account account = new Account(5000);
        Card card = new Card("1234-5678", testPin, account);
        Assert.Equal(testPin, card.PinCode);
    }

    // Kortet ska vara kopplat till just det konto som gavs i konstruktorn
    // (samma objektinstans, inte en kopia).
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

    // MatchesPin: true endast när inmatad PIN är exakt lika med kortets PIN.
    // Testar även tom PIN, för lång PIN och ett annat kort med annan PIN.
    [Theory]
    [InlineData("1234", "1234", true)]
    [InlineData("1234", "2345", false)]
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
