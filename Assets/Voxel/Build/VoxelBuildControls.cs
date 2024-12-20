using System;
using UnityEngine;

/// <summary>
/// Basic Camera mover script
/// </summary>
public class VoxelBuildControls : MonoBehaviour
{
    public VoxelBuildSystem voxelBuildSystem;
    public float moveSpeed = 5f;      // Speed of camera movement
    public float lastSpeed = 0f;      // Speed of camera movement
    public float rotateSpeed = 2f;   // Speed of camera rotation

    private Vector2 movementInput;
    private Vector2 mouseInput;
    private float scrollInput;
    
    public Controls controls;

    [Serializable]
    public class Controls {
        public bool invertLookY;
        public float mouseSensitivityX = 2f;
        public float mouseSensitivityY = 2f;

        public string axisLookHorzizontal = "Mouse X";              // Mouse to Look
        public string axisLookVertical    = "Mouse Y";              // 
        public string axisMoveHorzizontal = "Horizontal";           // WASD to Move
        public string axisMoveVertical    = "Vertical";             // 
        public string axisZoom            = "Mouse ScrollWheel";    // Mouse Scroll Wheel to Zoom
        public KeyCode keyAdd             = KeyCode.Mouse0;         // Left Click to Add
        public KeyCode keyRemove          = KeyCode.Mouse1;         // Right Click to Remove
        public KeyCode keyMoveUp          = KeyCode.E;         // E to Move Up
        public KeyCode keyMoveDown        = KeyCode.Q;         // Q to Move Down

        public float inputLookX        = 0;      //
        public float inputLookY        = 0;      //
        public float inputMoveX        = 0;      // range -1f to +1f
        public float inputMoveY        = 0;      // range -1f to +1f
        public float inputZoom         = 0;      // range -1f to +1f

        public bool isAddPressed        = false;  // is key Held
        public bool isRemovedPressed     = false;  // is key Held
        public bool isMoveUpPressed     = false;  // is key Held
        public bool isMoveDownPressed   = false;  // is key Held
        public float accMouseX;
        public float accMouseY;
        public float vertRotation;

        public void ReadInput(){
            
            inputLookX = Input.GetAxis( axisLookHorzizontal );
            inputLookY = Input.GetAxis( axisLookVertical );
            inputMoveX = Input.GetAxis( axisMoveHorzizontal );
            inputMoveY = Input.GetAxis( axisMoveVertical );
            inputZoom = Input.GetAxis( axisZoom );
            if(invertLookY){
                inputLookY = -inputLookY;
            }

            isAddPressed   = Input.GetKey( keyAdd );
            isRemovedPressed = Input.GetKey( keyRemove );
            isMoveUpPressed = Input.GetKey( keyMoveUp );
            isMoveDownPressed = Input.GetKey( keyMoveDown );
        }

    }


    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        ReadInput();

        HandleMovement();
        ProcessLook();
        HandleAddRemove();
        ProcessZoom();
    }
    private void ReadInput()
    {
        controls.ReadInput();

        // Read input
        movementInput = new Vector2(controls.inputMoveX, controls.inputMoveY);
        mouseInput = new Vector2(controls.inputLookX, controls.inputLookY);
        scrollInput = controls.inputZoom;
    }

    private void HandleAddRemove()
    {
        if (controls.isAddPressed)
        {
            voxelBuildSystem.AddVoxel();
        }
        if (controls.isRemovedPressed)
        {
            voxelBuildSystem.RemoveVoxel();
        }
    }
    private void HandleMovement()
    {
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        if ( move.magnitude > 1f )
            move = move.normalized;

        float nextSpeed = moveSpeed;
        float speed;
        float lerpFactor = lastSpeed > nextSpeed ? 4f : 2f;
        speed = Mathf.Lerp( lastSpeed, nextSpeed, lerpFactor * Time.fixedDeltaTime );
        lastSpeed = speed;
        var calc = move * speed * Time.fixedDeltaTime;
        
        // Up and down movement
        if (controls.isMoveDownPressed) // Move down
            calc += Vector3.down * moveSpeed * Time.deltaTime;
        if (controls.isMoveUpPressed) // Move up
            calc += Vector3.up * moveSpeed * Time.deltaTime;

        transform.Translate( calc );
    }

    void ProcessLook()
    {
        controls.accMouseX = Mathf.Lerp( controls.accMouseX, controls.inputLookX, 20 * Time.fixedDeltaTime );
        controls.accMouseY = Mathf.Lerp( controls.accMouseY, controls.inputLookY, 20 * Time.fixedDeltaTime );

        float mouseX = controls.accMouseX * controls.mouseSensitivityX * 100f * Time.fixedDeltaTime;
        float mouseY = controls.accMouseY * controls.mouseSensitivityY * 100f * Time.fixedDeltaTime;

        transform.Rotate( Vector3.up, mouseX, Space.World );
        transform.Rotate( Vector3.right, -mouseY, Space.Self );
    }

    void ProcessZoom()
    {
        // Zoom in and out
        float scroll = scrollInput * 200f * Time.deltaTime;
        transform.Translate(Vector3.forward * scroll, Space.Self);
    }

}
