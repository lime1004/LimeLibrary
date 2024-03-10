using UnityEngine.InputSystem;
using NotImplementedException = System.NotImplementedException;

namespace LimeLibrary.UI
{
    public class UICommonInput : IUICommonInput
    {
        private readonly InputAction _decideInputAction;
        private readonly InputAction _cancelInputAction;

        public UICommonInput()
        {
            _decideInputAction = new InputAction("Decide", InputActionType.Button);
            _decideInputAction.AddBinding("<Gamepad>/buttonSouth");
            _cancelInputAction = new InputAction("Cancel", InputActionType.Button);
            _cancelInputAction.AddBinding("<Gamepad>/buttonEast");
            _cancelInputAction.AddBinding("<Keyboard>/escape");
        }

        public InputAction GetDecideInputAction() => _decideInputAction;
        public InputAction GetCancelInputAction() => _cancelInputAction;
    }
}