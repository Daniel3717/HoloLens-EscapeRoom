using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Windows.Speech;


namespace Clues
{
    public class VoicePad : Clue
    {

        KeywordRecognizer keywordRecognizer = null;
        Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

        public GameObject ObjectToTrigger;
        public string TriggerName;
        public string Password;
        public bool visible;

        GameObject WholeVoicePad;

        public GameObject ScreenText;


        void Start()
        {
            WholeVoicePad = GameObject.Find("VoicePad");

            if (!visible)
            {
                WholeVoicePad.transform.localScale = new Vector3(0, 0, 0);
            }

            ScreenText = GameObject.Find("voice_pad_text");
            ScreenText.GetComponent<TextMesh>().text = "Say Password";

            AddAction("OnCorrectPassword", ObjectToTrigger, TriggerName); 

            keywords.Add(Password, () =>
            {
                var focusObject = GazeGestureManager.Instance.FocusedObject;
                if (focusObject != null)
                {
                    // Call the method on just the focused object.
                    focusObject.SendMessage("OnCorrectPassword");
                }
            });

            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();

        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            System.Action keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        }

        void OnCorrectPassword()
        {
            Trigger("OnCorrectPassword");
            ScreenText.GetComponent<TextMesh>().text = "Correct!";
        }

        void OnAppear()
        {
            if (!visible)
            {
                WholeVoicePad.transform.localScale = new Vector3(1F, 1F, 1F);
                visible = true;
            }
        }

        void Update()
        {

        }
    }
}