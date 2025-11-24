using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


public abstract class ScreenTransition : MonoBehaviour
{
    public abstract Task TriggerAsync(Action moveCamera);
}
