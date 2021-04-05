using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToSpriteMirror : MonoBehaviour
{
    private SpriteRenderer _spriteToUpdate;
    private BoxCollider2D _boxCollider2d;
    private bool _spriteNotVisibleCleaning;
    private bool _entitiesNotCloseCleaning;
    [SerializeField] Camera MirrorCam;
    [SerializeField] float SpriteWidth;
    [SerializeField] float SpriteHeight;
    private Color _defaultSpriteColor;
    private short _processDrawCounter;

    void Start()
    {
        _spriteToUpdate = GetComponent<SpriteRenderer>();
        _defaultSpriteColor = _spriteToUpdate.color;
        _defaultSpriteColor.a = 0f;
        _boxCollider2d = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        if (!_spriteToUpdate.isVisible)
        {
            if(!_spriteNotVisibleCleaning)
            {
                _spriteNotVisibleCleaning = true;
                Resources.UnloadUnusedAssets();
                _defaultSpriteColor.a = 0f;
                _spriteToUpdate.color = _defaultSpriteColor;
            }
            return;
        }

        if (AreEntitiesCloseEnough())
        {

            DrawingProcess();
        }
    }

    bool AreEntitiesCloseEnough()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(_boxCollider2d.bounds.center, _boxCollider2d.bounds.size, 0f, 
                                                    Vector2.down, 0, (1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemies")));

        if(raycastHit.collider != null)
        {
            _spriteNotVisibleCleaning = false;
            _entitiesNotCloseCleaning = false;
            return true;
        }

        if(!_entitiesNotCloseCleaning)
        {
            _entitiesNotCloseCleaning = true;
            Resources.UnloadUnusedAssets();
            _defaultSpriteColor.a = 0f;
            _spriteToUpdate.color = _defaultSpriteColor;
        }

        return false;
    }


    void DrawingProcess()
    {
        _defaultSpriteColor.a = 1f;
        _spriteToUpdate.color = _defaultSpriteColor;

        //Get camera render texture
        RenderTexture rendText = RenderTexture.active;
        RenderTexture.active = MirrorCam.targetTexture;

        //Render that camera
        MirrorCam.Render();

        //Convert to Texture2D
        Texture2D text = RenderTextureToTexture2D(MirrorCam.targetTexture);

        RenderTexture.active = rendText;

        //Convert to Sprite
        Sprite sprite = Texture2DToSprite(text);

        //Apply to SpriteRenderer
        _spriteToUpdate.sprite = sprite;

        _processDrawCounter++;

        if(_processDrawCounter >= 200)
        {
            _processDrawCounter = 0;
            Resources.UnloadUnusedAssets();
        }
    }

    Texture2D RenderTextureToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, _spriteToUpdate.sprite.rect.width, _spriteToUpdate.sprite.rect.height), 0, 0);
        tex.Apply();
        return tex;
    }

    Sprite Texture2DToSprite(Texture2D text2D)
    {
        Sprite sprite = Sprite.Create(text2D, new Rect(0, 0, SpriteWidth, SpriteHeight), Vector2.zero);
        return sprite;
    }
}