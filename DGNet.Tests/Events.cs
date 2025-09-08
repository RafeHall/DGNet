using DGNet.Event;

namespace DGNet.Tests;


public enum KickReason : byte
{
    None = 0,
    Cheating = 1,
    Idle = 2,
    Spamming = 3,
}

public static class KickReasonImpl
{
    public static string ToString(this KickReason reason)
    {
        return reason switch
        {
            KickReason.Cheating => "Cheating",
            KickReason.Idle => "Idle",
            KickReason.Spamming => "Spamming",
            _ => "No Reason Given",
        };
    }
}

public enum VoteResult : byte
{
    Passed = 0,
    Failed = 1,
    NotEnoughVotes = 2,
}

public static class VoteResultImpl
{
    public static string ToString(this VoteResult result)
    {
        return result switch
        {
            VoteResult.Passed => "Passed",
            VoteResult.Failed => "Failed",
            VoteResult.NotEnoughVotes => "Not Enough Votes",
            _ => "Unknown Result",
        };
    }
}

interface ITest
{
    public static abstract void Test();
}

[GenerateEvents]
public partial record VoteEvent
{
    public partial record BeginMapVote(string[] Maps) : VoteEvent { }
    public partial record BeginKickVote(string Player, KickReason Reason) : VoteEvent { }
    public partial record EndVote(string Message, VoteResult Result) : VoteEvent { }
    public partial record ClearVoting(string[] Messages) : VoteEvent { }
}

// dummy GUI interface
public interface IVoteMenu
{
    public string Title { get; set; }

    public void AddChoice(string text, int id, int fKey);
    public void AddText(string text);
    public void Reset();
    public void Hide();
    public void HideIn(float time);
    public void ShowCheck();
    public void ShowCross();
}

public class UsageExample
{
    private readonly IVoteMenu _menu;

    public UsageExample()
    {
        _menu = null!; // yes

        VoteEvent.BeginMapVote.Event += OnBeginMapEvent;
        VoteEvent.BeginKickVote.Event += OnBeginKickEvent;
        VoteEvent.EndVote.Event += OnVoteEndEvent;
        VoteEvent.ClearVoting.Event += OnClearVoteEvent;
    }

    ~UsageExample()
    {
        VoteEvent.BeginMapVote.Event -= OnBeginMapEvent;
        VoteEvent.BeginKickVote.Event -= OnBeginKickEvent;
        VoteEvent.EndVote.Event -= OnVoteEndEvent;
        VoteEvent.ClearVoting.Event -= OnClearVoteEvent;
    }

    private void OnClearVoteEvent(VoteEvent.ClearVoting ev)
    {
        _menu.Reset();
        _menu.Hide();
    }

    public void OnBeginMapEvent(VoteEvent.BeginMapVote ev)
    {
        _menu.Reset();
        _menu.Title = "What Map Next?";

        foreach (var (index, map) in ev.Maps.Index())
        {
            _menu.AddChoice(map, index, index);
        }
    }

    public void OnBeginKickEvent(VoteEvent.BeginKickVote ev)
    {
        _menu.Reset();
        _menu.Title = "Kick Player?";
        _menu.AddText(ev.Player);
        _menu.AddText(ev.Reason.ToString());
        _menu.AddChoice("Yes", 1, 0);
        _menu.AddChoice("No", 0, 1);
    }

    public void OnVoteEndEvent(VoteEvent.EndVote ev)
    {
        _menu.Reset();
        _menu.AddText(ev.Message);
        _menu.AddText(ev.Result.ToString());
        switch (ev.Result)
        {
            case VoteResult.Passed:
                _menu.ShowCheck();
                break;
            case VoteResult.Failed:
                _menu.ShowCross();
                break;
        }
        _menu.HideIn(3.0f);
    }
}