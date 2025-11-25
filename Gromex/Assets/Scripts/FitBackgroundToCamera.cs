using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitBackgroundToCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        Fit();
    }

    private void Fit()
    {
        if (_camera == null || _spriteRenderer == null || _spriteRenderer.sprite == null)
            return;

        // World size of camera view
        float worldScreenHeight = _camera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * _camera.aspect;

        // World size of sprite
        Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;

        // Scale needed to cover full screen
        float scaleX = worldScreenWidth / spriteSize.x;
        float scaleY = worldScreenHeight / spriteSize.y;

        // "Cover" mode: беремо більше значення, щоб не було чорних полів
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }

#if UNITY_WEBGL
    private void Update()
    {
        // On WebGL browser window can change size at runtime.
        // Refit each frame (cheap enough for one sprite).
        Fit();
    }
#endif
}

