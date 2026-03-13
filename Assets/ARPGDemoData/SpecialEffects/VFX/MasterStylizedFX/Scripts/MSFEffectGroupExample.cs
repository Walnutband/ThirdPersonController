using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MSFEffectGroupExample : MonoBehaviour
{
    public List<GameObject> ParticleGroups;
    int CurIndex = 0;
    public GameObject CurParticleGroup;
    private void Start()
    {
        PlayEffects();
    }
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.A))
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            CurIndex -= 1;
            UpdateCurIndex();
        }
        // if (Input.GetKeyDown(KeyCode.D))
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            CurIndex += 1;
            UpdateCurIndex();
        }
  
        // if (Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0)))
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlayEffects();
        }
    }

    public void UpdateCurIndex()
    {
        if (CurIndex >= ParticleGroups.Count)
        {
            CurIndex = 0;
        }
        if (CurIndex < 0)
        {
            CurIndex = ParticleGroups.Count - 1;
        }
        PlayEffects();
    }

    public void PlayEffects()
    {
        DisableCurGroup();
        CurParticleGroup = ParticleGroups[CurIndex];
        PlayCurGroup();
    }

    public void PlayCurGroup()
    {
        CurParticleGroup.SetActive(true);
        var pars = CurParticleGroup.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < pars.Length; i++)
        {
            pars[i].Play();
        }
    }
    public void DisableCurGroup()
    {
        if (CurParticleGroup != null)
        {
            CurParticleGroup.SetActive(false);
        }
    }

}
