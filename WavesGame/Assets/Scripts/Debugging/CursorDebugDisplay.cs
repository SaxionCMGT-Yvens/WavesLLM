using Core;
using TMPro;
using UnityEngine;
using UUtils;

namespace Debugging
{
    public class CursorDebugDisplay : MonoBehaviour
    {
        [SerializeField]
        private CursorController cursorController;
        [SerializeField]
        private TextMeshProUGUI cursorIndexText;

        private void Awake()
        {
            if (!GameManager.GetSingleton().GetSettings().debugCursorInformation)
            {
                Destroy(this);
                return;
            }
            
            AssessUtils.CheckRequirement(ref cursorController, this);
            AssessUtils.CheckRequirement(ref cursorIndexText, this);
        }

        private void Update()
        {
            cursorIndexText.text = cursorController.GetIndex().ToString();
        }
    }
}
