using UnityEngine;
using UnityEngine.UI;

namespace WeGotOne
{
    public class ColorPicker : MonoBehaviour
    {
        public Image circlePalette;
        public Image picker;
        public Color selectedColor = Color.white;
        public SpriteRenderer[] sprites;

        private Vector2 sizeOfPalette;
        private CircleCollider2D paletteCollider;

        void Start()
        {
            paletteCollider = circlePalette.GetComponent<CircleCollider2D>();

            sizeOfPalette = new Vector2(
                circlePalette.GetComponent<RectTransform>().rect.width, 
                circlePalette.GetComponent<RectTransform>().rect.height);
        }

        public void mousePointerDown()
        {
            selectColor();
            SetColor();
        }

        public void mouseDrag()
        {
            selectColor();
            SetColor();
        }

        public void SetColor()
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].color = selectedColor;
            }
        }

        private void selectColor()
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(SimpleInput.GetMousePosition());

            // CircleCollider2D 범위 제한
            Vector2 center = paletteCollider.transform.TransformPoint(
                paletteCollider.offset
            );

            float radius = paletteCollider.radius *
                           Mathf.Max(
                               paletteCollider.transform.lossyScale.x,
                               paletteCollider.transform.lossyScale.y
                           );

            Vector2 direction = mouseWorldPos - center;

            if (direction.magnitude > radius)
            {
                mouseWorldPos = center + direction.normalized * radius;
            }

            // 기존 코드 그대로
            picker.transform.position = mouseWorldPos;

            RectTransform paletteRect = circlePalette.rectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                paletteRect,
                mouseWorldPos,
                null,
                out Vector2 localPos
            );

            float u = Mathf.InverseLerp(
                paletteRect.rect.xMin,
                paletteRect.rect.xMax,
                localPos.x
            );

            float v = Mathf.InverseLerp(
                paletteRect.rect.yMin,
                paletteRect.rect.yMax,
                localPos.y
            );

            Texture2D texture = circlePalette.mainTexture as Texture2D;

            selectedColor = texture.GetPixelBilinear(u, v);
            selectedColor.a = 1f;
        }
    }
}