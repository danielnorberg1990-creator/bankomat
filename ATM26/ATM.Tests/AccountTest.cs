namespace ATM.ATM.Tests;

using Xunit;

public class AccountTest
{
    [Fact]
    public void AccountHasBalanceTest()
    {
        int testBalanceVal = 5000;
        Account account = new Account(testBalanceVal);
        // Assert.True(account.Balance == 6000, "Expected 6000 but got " + account.Balance);
        Assert.Equal(testBalanceVal, account.Balance);
    }

    [Fact]
    public void InsertCardTest()
    {
        // setup
        Account account = new Account(5000);
        // given
        Card card = new Card("1234-5678", "1234", account);
        AtmService atm = new AtmService(5000);
        // when
        atm.InsertCard(card);
        // then
        Assert.True(atm.HasCardInserted);
    }

    [Theory]
    [InlineData(5000, 5000, 0)]
    [InlineData(5000, 100, 4900)]
    [InlineData(5000, 6000, 5000)] // om jag tar ut..
    [InlineData(5000, 0, 5000)]
    [InlineData(5000, -1000, 5000)]
    [InlineData(5000, 75, 5000)]
    public void WithdrawTest(int accountBalance, int withdraw, int expectedAccountBalance)
    {
        // Setup
        Account account = new Account(accountBalance);
        Card card = new Card("1234-5678", "1234", account);
        AtmService atm = new AtmService(10000);
        atm.InsertCard(card);
        atm.EnterPin("1234");
        // When
        atm.Withdraw(withdraw);
        // Then
        int actualAccountBalance = account.GetBalance();
        Assert.Equal(expectedAccountBalance, actualAccountBalance);
    }
}
