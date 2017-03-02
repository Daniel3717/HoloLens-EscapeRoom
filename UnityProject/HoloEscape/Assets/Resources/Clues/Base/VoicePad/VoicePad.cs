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
        public bool Visible;


        private void Start()
        {
            SetProperty("visible", Visible);
            SetProperty("password", Password);

            Initialise();

            ScreenText = GameObject.Find("voice_pad_text");
            ScreenText.GetComponent<TextMesh>().text = "Say Password";

            if (ObjectToTrigger != null)
            {
                AddAction("OnCorrectPassword", ObjectToTrigger, TriggerName);
            }

            Initialise();
        }

        private new void Initialise()
        {
            base.Initialise();
            if (keywordRecognizer != null)
            {
                keywordRecognizer.Stop();
                keywordRecognizer.Dispose();
            }

            keywords.Clear();
            keywords.Add(GetProperty<string>("password"), () =>
            {
                var focusObject = GazeGestureManager.Instance.FocusedObject;
                if (focusObject != null)
                    focusObject.SendMessage("OnCorrectPassword");
            });

            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
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
            OnSetVisible();
        }

        private void Update()
        {
        }
    }
}