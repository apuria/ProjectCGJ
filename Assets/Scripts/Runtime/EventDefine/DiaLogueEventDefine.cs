using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;

public class DiaLogueEventDefine
{
    public class ShowUI : IEventMessage
    {
        public List<Speaker> speakers;
        public bool hasBackground;
        public Sprite BackGround;
        public bool hasReturnButton;
        public static void SendEventMessage(List<Speaker> speakers, bool hasBackground = false, Sprite BackGround = null, bool hasReturnButton = false)
        {
            var eventMessage = new ShowUI();
            eventMessage.speakers = speakers;
            eventMessage.hasBackground = hasBackground;
            eventMessage.BackGround = BackGround;
            eventMessage.hasReturnButton = hasReturnButton;
            UniEvent.SendMessage(eventMessage);
        }
    }

    public class Next : IEventMessage
    {
        public static void SendEventMessage()
        {
            var eventMessage = new Next();
            UniEvent.SendMessage(eventMessage);
        }
    }

    public class UpdateUI : IEventMessage
    {
        public int leftRoleIndex;
        public int rightRoleIndex;
        public string content;
        public TalkingSpeaker speaker;
        public EDialogueType dialogueType;
        public Sprite cgImage;
        public bool showDialogueBox;

        public static void SendEventMessage(int leftRoleIndex, int rightRoleIndex, string content, TalkingSpeaker speaker,
            EDialogueType dialogueType = EDialogueType.CharacterDialogue, Sprite cgImage = null, bool showDialogueBox = true)
        {
            var eventMessage = new UpdateUI();
            eventMessage.leftRoleIndex = leftRoleIndex;
            eventMessage.rightRoleIndex = rightRoleIndex;
            eventMessage.content = content;
            eventMessage.speaker = speaker;
            eventMessage.dialogueType = dialogueType;
            eventMessage.cgImage = cgImage;
            eventMessage.showDialogueBox = showDialogueBox;
            UniEvent.SendMessage(eventMessage);
        }
    }
}
