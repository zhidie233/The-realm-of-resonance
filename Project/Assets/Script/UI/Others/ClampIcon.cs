using UnityEngine;

namespace Game.Runtime.UI
{
    public class ClampIcon : MonoBehaviour
    {
        public bool isStatic = true;
        public bool isPooled = false;
        public Texture2D Icon;
        public Vector3 Offset;
        public float size = 50;
        public float timeToLive = 10;
        public Color m_Color = Color.white;

        private Transform ThisTransform;
        Vector3 viewportPoint;
        Vector2 drawPosition;
        float clampBorder = 12;
        bool visible = true;

        private void Awake()
        {
            ThisTransform = transform;
            visible = IsVisible(ThisTransform.position + Offset);
        }

        private void OnEnable()
        {
            ThisTransform = transform;
            if (isPooled) Invoke(nameof(Disable), timeToLive);
        }

        void OnGUI()
        {
            if (isStatic)
            {
                if (!RoomMenu.Instance.isCursorLocked)
                    return;
            }
            if (GameController.Instance.CameraRendered == null)
                return;

            Vector3 position = ThisTransform.position + Offset;
            if (Time.frameCount % 30 == 0)
            {
                visible = IsVisible(position);
            }

            if (!visible) return;

            //计算图标应绘制位置的 2D 坐标
            viewportPoint = GameController.Instance.CameraRendered.WorldToViewportPoint(position);

            //viewportPoint 坐标范围是 0 到 1，这里需要转换到屏幕空间
            drawPosition = new Vector2(viewportPoint.x * Screen.width, Screen.height * (1 - viewportPoint.y));

            //把位置限制在屏幕边缘，防止图标绘制到屏幕外
            drawPosition.x = Mathf.Clamp(drawPosition.x, clampBorder, Screen.width - clampBorder);
            drawPosition.y = Mathf.Clamp(drawPosition.y, clampBorder, Screen.height - clampBorder);

            GUI.color = m_Color;
            GUI.DrawTexture(new Rect(drawPosition.x - size * 0.5f, drawPosition.y - size * 0.5f, size, size), Icon);
            GUI.color = Color.white;
        }

        private bool IsVisible(Vector3 fromPos)
        {
            if (GameController.Instance.CameraRendered == null) return false;

            Plane plane = new Plane(GameController.Instance.CameraRendered.transform.forward, GameController.Instance.CameraRendered.transform.position);
            return plane.GetSide(fromPos);
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}