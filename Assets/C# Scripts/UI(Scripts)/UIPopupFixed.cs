using System.Collections;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;



[BurstCompile]
public class UIPopupFixed : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("The UIPopup object that will be displayed")]
    [SerializeField] protected RectTransform popupObj;


    [Header("Fade UIPopup in and out instead of toggling it")]
    [SerializeField] protected bool useFading = true;

    [Header("The time it takes for the UIPopup to fade in and out")]
    [SerializeField] protected float fadeTime = .15f;

    protected CanvasGroup canvasGroup;



    [BurstCompile]
    protected virtual void Start()
    {
        popupObj.gameObject.SetActive(false);

        canvasGroup = popupObj.GetComponent<CanvasGroup>();

        //if no canvasGroup is found, add it
        if (canvasGroup == null)
        {
            canvasGroup = popupObj.transform.AddComponent<CanvasGroup>();
        }
    }



    #region Mouse Hover Callbacks

    public void OnPointerEnter(PointerEventData _)
    {
        if (useFading)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI());
        }
        else
        {
            canvasGroup.alpha = 1;
            HardToggleUI(true);
        }
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (useFading)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutUI());
        }
        else
        {
            HardToggleUI(false);
        }
    }

    #endregion




    protected virtual void HardToggleUI(bool newState)
    {
        popupObj.gameObject.SetActive(newState);
    }


    #region Fade In/Out Popup Instead of hard toggling them

    [BurstCompile]
    protected virtual IEnumerator FadeInUI()
    {
        popupObj.gameObject.SetActive(true);

        float elapsed = 0;
        while (elapsed < fadeTime)
        {
            yield return null;

            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeTime;
        }

        canvasGroup.alpha = 1;
    }

    [BurstCompile]
    protected virtual IEnumerator FadeOutUI()
    {
        float elapsed = fadeTime;
        while (elapsed > 0)
        {
            yield return null;

            elapsed -= Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeTime;
        }

        canvasGroup.alpha = 0;
        popupObj.gameObject.SetActive(false);
    }

    #endregion
}