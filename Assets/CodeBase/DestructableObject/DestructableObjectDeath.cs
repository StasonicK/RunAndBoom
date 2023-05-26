﻿using System;
using System.Collections;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Logic;
using CodeBase.Services.PersistentProgress;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.DestructableObject
{
    [RequireComponent(typeof(AudioSource))]
    public class DestructableObjectDeath : MonoBehaviour, IDeath, IProgressReader
    {
        [SerializeField] private GameObject _solid;
        [SerializeField] private GameObject _broken;
        [SerializeField] private DestructableTypeId _typeId;

        private const float DestroyColliderTimer = 0.1f;

        private AudioSource _audioSource;
        private float _deathDelay = 50f;
        private bool _isBroken = false;
        private List<Rigidbody> _parts;
        private PlayerProgress _progress;
        private float _volume;

        public event Action Died;

        private void Awake()
        {
            _solid.SetActive(true);
            _broken.SetActive(false);
            _parts = new List<Rigidbody>(_broken.transform.childCount);
            _audioSource = GetComponent<AudioSource>();

            for (int i = 0; i < _broken.transform.childCount; i++)
                _parts.Add(_broken.transform.GetChild(i).GetComponent<Rigidbody>());
        }

        public void Die()
        {
            _solid.SetActive(false);
            _broken.SetActive(true);
            Destroy(_solid.GetComponentInChildren<BoxCollider>());
            Destroy(_broken.GetComponentInChildren<BoxCollider>(), DestroyColliderTimer);

            if (_isBroken == false)
                foreach (Rigidbody part in _parts)
                    part.AddForce(part.gameObject.transform.forward * 5f, ForceMode.Impulse);

            PlaySound();
            _isBroken = true;

            StartCoroutine(DestroyTimer());
        }

        private void PlaySound()
        {
            switch (_typeId)
            {
                case DestructableTypeId.WoodenBox:
                    SoundInstance.InstantiateOnTransform(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.DestructionWoodenBox),
                        transform: transform, _volume, _audioSource);
                    break;
                case DestructableTypeId.Concrete:
                    SoundInstance.InstantiateOnTransform(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.DestructionConcrete),
                        transform: transform, _volume, _audioSource);
                    break;
            }
        }

        private IEnumerator DestroyTimer()
        {
            yield return new WaitForSeconds(_deathDelay);
            Destroy(gameObject);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            _progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
        }

        private void VolumeChanged() =>
            _volume = _progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            _volume = _progress.SettingsData.SoundOn ? _progress.SettingsData.SoundVolume : Constants.Zero;
    }
}