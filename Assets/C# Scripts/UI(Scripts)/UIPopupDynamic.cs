using System.Collections;
using Unity.Burst;
using UnityEngine;



[BurstCompile]
public class UIPopupDynamic : UIPopupFixed
{
    private Canvas canvas;
    private RectTransform rectTransform;


    [BurstCompile]
    protected override void Start()
    {
        base.Start();

        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();


        canvasGroup.alpha = 0;
        popupObj.gameObject.SetActive(true);

        SetPopupToMouse();
        KeepPopupOnScreen();

        popupObj.gameObject.SetActive(false);
        canvasGroup.alpha = 1;
    }

    
    [BurstCompile]
    protected override IEnumerator FadeInUI()
    {
        SetPopupToMouse();
        KeepPopupOnScreen();

        yield return base.FadeInUI();
    }


    [BurstCompile]
    private void SetPopupToMouse()
    {
        // Convert target image position to local space
        Vector2 targetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            rectTransform.position,
            canvas.worldCamera, 
            out targetPos
        );


        // Apply new position to the popup
        popupObj.pivot = new Vector2(0.5f, 0f);

        popupObj.anchorMin = new Vector2(0.5f, 0.5f);
        popupObj.anchorMax = new Vector2(0.5f, 0.5f);

        popupObj.anchoredPosition = new Vector2(targetPos.x, targetPos.y + popupObj.rect.height * 0.5f + rectTransform.rect.height * 0.5f);
    }


    [BurstCompile]
    private void KeepPopupOnScreen()
    {
        Vector3[] corners = new Vector3[4];
        popupObj.GetWorldCorners(corners);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 position = popupObj.position;
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, position);

        float popupWidth = corners[2].x - corners[0].x;
        float popupHeight = corners[2].y - corners[0].y;

        // Adjust position if popup goes off-screen
        position.x = Mathf.Clamp(position.x, popupWidth / 2, screenWidth - popupWidth / 2);
        position.y = Mathf.Clamp(position.y, popupHeight / 2, screenHeight - popupHeight / 2);

        popupObj.position = position;
    }
}