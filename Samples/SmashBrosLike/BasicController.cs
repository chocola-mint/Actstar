using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CHM.Actstar;
namespace CHM.Actstar.Samples.SmashBrosLike
{
    [RequireComponent(typeof(Mover), typeof(IJumper), typeof(Fastfaller))]
    public class BasicController : MonoBehaviour
    {
        public InputAction move, dash, jump, fastfall;
        private Mover mover;
        private IJumper jumper;
        private Fastfaller fastfaller;
        void Awake() 
        {        
            TryGetComponent<Mover>(out mover);
            TryGetComponent<IJumper>(out jumper);
            TryGetComponent<Fastfaller>(out fastfaller);
        }
        void OnEnable() 
        {
            move.Enable();
            dash.Enable();
            jump.Enable();
            dash.performed += OnDash;
            move.canceled += OnDash;
            jump.started += OnJump;
            jump.canceled += OnJump;
            fastfall.Enable();
            fastfall.started += OnFastfall;
            fastfall.canceled += OnFastfall;
        }
        void OnDisable() 
        {
            dash.performed -= OnDash;
            move.canceled -= OnDash;
            jump.started -= OnJump;
            jump.canceled -= OnJump;
            fastfall.started -= OnFastfall;
            fastfall.canceled -= OnFastfall;
            move.Disable();
            dash.Disable();
            jump.Disable();   
            fastfall.Disable();
        }
        void Update() 
        {
            mover.Move(move.ReadValue<float>());
        }
        private void OnJump(InputAction.CallbackContext ctx)
        {
            if(ctx.canceled) jumper.CancelJump();
            else jumper.Jump();
        }
        private void OnDash(InputAction.CallbackContext ctx)
        {
            if(ctx.performed) mover.StartDashing();
            else mover.StopDashing();
        }

        private void OnFastfall(InputAction.CallbackContext ctx)
        {
            if(ctx.canceled) fastfaller.CancelFastfall();
            else fastfaller.StartFastfall();
        }
    }
}