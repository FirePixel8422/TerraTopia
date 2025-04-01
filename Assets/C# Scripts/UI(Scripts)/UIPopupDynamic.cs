using System.Collections;
using Unity.VisualScripting;
using UnityEngine;



public class UIPopupDynamic : UIPopupFixed
{

    protected override IEnumerator FadeInUI()
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

    protected override IEnumerator FadeOutUI()
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