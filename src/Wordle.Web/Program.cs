using Wordle.Domain;
using Wordle.Infrastructure;
using Wordle.Web;

var builder = WebApplication.CreateBuilder(args);

// --- Compose the dictionary once, at startup: remote French word list, local fallback. ---
// The HTTP call lives here, at the edge. The game never makes per-guess network calls.
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
var remoteWordList = new Uri("https://raw.githubusercontent.com/Taknok/French-Wordlist/master/francais.txt");
var remoteSource = new HttpRemoteWordSource(http, remoteWordList);
var wordList = await WordListLoader.LoadAsync(remoteSource, FrenchWords.FiveLetters);

builder.Services.AddSingleton<IWordDictionary>(new InMemoryWordDictionary(wordList.Words));
builder.Services.AddSingleton<GameStore>();

var app = builder.Build();

app.Logger.LogInformation(
    "Dictionnaire chargé : {Count} mots de 5 lettres ({Source}).",
    wordList.Words.Count,
    wordList.FromRemote ? "source distante" : "repli local");

app.UseDefaultFiles();
app.UseStaticFiles();

// Start a new game.
app.MapPost("/api/games", (GameStore store, IWordDictionary dictionary) =>
{
    var game = new Game(dictionary);
    var id = store.Add(game);
    return Results.Ok(new NewGameResponse(
        id, Game.MaxAttempts, Word.Length, game.Status.ToString(), game.RemainingAttempts));
});

// Play a guess on an existing game.
app.MapPost("/api/games/{id:guid}/guesses", (Guid id, GuessRequest request, GameStore store) =>
{
    var game = store.Find(id);
    if (game is null)
        return Results.NotFound();

    // Format is validated by the domain: a malformed input never becomes a Word.
    var creation = Word.Create(request.Word ?? string.Empty);
    if (creation.IsFailure)
        return Results.Ok(Rejected(creation.Error.ToString(), game));

    var outcome = game.Play(creation.Value);
    if (outcome.IsFailure)
        return Results.Ok(Rejected(outcome.Error.ToString(), game));

    var feedback = outcome.Value.Letters.Select(ToToken).ToList();
    var secret = game.Status == GameStatus.Lost ? game.Secret.Text : null;

    return Results.Ok(new GuessResponse(
        Accepted: true,
        Reason: null,
        Feedback: feedback,
        IsWin: outcome.Value.IsWin,
        Status: game.Status.ToString(),
        RemainingAttempts: game.RemainingAttempts,
        Secret: secret));
});

app.Run();

static GuessResponse Rejected(string reason, Game game) =>
    new(false, reason, null, false, game.Status.ToString(), game.RemainingAttempts, null);

static string ToToken(LetterEvaluation evaluation) => evaluation.Feedback switch
{
    LetterFeedback.Correct => "correct",
    LetterFeedback.Misplaced => "misplaced",
    _ => "absent"
};
