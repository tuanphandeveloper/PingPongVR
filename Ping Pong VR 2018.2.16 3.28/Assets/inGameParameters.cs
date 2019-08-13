using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class inGameParameters : MonoBehaviour
{
    //newly added ****************
    [SerializeField] Parameters parameters;
    [SerializeField] UnityEngine.UI.Text velocityValue;
    [SerializeField] UnityEngine.UI.Text angleVerticalValue;
    [SerializeField] UnityEngine.UI.Text angleHorizontalValue;
    [SerializeField] UnityEngine.UI.Text verticalSpinValue;
    [SerializeField] UnityEngine.UI.Text horizantalSpinValue;

    // Update is called once per frame
    void Update()
    {
        velocityValue.text = parameters.velocity.ToString();
        angleVerticalValue.text = parameters.angleVertical.ToString();
        angleHorizontalValue.text = parameters.angleHorizontal.ToString();
        verticalSpinValue.text = parameters.verticalSpin.ToString();
        horizantalSpinValue.text = parameters.horizontalSpin.ToString();
    }
}
