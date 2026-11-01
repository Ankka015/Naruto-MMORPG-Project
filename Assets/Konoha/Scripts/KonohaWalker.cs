using UnityEngine;
using UnityEngine.InputSystem;

public class KonohaWalker : MonoBehaviour
{
    public float speed=6f, gravity=20f; float vertical;
    void Update(){var k=Keyboard.current;if(k==null)return;var c=GetComponent<CharacterController>();float x=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),z=(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0);var f=Camera.main?Camera.main.transform.forward:Vector3.forward;f.y=0;f.Normalize();var r=Camera.main?Camera.main.transform.right:Vector3.right;r.y=0;r.Normalize();var move=f*z+r*x;if(move.sqrMagnitude>1)move.Normalize();c.Move(move*speed*Time.deltaTime);if(c.isGrounded&&vertical<0)vertical=-1;vertical-=gravity*Time.deltaTime;c.Move(Vector3.up*vertical*Time.deltaTime);if(move.sqrMagnitude>.02f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),12*Time.deltaTime);}
}
public class KonohaFollowCamera : MonoBehaviour
{
    public Transform target; float yaw; void LateUpdate(){if(!target)return;var mouse=Mouse.current;if(mouse!=null){var delta=mouse.delta.ReadValue();yaw+=delta.x*.06f;}float pitch=12;pitch=Mathf.Clamp(pitch,-5,55);var rot=Quaternion.Euler(pitch,yaw,0);transform.position=target.position+rot*new Vector3(0,3.5f,-6.5f);transform.LookAt(target.position+Vector3.up*1.25f);}
}
