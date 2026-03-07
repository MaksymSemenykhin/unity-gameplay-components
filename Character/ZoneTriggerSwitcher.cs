using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone trigger (Collider2D must be trigger) that reacts to Player enter/exit:
/// enables or disables given GameObjects and fades SpriteRenderer alpha (in on enter, out on exit).
/// Optional side filter: react only when player enters from left/right or exits to left/right (by comparing collider centers on X).
/// </summary>
[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Zone Trigger Switcher")]
public class ZoneTriggerSwitcher : MonoBehaviour
{
    /// <summary>
    /// Restricts when the trigger reacts based on player position relative to zone center.
    /// </summary>
    public enum SideFilter
    {
        /// <summary>React regardless of side.</summary>
        Any,
        /// <summary>Only when player is on the left of zone center (center.x less than zone center).</summary>
        LeftOnly,
        /// <summary>Only when player is on the right of zone center.</summary>
        RightOnly
    }

    [Header("Enable on enter, disable on exit")]
    [Tooltip("GameObjects to SetActive(true) on enter and SetActive(false) on exit.")]
    [SerializeField] private GameObject[] elementsToEnableAndDisable;

    [Header("Side filter")]
    [Tooltip("Only react when player enters from this side. Any = ignore.")]
    [SerializeField] private SideFilter enterFrom = SideFilter.Any;
    [Tooltip("Only react when player exits to this side. Any = ignore.")]
    [SerializeField] private SideFilter exitTo = SideFilter.Any;

    [Header("Alpha fade on enter/exit")]
    [Tooltip("Targets to fade in (alpha 1) on enter and fade out (alpha 0) on exit. Each has its own duration. Target must have SpriteRenderer.")]
    [SerializeField] private AlphaFadeEntry[] alphaFadeTargets;

    /// <summary>
    /// One alpha fade target: GameObject with SpriteRenderer and optional duration (per target).
    /// </summary>
    [System.Serializable]
    public class AlphaFadeEntry
    {
        [Tooltip("Object with SpriteRenderer to fade.")]
        public GameObject target;
        [Min(0.01f)]
        [Tooltip("Fade duration in seconds.")]
        public float fadeDuration = 0.2f;
    }

    private List<Coroutine> _fadeCoroutines = new List<Coroutine>();
    private Collider2D _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Editor: ensure collider is set as trigger when component is added or reset.
    /// </summary>
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    /// <summary>On Player enter (and side filter): enable elements, fade in alpha targets.</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!PassesSideCheck(enterFrom, other)) return;
        SetElementsActive(true);
        ShowAlphaTargets();
    }

    /// <summary>On Player exit (and side filter): disable elements, fade out alpha targets.</summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!PassesSideCheck(exitTo, other)) return;
        SetElementsActive(false);
        HideAlphaTargets();
    }

    /// <summary>
    /// Sets active state for all elements in elementsToEnableAndDisable.
    /// </summary>
    private void SetElementsActive(bool active)
    {
        if (elementsToEnableAndDisable == null) return;
        foreach (var go in elementsToEnableAndDisable)
        {
            if (go != null) go.SetActive(active);
        }
    }

    private void ShowAlphaTargets() => ApplyAlphaTargets(1f);
    private void HideAlphaTargets() => ApplyAlphaTargets(0f);

    /// <summary>
    /// Stops all running fades, then starts fade to targetAlpha for each alphaFadeTargets entry.
    /// </summary>
    /// <param name="targetAlpha">1 = fade in, 0 = fade out.</param>
    private void ApplyAlphaTargets(float targetAlpha)
    {
        StopAllFades();
        if (alphaFadeTargets == null) return;
        foreach (var entry in alphaFadeTargets)
        {
            if (entry?.target == null) continue;
            _fadeCoroutines.Add(StartCoroutine(FadeAlpha(entry.target, entry.fadeDuration, targetAlpha)));
        }
    }

    /// <summary>
    /// True if filter is Any, or if other's center X matches the filter (left/right of zone center).
    /// </summary>
    private bool PassesSideCheck(SideFilter filter, Collider2D other)
    {
        if (filter == SideFilter.Any) return true;
        if (_triggerCollider == null) _triggerCollider = GetComponent<Collider2D>();
        bool isLeft = other.bounds.center.x < _triggerCollider.bounds.center.x;
        return filter == SideFilter.LeftOnly ? isLeft : !isLeft;
    }

    /// <summary>
    /// Stops all active fade coroutines and clears the list.
    /// </summary>
    private void StopAllFades()
    {
        foreach (var c in _fadeCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        _fadeCoroutines.Clear();
    }

    /// <summary>
    /// Coroutine: lerps SpriteRenderer alpha from current to targetAlpha over duration. Exits if target or SpriteRenderer is missing or destroyed.
    /// </summary>
    /// <param name="target">GameObject with SpriteRenderer.</param>
    /// <param name="duration">Duration in seconds (min 0.01).</param>
    /// <param name="targetAlpha">Target alpha (0 or 1).</param>
    private IEnumerator FadeAlpha(GameObject target, float duration, float targetAlpha)
    {
        var sr = target != null ? target.GetComponent<SpriteRenderer>() : null;
        if (sr == null) yield break;

        duration = Mathf.Max(duration, 0.01f);
        float startAlpha = sr.color.a;
        float t = 0f;

        while (t < 1f)
        {
            if (target == null) yield break;
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(t));
            SetSpriteAlpha(sr, a);
            yield return null;
        }
        if (target != null) SetSpriteAlpha(sr, targetAlpha);
    }

    /// <summary>
    /// Sets alpha on the SpriteRenderer's color.
    /// </summary>
    private static void SetSpriteAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        var c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}
