namespace ATM;

// Bankomatens service: hanterar kortinsättning/utmatning, PIN-autentisering
// samt uttag och insättningar mellan bankomatens kassa och det konto som
// det insatta kortet är kopplat till.
public class AtmService
{
    // Det kort som för närvarande är satt i bankomaten (null = inget kort).
    private Card? _currentCard;
    // True när det insatta kortets PIN-kod har godkänts.
    private bool _isAuthenticated;

    // True så länge ett kort är satt i bankomaten.
    public bool HasCardInserted => _currentCard != null;
    // True när kortets PIN-kod har godkänts.
    public bool IsAuthenticated => _isAuthenticated;
    
    // Bankomatens eget kontantförråd (sedlar i kassan).
    public int AtmBalance { get; private set; }

    // Det senaste meddelandet bankomaten skickat till kunden.
    // Förklarar bland annat varför ett uttag eller en insättning avvisades.
    public string LastMessage { get; private set; } = string.Empty;

    // Skapar en bankomat med ett initialt kontantförråd.
    public AtmService(int initialBalance)
    {
        AtmBalance = initialBalance;
    }

    // Stoppar in ett kort i bankomaten. Autentiseringen nollställs
    // eftersom varje ny session kräver PIN-verifiering.
    public void InsertCard(Card card)
    {
        _currentCard = card;
        _isAuthenticated = false;
        LastMessage = "Kort insatt. Mata in din PIN-kod.";
    }

    // Matar ut det insatta kortet och avslutar sessionen helt.
    public void EjectCard()
    {
        _currentCard = null;
        _isAuthenticated = false;
        LastMessage = "Kortet har matats ut.";
    }

    // Verifierar en inmatad PIN-kod mot det insatta kortets PIN.
    // Returnerar true och sätter IsAuthenticated om koden stämmer,
    // annars false. Returnerar false om inget kort är satt.
    public bool EnterPin(string pinCode)
    {
        // Utan insatt kort kan ingen PIN verifieras.
        if (_currentCard == null)
        {
            LastMessage = "Inget kort är insatt.";
            return false;
        }

        // Kortet själv känner till sin PIN; bankomaten får bara ja/nej.
        _isAuthenticated = _currentCard.MatchesPin(pinCode);
        LastMessage = _isAuthenticated ? "PIN-koden är godkänd." : "Felaktig PIN-kod.";
        return _isAuthenticated;
    }

    // Returnerar saldon på det konto som det autentiserade kortet är
    // kopplat till. Kräver en autentiserad session.
    public int GetBalance()
    {
        EnsureAuthenticated();
        return _currentCard!.Account.GetBalance();
    }

    // Tar ut ett belopp ur bankomatens kassa och drar det från det
    // kopplade kontot. Returnerar false och sätter LastMessage om:
    //   - beloppet är <= 0 eller inte ett 100-tal,
    //   - kontot saknar täckning,
    //   - bankomatens kassa saknar kontanter.
    // I alla avvisade fall ändras varken konto eller kassa.
    public bool Withdraw(int amount)
    {
        EnsureAuthenticated();

        // Bankomaten har endast 100-kronorsedlar.
        if (amount <= 0 || amount % 100 != 0)
        {
            LastMessage = "Ogiltigt belopp. Beloppet måste vara ett positivt belopp i 100-tal.";
            return false;
        }

        // Kontot måste täcka hela beloppet.
        if (amount > _currentCard!.Account.Balance)
        {
            LastMessage = "Kontot har inte täckning.";
            return false;
        }

        // Kassan måste innehålla tillräckligt med kontanter.
        if (amount > AtmBalance)
        {
            LastMessage = "Kontot har täckning, men bankomaten saknar kontanter.";
            return false;
        }

        AtmBalance -= amount;
        _currentCard.Account.Withdraw(amount);
        LastMessage = $"Uttag på {amount} kr genomfört.";
        return true;
    }
    
    // Sätter in ett belopp på det kopplade kontot. Bankomatens kassa
    // påverkas inte. Avvisar belopp som är <= 0 eller inte 100-tal.
    public bool Deposit(int amount)
    {
        EnsureAuthenticated();

        // Bankomaten accepterar endast belopp i 100-tal.
        if (amount <= 0 || amount % 100 != 0)
        {
            LastMessage = "Ogiltigt belopp. Beloppet måste vara ett positivt belopp i 100-tal.";
            return false;
        }

        _currentCard!.Account.Deposit(amount);
        LastMessage = $"Insättning på {amount} kr genomfört.";
        return true;
    }
    
    // Säkerställer att ett kort är insatt och att dess PIN godkänts.
    // Kastar InvalidOperationException annars; används som förutsättning
    // för alla operationer som kräver en autentiserad session.
    public void EnsureAuthenticated()
    {
        if (_currentCard == null || !_isAuthenticated)
        {
            throw new InvalidOperationException("Ingen autentiserad session.");
        }
    }
}