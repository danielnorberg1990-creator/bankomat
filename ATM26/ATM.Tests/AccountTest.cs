namespace ATM.ATM.Tests;

using Xunit;

// Enhetstester för Account-klassen: konstruktor, GetBalance, Withdraw och Deposit.
public class AccountTest
{
    // Kontot ska ha exakt det saldo som angavs i konstruktorn.
    [Fact]
    public void AccountHasBalanceTest()
    {
        int testBalanceVal = 5000;
        Account account = new Account(testBalanceVal);
        // Assert.True(account.Balance == 6000, "Expected 6000 but got " + account.Balance);
        Assert.Equal(testBalanceVal, account.Balance);
    }

    // GetBalance ska returnera samma värde som Balance-propertyn.
    [Fact]
    public void GetBalanceTest()
    {
        Account account = new Account(5000);
        Assert.Equal(5000, account.GetBalance());
    }

    // Withdraw: verifierar både returvärdet och saldon efteråt.
    // Giltiga uttag dras från saldon; ogiltiga (mer än saldot, noll,
    // negativt) ska returnera false och saldon ska vara oförändrad.
    [Theory]
    [InlineData(5000, 5000, 0, true)]     // hela saldot
    [InlineData(5000, 100, 4900, true)]   // normalt uttag
    [InlineData(5000, 6000, 5000, false)] // mer än saldot
    [InlineData(5000, 0, 5000, false)]    // noll
    [InlineData(5000, -1000, 5000, false)] // negativt
    public void WithdrawTest(int accountBalance, int withdrawAmount, int expectedBalance, bool expectedResult)
    {
        // Setup
        Account account = new Account(accountBalance);
        // When
        bool result = account.Withdraw(withdrawAmount);
        // Then
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedBalance, account.GetBalance());
    }

    // Deposit: giltiga insättningar ska öka saldon; noll och negativa
    // belopp ska avvisas utan effekt.
    [Theory]
    [InlineData(5000, 3000, 8000, true)]
    [InlineData(5000, 0, 5000, false)]
    [InlineData(5000, -1000, 5000, false)]
    public void DepositTest(int accountBalance, int depositAmount, int expectedBalance, bool expectedResult)
    {
        // Setup
        Account account = new Account(accountBalance);
        // When
        bool result = account.Deposit(depositAmount);
        // Then
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedBalance, account.GetBalance());
    }
}
