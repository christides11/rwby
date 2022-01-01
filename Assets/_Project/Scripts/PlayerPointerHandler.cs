using Rewired;
using Rewired.Integration.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace rwby
{
    public class PlayerPointerHandler : MonoBehaviour
    {
        public static PlayerPointerHandler singleton;

        [SerializeField] private float timeBeforeFadeout = 5.0f;
        [SerializeField] private RectTransform[] playerPointerImages;
        [SerializeField] private Rewired.Components.PlayerMouse[] playerMouses;

        public bool MiceHidden { private set; get; } = false;

        public Dictionary<int, float> miceTimers = new Dictionary<int, float>();

        private void Awake()
        {
            singleton = this;
        }

        void Start()
        {
            for(int i = 0; i < playerMouses.Length; i++)
            {
                int index = i;
                playerMouses[i].ScreenPositionChangedEvent += (data) => { OnPlayerPointerMoved(index, data); };
                playerPointerImages[i].GetComponentInChildren<Image>().color = Random.ColorHSV();
            }
        }

        List<int> removeTimers = new List<int>();
        private void FixedUpdate()
        {
            foreach(var v in miceTimers)
            {
                if (Time.realtimeSinceStartup - v.Value < timeBeforeFadeout) continue;
                removeTimers.Add(v.Key);
                playerPointerImages[v.Key].gameObject.SetActive(false);
            }

            foreach(int r in removeTimers)
            {
                miceTimers.Remove(r);
            }
        }

        private void OnPlayerPointerMoved(int index, Vector2 screenPosition)
        {
            if (MiceHidden == true) return;
            if (playerPointerImages[index].gameObject.activeSelf == false)
            {
                playerPointerImages[index].gameObject.SetActive(true);
            }
            if (!miceTimers.ContainsKey(index))
            {
                miceTimers.Add(index, 0);
            }
            miceTimers[index] = Time.realtimeSinceStartup;
        }

        public void HideMice(bool setHiddenState = true)
        {
            for(int i = 0; i < playerPointerImages.Length; i++)
            {
                playerPointerImages[i].gameObject.SetActive(false);
                playerMouses[i].gameObject.SetActive(false);
            }
            miceTimers.Clear();
            MiceHidden = setHiddenState ? true : MiceHidden;
        }

        public void ShowMice()
        {
            for (int i = 0; i < playerPointerImages.Length; i++)
            {
                playerPointerImages[i].gameObject.SetActive(false);
                playerMouses[i].gameObject.SetActive(true);
            }
            MiceHidden = false;
        }
    }
}