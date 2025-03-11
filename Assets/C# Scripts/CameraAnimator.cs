using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraAnimator : MonoBehaviour
{
    [SerializeField] private CameraAnimationData[] cameraAnimationData;


    private void Start()
    {
        StartCoroutine(CameraAnimationLoop());
    }


    private IEnumerator CameraAnimationLoop()
    {
        int camAnimDataId = 0;

        float t = 0;
        Quaternion targetRotation = GetNewRotation(cameraAnimationData[camAnimDataId].toLookAtTransform.position);
        Quaternion camRot = transform.rotation;


        while (true)
        {
            yield return null;

            t += Time.deltaTime / cameraAnimationData[camAnimDataId].travelTime;


            // lerp to targetPos
            transform.rotation = Quaternion.Lerp(camRot, targetRotation, t);


            //when point has been reached
            if (t >= 1)
            {
                yield return new WaitForSeconds(cameraAnimationData[camAnimDataId].waitTime);

                camAnimDataId += 1;

                if(camAnimDataId == cameraAnimationData.Length)
                {
                    camAnimDataId = 0;
                }

                targetRotation = GetNewRotation(cameraAnimationData[camAnimDataId].toLookAtTransform.position);
                camRot = transform.rotation;

                t = 0;
            }
        }
    }


    private Quaternion GetNewRotation(Vector3 toLookAtPos)
    {
        // Get direction to target
        Vector3 direction = toLookAtPos - transform.position;
        direction.Normalize();

        // Get target rotation
        return Quaternion.LookRotation(direction);
    }



    private void OnDrawGizmos()
    {
        for (int i = 0; i < cameraAnimationData.Length; i++)
        {
            if(cameraAnimationData[i].toLookAtTransform.position == null)
            {
                continue;
            }

            Gizmos.DrawWireSphere(cameraAnimationData[i].toLookAtTransform.position, 0.5f);
        }
    }
}
