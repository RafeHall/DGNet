using DGNet.Event;
using DGNet.Serde;

namespace DGNet.Tests;

public enum KickReason
{
    None = 0,
    Cheating = 1,
    Idle = 2,
}

public static class KickReasonImpl
{
    public static string KickReasonString(this KickReason reason)
    {
        return reason switch
        {
            KickReason.Cheating => "Cheating",
            KickReason.Idle => "Idle",
            _ => "No Reason Given",
        };
    }
}

[EventGroup]
public partial record VoteEvent
{
    public partial record BeginMapVote(string[] Maps) : VoteEvent
    {
        public bool HasAnyMaps => Maps.Length != 0;
    }

    public partial record BeginKickVote(string Player, KickReason Reason) : VoteEvent { }
    public partial record EndVote(string Message, bool Passed) : VoteEvent { }
}

// public partial record VoteEvent
// {
//     public partial record BeginMapVote
//     {
//         public static void Serialize(BeginMapVote self, Serializer se)
//         {
//             se.SerializeArray<string>(self.Maps, (se, _, value) => se.SerializeString(value));
//         }

//         public static BeginMapVote Deserialize(Deserializer de)
//         {
//             return new(de.DeserializeArray((de, _) => de.DeserializeString()));
//         }
//     }

//     public partial record BeginKickVote
//     {
//         public static void Serialize(BeginKickVote self, Serializer se)
//         {
//             se.SerializeString(self.Player);
//             se.SerializeUInt8((byte)self.Reason);
//         }

//         public static BeginKickVote Deserialize(Deserializer de)
//         {
//             return new(
//                 de.DeserializeString(),
//                 (KickReason)de.DeserializeUInt8()
//             );
//         }
//     }

//     public partial record EndVote
//     {
//         public static void Serialize(EndVote self, Serializer se)
//         {
//             se.SerializeString(self.Message);
//             se.SerializeBool(self.Passed);
//         }

//         public static EndVote Deserialize(Deserializer de)
//         {
//             return new(
//                 de.DeserializeString(),
//                 de.DeserializeBool()
//             );
//         }
//     }
// }

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
    }

    public void EnterTree()
    {
        VoteEvent.BeginMapVote.Event += OnBeginMapEvent;
        VoteEvent.BeginKickVote.Event += OnBeginKickEvent;
        VoteEvent.EndVote.Event += OnVoteEndEvent;
    }

    public void ExitTree()
    {
        VoteEvent.BeginMapVote.Event -= OnBeginMapEvent;
        VoteEvent.BeginKickVote.Event -= OnBeginKickEvent;
        VoteEvent.EndVote.Event -= OnVoteEndEvent;
    }

    public void OnBeginMapEvent(VoteEvent.BeginMapVote ev)
    {
        if (!ev.HasAnyMaps)
        {
            return;
        }

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
        _menu.AddText(ev.Reason.KickReasonString());
        _menu.AddChoice("Yes", 1, 0);
        _menu.AddChoice("No", 0, 1);
    }

    public void OnVoteEndEvent(VoteEvent.EndVote ev)
    {
        _menu.Reset();
        _menu.AddText(ev.Message);
        if (ev.Passed)
        {
            _menu.ShowCheck();
        }
        else
        {
            _menu.ShowCross();
        }
        _menu.HideIn(3.0f);
    }
}