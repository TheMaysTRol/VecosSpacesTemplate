using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}

public class AnimateOnInput : MonoBehaviour
{
    public List<AnimationInput> animationInputs;
    public Animator animator;
    public CharacterController characterController;

    // Update is called once per frame
    void Update()
    {
        if (animator && characterController)
        {
            if (characterController.velocity.magnitude > 0.01f)
            {
                animator.SetInteger("Status", 7);
            }
            else
            {
                animator.SetInteger("Status", 0);
            }

        }
        foreach (var item in animationInputs)
        {
            if (item != null && item.action != null && item.action.action != null)
            {
                float actionValue = item.action.action.ReadValue<float>();
                animator.SetFloat(item.animationPropertyName, actionValue);
            }
        }
    }
}
