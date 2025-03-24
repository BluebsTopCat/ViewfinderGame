using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SkyboxCamera : MonoBehaviour
{
    public float skyboxScale = .05f;

    private Camera _camera;
    private Camera _mainCamera;

    // Update is called once per frame
    private void Start()
    {
        setup();
    }

    void LateUpdate()
    {

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            return;
        }

        _camera.fieldOfView = _mainCamera.fieldOfView;
        transform.localPosition = _mainCamera.transform.position * skyboxScale;
        transform.rotation = _mainCamera.transform.rotation;
    }

    void setup()
    {
        _camera = this.GetComponent<Camera>();
        _mainCamera = Camera.main;
    }
}
