using UnityEngine;

/// <summary>
/// Placed at the metro exit door (same spot the player first walked out of).
/// When the player enters this trigger at the end → shows title + game ends.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string titleText = "Between Stops";
    [SerializeField] private float  titleHoldSeconds = 4f;

    private bool _fired;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        gameObject.SetActive(false); // disabled until EndingSequencer enables it
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fired || !other.CompareTag(playerTag)) return;
        _fired = true;
        StartCoroutine(EndRoutine());
    }

    private System.Collections.IEnumerator EndRoutine()
    {
        var fader = ScreenFader.Instance;
        var fp    = FindAnyObjectByType<FPController_CC>();
        if (fp != null) fp.enabled = false;

        if (fader != null)
        {
            fader.FadeOut(1.5f, onComplete: () => fader.ShowTitle(titleText));
        }

        yield return new WaitForSeconds(titleHoldSeconds + 1.5f);

        // Optionally quit or return to main menu — for now just stay on title
    }
}
