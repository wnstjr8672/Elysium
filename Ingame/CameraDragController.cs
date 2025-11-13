using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragController : MonoBehaviour
{
    public float dragSpeed = 2f;
    [SerializeField] private float dragThreshold = 0.02f;

    [Header("카메라 이동 제한")]
    public float minX = -5f;
    public float maxX = 5f;

    private Vector3 dragOrigin;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseDrag();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchDrag();
#endif
    }

    void HandleMouseDrag()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 diff = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);

            if (Mathf.Abs(diff.x) < dragThreshold) return;

            Vector3 newPos = cam.transform.position + new Vector3(diff.x, 0, 0);
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            cam.transform.position = newPos;

            //  이동 후 기준점 갱신 → 순간이동 방지
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    void HandleTouchDrag()
    {
        if (EventSystem.current != null && Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                dragOrigin = cam.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 diff = dragOrigin - cam.ScreenToWorldPoint(touch.position);

                if (Mathf.Abs(diff.x) < dragThreshold) return;

                Vector3 newPos = cam.transform.position + new Vector3(diff.x, 0, 0);
                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                cam.transform.position = newPos;

                //  이동 후 기준점 갱신
                dragOrigin = cam.ScreenToWorldPoint(touch.position);
            }
        }
    }
}
