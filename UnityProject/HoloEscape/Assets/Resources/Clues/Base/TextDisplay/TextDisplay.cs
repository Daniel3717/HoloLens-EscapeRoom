using UnityEngine;

namespace Clues.Base.TextDisplay
{
    public class TextDisplay : Clue
    {
        // Use this for initialization

        public string text1;
        public string text2;
        public string initialText;

        private void Start()
        {
            if (!IsPropertySet("text1"))
            {
                SetProperty("text1", text1);
            }
            if (!IsPropertySet("text2"))
            {
                SetProperty("text2", text2);
            }
            if (!IsPropertySet("initialText"))
            {
                SetProperty("initialText", initialText);
            }
            Initialise();
        }

        public new void Initialise()
        {
            base.Initialise();

            GetComponent<TextMesh>().text = GetProperty<string>("initialText");
        }

        private void OnTrigger1()
        {
            GetComponent<TextMesh>().text = GetProperty<string>("text1");
        }

        private void OnTrigger2()
        {
            GetComponent<TextMesh>().text = GetProperty<string>("text2");
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}