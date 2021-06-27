using UnityEngine;

public abstract class IBasePlayerTool : MonoBehaviour
{
    // Interface begin --
    public virtual void OnEnabledChanged( bool enabled ) { }
    public virtual bool OnMouse1( bool pressed ) { return false; }
    public virtual bool OnMouse2( bool pressed ) { return false; }
    public virtual bool OnMouseWheel( bool pressed ) { return false; }
    public virtual bool OnMouseWheelScroll( float axis ) { return false; }
    public virtual bool OnLook( Vector2 axis ) { return false; }
    public virtual bool OnSpecialAction( bool pressed ) { return false; }
    public virtual bool OnSpecialActionAlt( bool pressed ) { return false; }
    // Interface end --

    private void Start()
    {
        playerController = GetComponentInParent<FPSController>();
    }

    public void SetEnabled( bool enabled )
    {
        if( enabled != isEnabled )
        {
            isEnabled = enabled;
            OnEnabledChanged( isEnabled );
        }
    }

    public string GetName() { return toolName; }
    public Texture2D GetImage() { return toolDisplayImage; }

    protected bool isEnabled;

    [SerializeField]
    protected string toolName;

    [SerializeField]
    protected Texture2D toolDisplayImage;

    protected FPSController playerController;
}
