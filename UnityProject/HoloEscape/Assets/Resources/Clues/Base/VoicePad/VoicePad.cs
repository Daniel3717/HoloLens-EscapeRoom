using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace Clues.Base.VoicePad
{
    public class VoicePad : Clue
    {
        private KeywordRecognizer keywordRecognizer;
        private readonly Dictionary<string, Action> keywords = new Dictionary<string, Action>();

        public GameObject ObjectToTrigger;
        public string Password;

        public GameObject ScreenText;
        public string TriggerName;
        public bool visible;

        private GameObject WholeVoicePad;


        private void Start()
        {
            WholeVoicePad = GameObject.Find("VoicePad");

            if (!visible)
                WholeVoicePad.transform.localScale = new Vector3(0, 0, 0);

            ScreenText = GameObject.Find("voice_pad_text");
            ScreenText.GetComponent<TextMesh>().text = "Say Password";

            AddAction("OnCorrectPassword", ObjectToTrigger, TriggerName);

            keywords.Add(Password, () =>
            {
                var focusObject = GazeGestureManager.Instance.FocusedObject;
                if (focusObject != null)
                    focusObject.SendMessage("OnCorrectPassword");
            });

            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Action keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
                keywordAction.Invoke();
        }

        private void OnCorrectPassword()
        {
            Trigger("OnCorrectPassword");
            ScreenText.GetComponent<TextMesh>().text = "Correct!";
        }

        private void OnAppear()
        {
            if (!visible)
            {
                WholeVoicePad.transform.localScale = new Vector3(1F, 1F, 1F);
                visible = true;
            }
        }

        private void Update()
        {
        }
    }
}