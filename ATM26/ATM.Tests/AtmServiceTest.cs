namespace ATM.ATM.Tests;

using Xunit;

// Enhetstester för AtmService: en test per metod samt de interaktiva
// scenarion (kort in, PIN, uttag med/utan täckning, kort ut).
public class AtmServiceTest
{
    private const string CorrectPin = "1234";
    private const string WrongPin = "2345";

    // Testhjälpare: bygger en bankomat med ett kort stoppat in.
    // Med authenticate = true godkänns dessutom kortets PIN, så att
    // sessionen är redo för saldo-/uttags-/insättningstester.
    private static (Account account, Card card, AtmService atm) CreateAtmWithInsertedCard(
        int accountBalance = 5000,
        int atmBalance = 7000,
        bool authenticate = false)
    {
        Account account = new Account(accountBalance);
        Card card = new Card("1234-5678", CorrectPin, account);
        AtmService atm = new AtmService(atmBalance);
        atm.InsertCard(card);

        if (authenticate)
        {
            atm.EnterPin(CorrectPin);
        }

        return (account, card, atm);
    }

    // Bankomaten ska hålla den kontantbalans som gavs vid skapande.
    [Fact]
    public void AtmServiceHasCashTest()
    {
        AtmService atm = new AtmService(7000);
        Assert.Equal(7000, atm.AtmBalance);
    }

    // Efter kortinsättning ska bankomaten veta att ett kort är inne,
    // utan att sessionen är autentiserad.
    [Fact]
    public void InsertCardTest()
    {
        // given: kort stoppas in i bankomaten
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard();
        // then: bankomaten vet att ett kort är inne, men sessionen är inte autentiserad
        Assert.True(atm.HasCardInserted);
        Assert.False(atm.IsAuthenticated);
    }

    // Felaktig PIN (2345) ska avvisas: bankomaten ska veta att koden var fel.
    [Fact]
    public void EnterWrongPinTest()
    {
        // given: kortets PIN är 1234
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard();
        // when: felaktig pinkod matas in
        bool result = atm.EnterPin(WrongPin);
        // then: bankomaten vet att koden var fel
        Assert.False(result);
        Assert.False(atm.IsAuthenticated);
    }

    // Korrekt PIN (1234) ska godkännas och autentisera sessionen.
    [Fact]
    public void EnterCorrectPinTest()
    {
        // given
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard();
        // when: korrekt pinkod matas in
        bool result = atm.EnterPin(CorrectPin);
        // then: bankomaten vet att koden var rätt
        Assert.True(result);
        Assert.True(atm.IsAuthenticated);
    }

    // En felaktig PIN efter en korrekt PIN ska återställa autentiseringen.
    [Fact]
    public void WrongPinAfterCorrectPinResetsAuthenticationTest()
    {
        // given: en autentiserad session
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        Assert.True(atm.IsAuthenticated);
        // when: felaktig pinkod matas in
        bool result = atm.EnterPin(WrongPin);
        // then: sessionen är inte längre autentiserad
        Assert.False(result);
        Assert.False(atm.IsAuthenticated);
    }

    // PIN-verifiering utan insatt kort ska returnera false utan att kasta.
    [Fact]
    public void EnterPinWithoutCardTest()
    {
        AtmService atm = new AtmService(7000);
        bool result = atm.EnterPin(CorrectPin);
        Assert.False(result);
        Assert.False(atm.IsAuthenticated);
    }

    // GetBalance ska returnera saldon på det kopplade kontot.
    [Fact]
    public void GetBalanceTest()
    {
        // given: autentiserad session med kontot 5000 kr
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when/then
        Assert.Equal(5000, atm.GetBalance());
    }

    // GetBalance utan autentiserad session ska kasta InvalidOperationException.
    [Fact]
    public void GetBalanceWhenNotAuthenticatedTest()
    {
        // given: kort insatt men ingen PIN godkänd
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard();
        // when/then
        Assert.Throws<InvalidOperationException>(() => atm.GetBalance());
    }

    // Uttag av 5000 (ATM 7000, konto 5000) ska lyckas: beloppet dras
    // från både kontot och kassan.
    [Fact]
    public void WithdrawWithinLimitsTest()
    {
        // given: bankomaten har 7000 kr och kontot 5000 kr
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when
        bool result = atm.Withdraw(5000);
        // then: balansen tas från kontot som är kopplat till kortet (och från bankomaten)
        Assert.True(result);
        Assert.Equal(0, account.GetBalance());
        Assert.Equal(2000, atm.AtmBalance);
    }

    // Uttag av 6000 med kontot på 5000 ska avvisas: inget dras, och
    // kunden ska få veta att kontot saknar täckning.
    [Fact]
    public void WithdrawExceedingAccountBalanceTest()
    {
        // given: bankomaten har 7000 kr och kontot 5000 kr
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when
        bool result = atm.Withdraw(6000);
        // then: balansen dras inte från kontot eller bankomaten, och kunden meddelas att kontot inte har täckning
        Assert.False(result);
        Assert.Equal(5000, account.GetBalance());
        Assert.Equal(7000, atm.AtmBalance);
        Assert.Equal("Kontot har inte täckning.", atm.LastMessage);
    }

    // Uttag av 8000 med kontot på 8000 men ATM på 7000 ska avvisas:
    // inget dras, och kunden ska få veta att bankomaten saknar kontanter.
    [Fact]
    public void WithdrawExceedingAtmCashTest()
    {
        // given: bankomaten har 7000 kr och kontot höjs till 8000 kr
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        account.Deposit(3000);
        Assert.Equal(8000, account.GetBalance());
        // when
        bool result = atm.Withdraw(8000);
        // then: balansen dras inte från kontot eller bankomaten, och kunden meddelas att bankomaten saknar kontanter
        Assert.False(result);
        Assert.Equal(8000, account.GetBalance());
        Assert.Equal(7000, atm.AtmBalance);
        Assert.Equal("Kontot har täckning, men bankomaten saknar kontanter.", atm.LastMessage);
    }

    // Ogiltiga uttagsbelopp (0, negativt, inte 100-tal) ska avvisas
    // utan effekt på vare sig konto eller kassa.
    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    [InlineData(75)]
    public void WithdrawInvalidAmountTest(int amount)
    {
        // given
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when
        bool result = atm.Withdraw(amount);
        // then: inget händer vare sig med kontot eller bankomaten
        Assert.False(result);
        Assert.Equal(5000, account.GetBalance());
        Assert.Equal(7000, atm.AtmBalance);
    }

    // Uttag utan autentiserad session ska kasta InvalidOperationException.
    [Fact]
    public void WithdrawWhenNotAuthenticatedTest()
    {
        // given: kort insatt men ingen PIN godkänd
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard();
        // when/then
        Assert.Throws<InvalidOperationException>(() => atm.Withdraw(500));
    }

    // Insättning ska öka kontot utan att påverka bankomatens kassa.
    [Fact]
    public void DepositTest()
    {
        // given: autentiserad session
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when
        bool result = atm.Deposit(1000);
        // then: kontot ökar, bankomatens kontanter påverkas inte
        Assert.True(result);
        Assert.Equal(6000, account.GetBalance());
        Assert.Equal(7000, atm.AtmBalance);
    }

    // Ogiltiga insättningsbelopp (0, negativt, inte 100-tal) ska avvisas
    // utan effekt på kontot.
    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    [InlineData(75)]
    public void DepositInvalidAmountTest(int amount)
    {
        // given
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when
        bool result = atm.Deposit(amount);
        // then: kontot påverkas inte
        Assert.False(result);
        Assert.Equal(5000, account.GetBalance());
    }

    // Insättning utan autentiserad session ska kasta InvalidOperationException.
    [Fact]
    public void DepositWhenNotAuthenticatedTest()
    {
        // given: kort insatt men ingen PIN godkänd
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard();
        // when/then
        Assert.Throws<InvalidOperationException>(() => atm.Deposit(1000));
    }

    // Efter utmatning ska bankomaten ha glömt kortet och sessionen
    // vara helt avslutad (även PIN-verifiering ska därefter avvisas).
    [Fact]
    public void EjectCardTest()
    {
        // given: en autentiserad session
        (Account account, Card card, AtmService atm) = CreateAtmWithInsertedCard(authenticate: true);
        // when
        atm.EjectCard();
        // then: bankomaten vet inte längre att ett kort är inne
        Assert.False(atm.HasCardInserted);
        Assert.False(atm.IsAuthenticated);
        Assert.False(atm.EnterPin(CorrectPin));
    }

    // EnsureAuthenticated ska kasta för ej autentiserade sessioner
    // (både utan kort och utan godkänd PIN) och låta en autentiserad
    // session passera.
    [Fact]
    public void EnsureAuthenticatedTest()
    {
        // given: tre tillstånd - inget kort, kort utan PIN, kort med godkänd PIN
        AtmService noCardAtm = new AtmService(7000);
        (Account account, Card card, AtmService insertedAtm) = CreateAtmWithInsertedCard();
        (Account account2, Card card2, AtmService authenticatedAtm) = CreateAtmWithInsertedCard(authenticate: true);
        // when/then
        Assert.Throws<InvalidOperationException>(() => noCardAtm.EnsureAuthenticated());
        Assert.Throws<InvalidOperationException>(() => insertedAtm.EnsureAuthenticated());
        authenticatedAtm.EnsureAuthenticated(); // kastar inte undan autentiserad session
    }
}