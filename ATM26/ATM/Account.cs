namespace ATM;

// Representerar ett bankkonto: kapslar saldon och tillåter uttag och
// insättningar med validering av beloppen.
public class Account
{
    // Kontots aktuella saldo. Endast klassen själv kan ändra den.
    public int Balance { get; private set; }

    // Skapar ett konto med ett startsaldo.
    public Account(int initialBalance)
    {
        Balance = initialBalance;
    }

    // Returnerar kontots aktuella saldo.
    public int GetBalance()
    {
        return Balance;
    }

    // Tar ut ett belopp från kontot. Returnerar false (utan att ändra
    // saldon) om beloppet är <= 0 eller om kontot saknar täckning,
    // annars dras beloppet från saldon och true returneras.
    public bool Withdraw(int amount)
    {
        // Avvisa ogiltigt belopp eller uttag som överskrider saldon.
        if (amount <= 0 || amount > Balance)
        {
            return false;
        }

        Balance -= amount;
        return true;
    }
    
    // Sätter in ett belopp på kontot. Returnerar false om beloppet är
    // <= 0, annars ökar saldon och true returneras.
    public bool Deposit(int amount)
    {
        // Avvisa ogiltigt belopp.
        if (amount <= 0)
        {
            return false;
        }

        Balance += amount;
        return true;
    }
    
}