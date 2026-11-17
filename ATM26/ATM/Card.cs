namespace ATM;

// Representerar ett bankkort: lagrar kortnummer, PIN-kod och en referens
// till det konto som kortet är kopplat till.
public class Card
{
    // Kortnumret (endast läsning).
    public string CardNumber { get; }
    // PIN-koden som krävs för att autentisera kortet (endast läsning).
    public string PinCode { get; }
    // Det konto som kortet är kopplat till (endast läsning).
    public Account Account { get; }

    // Skapar ett kort med angivet nummer, PIN-kod och kopplat konto.
    public Card(string cardNumber, string pinCode, Account account)
    {
        CardNumber = cardNumber;
        PinCode = pinCode;
        Account = account;
    }

    // Jämför en inmatad PIN-kod med kortets PIN. Returnerar true om de
    // stämmer överens, annars false.
    public bool MatchesPin(string pinCode)
    {
        return PinCode == pinCode;
    }
}