using UnityEngine;
using System.Collections;

public class MainCameraControl : MonoBehaviour {

    public bool allowZoom = true;
    public bool allowRotate = true;
    public bool allowPan = true;

    //Vector3 defaultPosition;

    float defaultRot;
    float targetRot = 0f;
    float rotationStep = 45f;
    float rotationSpeed = 2f;

    float currentZoom;
    float targetZoom;
    float zoomStep = 20f;
    float zoomSpeed = 5f;
    float minZoom = 15f;
    float maxZoom = 100f;

    public GameObject cameraCenter;
    public Camera cam;

	// Use this for initialization
	void Start () {
        
        //defaultPosition = transform.position;
        defaultRot = cameraCenter.transform.rotation.z;

        currentZoom = cam.fieldOfView;
        targetZoom = currentZoom;

	}
	
	// Update is called once per frame
	void Update () {

        if (allowRotate)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                targetRot -= rotationStep;
                StartCoroutine("RotateToTarget");
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                targetRot += rotationStep;
                StartCoroutine("RotateToTarget");
            }
        }

        if (allowZoom)
        {
            if(Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                targetZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomStep;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }

        }

        if(Mathf.Abs(currentZoom - targetZoom) > 0.1)
        {
            Zoom();
        }
	}


    IEnumerator RotateToTarget()
    {
        float currentRot = cameraCenter.transform.rotation.eulerAngles.z;
        float angleLeft = Mathf.Abs(currentRot - targetRot);
        while (angleLeft >= 0.1f)
        {
            //Debug.Log("CurrentRot: " + currentRot + "  Target: " + targetRot + "   Remaining: " + angleLeft);
            float angle = Mathf.LerpAngle(currentRot, targetRot, rotationSpeed * Time.deltaTime);
            //Debug.Log("lerp " + angle);

            cameraCenter.transform.eulerAngles = new Vector3(0f, 0f, angle);
            currentRot = cameraCenter.transform.rotation.eulerAngles.z;
            angleLeft = Mathf.Abs(currentRot - targetRot);
            yield return null;
        }
        yield return null;
    }

    void Zoom()
    {
        
        float zoom = Mathf.Lerp(currentZoom, targetZoom, zoomSpeed * Time.deltaTime);
        
        cam.fieldOfView = zoom;
        currentZoom = cam.fieldOfView;
    }

    void ResetToDefault()
    {
        cameraCenter.transform.eulerAngles = new Vector3 (0f,0f,defaultRot);
        
    }

    public float GetYRotFromVec(Vector2 v1, Vector2 v2)
    {
        float _r = Mathf.Atan2(v1.x - v2.x, v1.y - v2.y);
        float _d = (_r / Mathf.PI) * 180;

        return _d;

    }

    public void DisableCameraControls()
    {
        allowZoom = false;
        allowRotate = false;
        allowPan = false;
    }
    public void EnableCameraControls()
    {
        allowZoom = true;
        allowRotate = true;
        allowPan = true;
    }
}
