using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;



public class UIPopupFixed : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("The UIPopup object that will be displayed")]
    [SerializeField] protected GameObject popupObj;


    [Header("Fade UIPopup in and out instead of toggling it")]
    [SerializeField] protected bool useFading = true;

    [Header("The time it takes for the UIPopup to fade in and out")]
    [SerializeField] protected float fadeTime = .15f;

    protected CanvasGroup canvasGroup;



    protected virtual void Start()
    {
        popupObj.SetActive(false);

        canvasGroup = popupObj.GetComponent<CanvasGroup>();

        if (useFading && canvasGroup == null)
        {
            canvasGroup = popupObj.transform.AddComponent<CanvasGroup>();
        }
    }

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
            popupObj.SetActive(true);
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
            popupObj.SetActive(false);
        }
    }



    protected virtual IEnumerator FadeInUI()
    {
        popupObj.SetActive(true);

        float elapsed = 0;
        while (elapsed < fadeTime)
        {
            yield return null;

            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeTime;
        }

        canvasGroup.alpha = 1;
    }

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
        popupObj.SetActive(false);
    }
}